using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HoloLab.Spirare.Wasm.Core")]

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("HoloLab.Spirare.PlayModeTests")]
#endif