namespace HoloLab.Spirare
{
    public static class PomlElementExtensions
    {
        public static bool IsRenderedInScreenSpace(this PomlElement element)
        {
            if (element is PomlScreenSpaceElement)
            {
                return true;
            }

            if (element.Parent == null)
            {
                return false;
            }

            return element.Parent.IsRenderedInScreenSpace();
        }
    }
}
