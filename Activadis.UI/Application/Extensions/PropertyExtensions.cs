using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Activadis.UI.Application.Extensions
{
    public static class PropertyExtensions
    {
        public static int? GetMaxLength(this object instance, string propertyName)
        {
            PropertyInfo? property = instance.GetType().GetProperty(propertyName);
            MaxLengthAttribute? attribute = property?.GetCustomAttribute<MaxLengthAttribute>();
            return attribute?.Length ?? 0;
        }
    }
}
