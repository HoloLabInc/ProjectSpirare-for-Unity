using Newtonsoft.Json.Linq;

namespace HoloLab.Spirare
{
    internal abstract class PomlPatch
    {
        public enum PomlPatchOperation
        {
            None,
            [EnumLabel("add")]
            Add,
            [EnumLabel("update")]
            Update,
            [EnumLabel("remove")]
            Remove
        }

        public class PomlPatchTarget
        {
            public string Id { set; get; }
            public string Tag { set; get; }
        }

        public PomlPatchOperation Operation { get; }

        public PomlPatchTarget Target { get; set; }

        public PomlPatch(PomlPatchOperation operation)
        {
            Operation = operation;
        }
    }

    internal class PomlPatchAddElement
    {
        public string Tag { set; get; }
        public JObject Attributes { set; get; }

        public PomlPatchAddElement[] Children { set; get; }
    }

    internal class PomlPatchAdd : PomlPatch
    {
        public PomlPatchAddElement Element { set; get; }

        public PomlPatchAdd() : base(PomlPatchOperation.Add) { }
    }

    internal class PomlPatchUpdate : PomlPatch
    {
        public JObject Attributes { set; get; }

        public PomlPatchUpdate() : base(PomlPatchOperation.Update) { }
    }

    internal class PomlPatchRemove : PomlPatch
    {
        public PomlPatchRemove() : base(PomlPatchOperation.Remove) { }
    }
}
