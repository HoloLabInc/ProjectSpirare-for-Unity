namespace HoloLab.Spirare
{
    public sealed class RealWorldSpaceReferenceElementComponent : SpaceReferenceElementComponent
    {
        internal RealWorldSpaceReferenceElementComponent Initialize(PomlSpaceReferenceElement spaceReferenceElement)
        {
            base.Initialize(spaceReferenceElement);

            SpaceReferenceElement = spaceReferenceElement;

            if (gameObject.TryGetComponent<MultiSpaceReferenceComponent>(out var multiSpaceReferenceComponent) == false)
            {
                multiSpaceReferenceComponent = gameObject.AddComponent<MultiSpaceReferenceComponent>();
                multiSpaceReferenceComponent.Initialize();
            }
            multiSpaceReferenceComponent.AddSpaceReference(spaceReferenceElement);

            return this;
        }
    }
}
