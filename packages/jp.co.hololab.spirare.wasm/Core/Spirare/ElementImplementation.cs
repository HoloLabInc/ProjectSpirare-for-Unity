using System;
using System.Linq;
using System.Text;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace HoloLab.Spirare.Wasm.Core.Spirare
{
    public sealed class SpirareApiImpl
    {
        private readonly ElementDescriptorHelper _helper;

        public SpirareApiImpl(ElementDescriptorHelper elementDescriptorHelper)
        {
            _helper = elementDescriptorHelper;
        }

#pragma warning disable IDE1006 // naming style

        public int get_element_by_id(IntPtr memoryPtr, uint memoryLength, int idPtr, int idLength, int elementInfoPtr)
        {
            if (MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, idPtr, idLength, out var text) == false)
            {
                return (int)Errno.InvalidArgument;
            }

            if (_helper.TryGetElementById(text, out var elemComponent, out var elemDescr))
            {
                var info = new ElementInfo(elemDescr, elemComponent.PomlElement.ElementType);
                if (MemoryHelper.TryWrite(memoryPtr, memoryLength, elementInfoPtr, info))
                {
                    return (int)Errno.Success;
                }
                else
                {
                    return (int)Errno.InvalidArgument;
                }
            }

            MemoryHelper.TryWrite(memoryPtr, memoryLength, elementInfoPtr, ElementInfo.InvalidElement);
            return (int)Errno.ElementNotFound;
        }

        public int get_all_elements_len(IntPtr memoryPtr, uint memoryLength, int elementsNumPtr)
        {
            var num = _helper.ElementCount;
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, elementsNumPtr, num) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int get_all_elements(IntPtr memoryPtr, uint memoryLength, int elementInfoArrayPtr, int arrayLen, int writtenLenPtr)
        {
            var allInfo = _helper
                .GetAllElementsWithDescriptor()
                .Select(x => new ElementInfo(x.ElementDescriptor, x.Component.PomlElement.ElementType))
                .ToArray();
            var count = allInfo.Length;

            if (allInfo.Length > arrayLen)
            {
                return (int)Errno.InsufficientBufferSize;
            }

            if (MemoryHelper.TryWriteArray(memoryPtr, memoryLength, elementInfoArrayPtr, allInfo) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, writtenLenPtr, count) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int register_event(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int eventName, int data, EventCallbackState callbackState)
        {
            if (EnumUtility.TryToEnum<EventName>(eventName, out var eventEnum) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            if (_helper.TryGetElement<PomlElement>(elementDescriptor, out var elementComponent, out _, out var error) == false)
            {
                return error;
            }

            var actualElementDescriptor = elementDescriptor;    // TODO:
            var elementType = elementComponent.PomlElement.ElementType;
            Action<EventName> onEvent = eventName =>
            {
                callbackState.Callback(actualElementDescriptor, (int)elementType, (int)eventName, data);
            };
            switch (eventEnum)
            {
                case EventName.Update:
                    elementComponent.OnUpdate += _ => onEvent(eventEnum);
                    break;
                case EventName.Start:
                    return (int)Errno.NotImplemented;
                case EventName.Select:
                    if (elementComponent is PomlObjectElementComponent objElemComponent)
                    {
                        objElemComponent.OnSelect += () => onEvent(eventEnum);
                    }
                    else
                    {
                        return (int)Errno.UnsupportedOperation;
                    }
                    break;
                default:
                    break;
            }
            return (int)Errno.Success;
        }

        public int get_position(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int positionPtr)
        {
            return _helper.GetAttribute(elementDescriptor, (element) =>
            {
                var position = element.Position;
                if (MemoryHelper.TryWrite(memoryPtr, memoryLength, positionPtr, position) == false)
                {
                    return Errno.InvalidArgument;
                }

                return Errno.Success;
            });
        }

        public int set_position(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int positionPtr)
        {
            return _helper.SetAttribute(elementDescriptor, (element) =>
            {
                if (MemoryHelper.TryRead(memoryPtr, memoryLength, positionPtr, out Vector3 position) == false)
                {
                    return Errno.InvalidArgument;
                }

                element.Position = position;
                return Errno.Success;
            });
        }

        public int get_rotation(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int rotationPtr)
        {
            return _helper.GetAttribute(elementDescriptor, (element) =>
            {
                var rot = element.Rotation;
                if (MemoryHelper.TryWrite(memoryPtr, memoryLength, rotationPtr, rot) == false)
                {
                    return Errno.InvalidArgument;
                }

                return Errno.Success;
            });
        }

        public int set_rotation(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int rotationPtr)
        {
            return _helper.SetAttribute(elementDescriptor, (element) =>
            {
                if (MemoryHelper.TryRead(memoryPtr, memoryLength, rotationPtr, out Quaternion rot) == false)
                {
                    return Errno.InvalidArgument;
                }

                element.Rotation = rot;
                return Errno.Success;
            });
        }

        public int get_scale(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int scalePtr)
        {
            return _helper.GetAttribute(elementDescriptor, (element) =>
            {
                var scale = element.Scale;
                if (MemoryHelper.TryWrite(memoryPtr, memoryLength, scalePtr, scale) == false)
                {
                    return Errno.InvalidArgument;
                }

                return Errno.Success;
            });
        }

        public int set_scale(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int scalePtr)
        {
            return _helper.SetAttribute(elementDescriptor, (element) =>
            {
                if (MemoryHelper.TryRead(memoryPtr, memoryLength, scalePtr, out Vector3 scale) == false)
                {
                    return Errno.InvalidArgument;
                }

                element.Scale = scale;
                return Errno.Success;
            });
        }

        public int get_position_from(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int referenceElementDescriptor, int positionPtr)
        {
            int errorCode;
            if (_helper.TryGetElement<PomlElement>(elementDescriptor, out var elementComponent, out var element, out errorCode) == false)
            {
                return errorCode;
            }
            if (_helper.TryGetElement<PomlElement>(referenceElementDescriptor, out var referenceComponent, out var reference, out errorCode) == false)
            {
                return errorCode;
            }
            var worldToLocal = referenceComponent.transform.worldToLocalMatrix;
            var elementWorldPos = elementComponent.transform.position;
            var localPos = worldToLocal.MultiplyPoint(elementWorldPos);
            localPos = CoordinateUtility.ToSpirareCoordinate(localPos, true);
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, positionPtr, localPos) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int get_rotation_from(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int referenceElementDescriptor, int rotationPtr)
        {
            int errorCode;
            if (_helper.TryGetElement<PomlElement>(elementDescriptor, out var elementComponent, out var element, out errorCode) == false)
            {
                return errorCode;
            }
            if (_helper.TryGetElement<PomlElement>(referenceElementDescriptor, out var referenceComponent, out var reference, out errorCode) == false)
            {
                return errorCode;
            }
            var rot = elementComponent.transform.rotation * Quaternion.Inverse(referenceComponent.transform.rotation);
            rot = CoordinateUtility.ToSpirareCoordinate(rot);
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, rotationPtr, rot) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        [Obsolete("not implemented yet", true)]
        public int get_scale_from(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int referenceElementDescriptor, int scalePtr)
        {
            int errorCode;
            if (_helper.TryGetElement<PomlElement>(elementDescriptor, out var elementComponent, out var element, out errorCode) == false)
            {
                return errorCode;
            }
            if (_helper.TryGetElement<PomlElement>(referenceElementDescriptor, out var referenceComponent, out var reference, out errorCode) == false)
            {
                return errorCode;
            }
            var scale = elementComponent.transform.lossyScale;
            var referenceScale = referenceComponent.transform.lossyScale;

            throw new NotImplementedException();

            //scale.x /= referenceScale.x;
            //scale.y /= referenceScale.y;
            //scale.z /= referenceScale.z;
            //if (MemoryHelper.TryWrite(memoryPtr, memoryLength, scalePtr, scale) == false)
            //{
            //    return (int)Errno.InvalidArgument;
            //}
            //return (int)Errno.Success;
        }

        public int get_display(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int displayPtr)
        {
            return _helper.GetAttribute(elementDescriptor, (element) =>
            {
                var display = element.Display;
                if (MemoryHelper.TryWrite(memoryPtr, memoryLength, displayPtr, display) == false)
                {
                    return Errno.InvalidArgument;
                }

                return Errno.Success;
            });
        }

        public int set_display(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int display)
        {
            return _helper.SetAttribute(elementDescriptor, (element) =>
            {
                if (EnumUtility.TryToEnum(display, out PomlDisplayType pomlDisplayType) == false)
                {
                    return Errno.InvalidArgument;
                }
                element.Display = pomlDisplayType;
                return Errno.Success;
            });
        }

        public int get_id_len(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int idLengthPtr)
        {
            return _helper.GetAttribute(elementDescriptor, (element) =>
            {
                var id = element.Id;
                if (MemoryHelper.TryWrite<int>(memoryPtr, memoryLength, idLengthPtr, id.Length) == false)
                {
                    return Errno.InvalidArgument;
                }
                return Errno.Success;
            });
        }

        public int get_id(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int idPtr, int idLength)
        {
            return _helper.GetAttribute(elementDescriptor, (element) =>
            {
                uint offset = (uint)idPtr;
                var id = element.Id;
                if (idLength < id.Length)
                {
                    return Errno.InsufficientBufferSize;
                }
                if (MemoryHelper.TryWriteUtf8(memoryPtr, memoryLength, id, ref offset, addNullTermination: false) == false)
                {
                    return Errno.InvalidArgument;
                }
                return Errno.Success;
            });
        }

        public int get_text(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int textPtr, int textLength)
        {
            return _helper.GetAttribute<PomlTextElement>(elementDescriptor, (element) =>
            {
                var text = element.Text;
                var byteLen = Encoding.UTF8.GetByteCount(text);

                if (textLength < byteLen)
                {
                    return Errno.InsufficientBufferSize;
                }

                var addNullTermination = (textLength > byteLen);
                var offset = (uint)textPtr;

                if (MemoryHelper.TryWriteUtf8(memoryPtr, memoryLength, text, ref offset, addNullTermination) == false)
                {
                    return Errno.InvalidArgument;
                }

                return Errno.Success;
            });
        }

        public int get_text_len(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int textLenPtr)
        {
            return _helper.GetAttribute<PomlTextElement>(elementDescriptor, (element) =>
            {
                var text = element.Text;
                var textLen = Encoding.UTF8.GetByteCount(text);
                if (MemoryHelper.TryWrite(memoryPtr, memoryLength, textLenPtr, textLen) == false)
                {
                    return Errno.InvalidArgument;
                }

                return Errno.Success;
            });
        }

        public int set_text(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int textPtr, int textLength)
        {
            return _helper.SetAttribute<PomlTextElement>(elementDescriptor, (element) =>
            {
                if (MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, textPtr, textLength, out var text) == false)
                {
                    return Errno.InvalidArgument;
                }

                element.Text = text;
                return Errno.Success;
            });
        }

        public int get_background_color(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int colorPtr)
        {
            return _helper.GetAttribute<PomlTextElement>(elementDescriptor, (element) =>
            {
                var color = element.BackgroundColor;
                if (MemoryHelper.TryWrite(memoryPtr, memoryLength, colorPtr, color) == false)
                {
                    return Errno.InvalidArgument;
                }
                return Errno.Success;
            });
        }

        public int set_background_color(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int colorPtr)
        {
            return _helper.SetAttribute<PomlTextElement>(elementDescriptor, (element) =>
            {
                if (MemoryHelper.TryRead<UnityEngine.Color>(memoryPtr, memoryLength, colorPtr, out var color) == false)
                {
                    return Errno.InvalidArgument;
                }
                element.BackgroundColor = color;
                return Errno.Success;
            });
        }

        private bool TryGetModel(int elementDescriptor, out ModelElementComponent modelComponent, out int errorCode)
        {
            modelComponent = null;
            if (_helper.TryGetElement<PomlModelElement>(elementDescriptor, out var elementComponent, out var element, out errorCode) == false)
            {
                return false;
            }
            if (elementComponent.TryGetComponent(out modelComponent) == false)
            {
                return false;
            }
            return true;
        }

        private static bool ConvertWrapMode(int wrap, out UnityEngine.WrapMode wrapMode)
        {
            bool result;
            (result, wrapMode) = wrap switch
            {
                0 => (true, UnityEngine.WrapMode.Once),
                1 => (true, UnityEngine.WrapMode.Loop),
                _ => (false, UnityEngine.WrapMode.Loop),
            };
            return result;
        }

        public int change_anim_by_name(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int namePtr, int nameLen, int play, int wrap)
        {
            if (MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, namePtr, nameLen, out var name) == false)
            {
                return (int)Errno.InvalidArgument;
            }

            if (TryGetModel(elementDescriptor, out var modelComponent, out var errorCode) == false)
            {
                return errorCode;
            }
            if (modelComponent.ChangeAnimation(name) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            if (Convert.ToBoolean(play))
            {
                ConvertWrapMode(wrap, out var wrapMode);
                modelComponent.PlayAnimation(wrapMode);
            }
            else
            {
                modelComponent.StopAnimation();
            }
            return (int)Errno.Success;
        }

        public int change_anim(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int index, int play, int wrap)
        {
            if (TryGetModel(elementDescriptor, out var modelComponent, out var errorCode) == false)
            {
                return errorCode;
            }
            if (modelComponent.ChangeAnimation(index) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            if (Convert.ToBoolean(play))
            {
                ConvertWrapMode(wrap, out var wrapMode);
                modelComponent.PlayAnimation(wrapMode);
            }
            else
            {
                modelComponent.StopAnimation();
            }
            return (int)Errno.Success;
        }

        public int get_anim_state(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int statePtr)
        {
            if (TryGetModel(elementDescriptor, out var modelComponent, out var errorCode) == false)
            {
                return errorCode;
            }
            var isPlaying = modelComponent.IsAnimationPlaying();
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, statePtr, isPlaying) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int set_anim_state(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int state)
        {
            if (TryGetModel(elementDescriptor, out var modelComponent, out var errorCode) == false)
            {
                return errorCode;
            }
            var play = Convert.ToBoolean(state);
            if (play)
            {
                modelComponent.PlayAnimation(modelComponent.WrapMode);
            }
            else
            {
                modelComponent.StopAnimation();
            }
            return (int)Errno.Success;
        }

        public int get_current_anim(IntPtr memoryPtr, uint memoryLength, int elementDescriptor, int indexPtr)
        {
            if (TryGetModel(elementDescriptor, out var modelComponent, out var errorCode) == false)
            {
                return errorCode;
            }
            if (modelComponent.TryGetCurrentAnimation(out int index) == false)
            {
                index = -1;
            }
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, indexPtr, index) == false)
            {
                return (int)Errno.InvalidArgument;
            }

            return (int)Errno.Success;
        }

        public int get_camera_position(IntPtr memoryPtr, uint memoryLength, int cameraDescriptor, int referenceElementDescriptor, int positionPtr)
        {
            int errorCode;
            if (_helper.TryGetElement<PomlElement>(referenceElementDescriptor, out var referenceComponent, out var reference, out errorCode) == false)
            {
                return errorCode;
            }
            if (CameraDescriptorHelper.TryGetCamera(cameraDescriptor, out var camera, out errorCode) == false)
            {
                return errorCode;
            }
            var worldToLocal = referenceComponent.transform.worldToLocalMatrix;
            var cameraWorldPos = camera.transform.position;
            var localPos = worldToLocal.MultiplyPoint(cameraWorldPos);
            localPos = CoordinateUtility.ToSpirareCoordinate(localPos, true);
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, positionPtr, localPos) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int get_camera_rotation(IntPtr memoryPtr, uint memoryLength, int cameraDescriptor, int referenceElementDescriptor, int rotationPtr)
        {
            int errorCode;
            if (_helper.TryGetElement<PomlElement>(referenceElementDescriptor, out var referenceComponent, out var reference, out errorCode) == false)
            {
                return errorCode;
            }
            if (CameraDescriptorHelper.TryGetCamera(cameraDescriptor, out var camera, out errorCode) == false)
            {
                return errorCode;
            }
            var rot = camera.transform.rotation * Quaternion.Inverse(referenceComponent.transform.rotation);
            rot = CoordinateUtility.ToSpirareCoordinate(rot);
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, rotationPtr, rot) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int get_camera_type(IntPtr memoryPtr, uint memoryLength, int cameraDescriptor, int typePtr)
        {
            int errorCode;
            if (CameraDescriptorHelper.TryGetCamera(cameraDescriptor, out var camera, out errorCode) == false)
            {
                return errorCode;
            }
            var type = camera.orthographic ? CameraType.Perspective : CameraType.Orthographic;
            if (MemoryHelper.TryWrite<CameraType>(memoryPtr, memoryLength, typePtr, type) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int get_camera_perspective_params(
            IntPtr memoryPtr,
            uint memoryLength,
            int cameraDescriptor,
            int fovyPtr,
            int aspectPtr,
            int nearPtr,
            int farPtr)
        {
            int errorCode;
            if (CameraDescriptorHelper.TryGetCamera(cameraDescriptor, out var camera, out errorCode) == false)
            {
                return errorCode;
            }
            var fovy = camera.fieldOfView;
            var aspect = camera.aspect;
            var near = camera.nearClipPlane;
            var far = camera.farClipPlane;

            if (MemoryHelper.TryWrite<float>(memoryPtr, memoryLength, fovyPtr, fovy) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            if (MemoryHelper.TryWrite<float>(memoryPtr, memoryLength, aspectPtr, aspect) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            if (MemoryHelper.TryWrite<float>(memoryPtr, memoryLength, nearPtr, near) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            if (MemoryHelper.TryWrite<float>(memoryPtr, memoryLength, farPtr, far) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int is_within_camera(IntPtr memoryPtr, uint memoryLength, int cameraDescriptor, int elementDescriptor, int resultPtr)
        {
            int errorCode;

            if (CameraDescriptorHelper.TryGetCamera(cameraDescriptor, out var camera, out errorCode) == false)
            {
                return errorCode;
            }
            if (_helper.TryGetElement<PomlElement>(elementDescriptor, out PomlElementComponent elementComponent, out var element, out errorCode) == false)
            {
                return errorCode;
            }

            bool result;
            if (elementComponent.TryGetComponent<IWithinCamera>(out var x))
            {
                result = x.IsWithinCamera(camera);
            }
            else
            {
                result = false;
            }

            if (MemoryHelper.TryWrite<bool>(memoryPtr, memoryLength, resultPtr, result) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

        public int get_main_camera(IntPtr memoryPtr, uint memoryLength, int cameraDescriptorPtr)
        {
            var descriptor = CameraDescriptorHelper.MainCameraDescriptor;
            if (MemoryHelper.TryWrite<int>(memoryPtr, memoryLength, cameraDescriptorPtr, descriptor) == false)
            {
                return (int)Errno.InvalidArgument;
            }
            return (int)Errno.Success;
        }

#pragma warning restore IDE1006 // naming style
    }

    public readonly struct EventCallbackState
    {
        public readonly Func<(IntPtr MemoryPtr, uint MemoryLen)> GetMemory { get; }
        public readonly Action<int, int, int, int> Callback { get; }

        public EventCallbackState(
            Func<(IntPtr MemoryPtr, uint MemoryLen)> getMemory,
            Action<int, int, int, int> callback
        )
        {
            GetMemory = getMemory;
            Callback = callback;
        }
    }
}
