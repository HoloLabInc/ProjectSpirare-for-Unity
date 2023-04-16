using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HoloLab.Spirare
{
    internal sealed class ElementStore
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<int, ElementInfo> _descrToInfo = new Dictionary<int, ElementInfo>();
        private readonly Dictionary<PomlElementComponent, ElementInfo> _componentToInfo = new Dictionary<PomlElementComponent, ElementInfo>();

        // Reserve 0-9 values for special use.
        private const int initialElementDescriptor = 10;
        private DescriptorGenerator _descrGen = new DescriptorGenerator(initialElementDescriptor);

        public int ElementCount => _descrToInfo.Count;

        public void RegisterElement(PomlElementComponent elementComponent)
        {
            var descr = _descrGen.NewDescriptor();
            var info = new ElementInfo(elementComponent, descr);
            lock (_lockObj)
            {
                _descrToInfo.Add(descr, info);
                _componentToInfo.Add(elementComponent, info);
            }
        }

        public bool RemoveElement(PomlElementComponent elementComponent)
        {
            lock (_lockObj)
            {
                if (_componentToInfo.TryGetValue(elementComponent, out var info))
                {
                    _componentToInfo.Remove(elementComponent);
                    _descrToInfo.Remove(info.ElementDescriptor);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool RemoveElementByDescriptor(int elementDescriptor)
        {
            lock (_lockObj)
            {
                if (_descrToInfo.TryGetValue(elementDescriptor, out var info))
                {
                    _descrToInfo.Remove(elementDescriptor);
                    _componentToInfo.Remove(info.ElementComponent);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public PomlElementComponent[] GetAllElements()
        {
            // Since it is assumed that it will work asynchronously with lock,
            // evaluate it as an array instead of IEnumerable<T> and return it.
            lock (_lockObj)
            {
                return _componentToInfo.Keys.ToArray();
            }
        }

        public (int ElementDescriptor, PomlElementComponent Component)[] GetAllElementsWithDescriptor()
        {
            lock (_lockObj)
            {
                var array = _componentToInfo.Values.Select(x => (x.ElementDescriptor, x.ElementComponent)).ToArray();
                return array;
            }
        }

        public bool TryGetElementById(string id, out PomlElementComponent elementComponent)
        {
            lock (_lockObj)
            {
                elementComponent = _componentToInfo.Keys.FirstOrDefault(x => x.PomlElement.Id == id);
                return elementComponent != null;
            }
        }

        public bool TryGetElementById(string id, out PomlElementComponent elementComponent, out int descriptor)
        {
            lock (_lockObj)
            {
                var info = _descrToInfo.Values.FirstOrDefault(x => x.Id == id);
                if (info == null)
                {
                    elementComponent = null;
                    descriptor = -1;
                    return false;
                }
                elementComponent = info.ElementComponent;
                descriptor = info.ElementDescriptor;
                return true;
            }
        }

        public bool TryGetElementByDescriptor(int elementDescriptor, out PomlElementComponent elementComponent)
        {
            lock (_lockObj)
            {
                if (_descrToInfo.TryGetValue(elementDescriptor, out var info))
                {
                    elementComponent = info.ElementComponent;
                    return true;
                }
                elementComponent = null;
                return false;
            }
        }

        private sealed class ElementInfo
        {
            public PomlElementComponent ElementComponent { get; }
            public int ElementDescriptor { get; }
            public string Id => (ElementComponent != null) ? ElementComponent.PomlElement?.Id : null;

            public ElementInfo(PomlElementComponent elementComponent, int elementDescriptor)
            {
                ElementComponent = elementComponent;
                ElementDescriptor = elementDescriptor;
            }
        }

        private struct DescriptorGenerator
        {
            private int descr;

            public DescriptorGenerator(int initial)
            {
                descr = initial - 1;
            }

            public int NewDescriptor() => Interlocked.Increment(ref descr);
        }
    }
}
