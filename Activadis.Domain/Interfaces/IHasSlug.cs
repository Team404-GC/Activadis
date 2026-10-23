namespace Activadis.Domain.Interfaces
{
    /// <summary>
    ///     This forces the entity that inherits it to have a slug.
    ///     This will also make it so that a slug is generated automatically when the <strong>[SlugSource]</strong> is created or changed.
    ///     Because of that, the entity requires a property with the header <strong>[SlugSource]</strong> to work.
    /// </summary>
    public interface IHasSlug
    {
        string? Slug { get; set; }
    }
}
