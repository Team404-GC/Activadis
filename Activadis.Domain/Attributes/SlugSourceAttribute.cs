namespace Activadis.Domain.Attributes
{
    /// <summary>
    ///     This tells IHasSlug that this is the source it should take from to make the slug.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SlugSourceAttribute : Attribute { }
}
