using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Extensions;

public static class ObjectExtensions
{
    public static ExpandoObject
        ShapeData<TSource>(this TSource source, string? fields)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        // create an ExpandoObject that will hold the selected properties & values
        var dataShapedObject = new ExpandoObject();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // all public properties should be in the ExpandoObject
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            foreach (var propertyInfo in propertyInfos)
            {
                // GetValue returns the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(source);

                // add the field to the ExpandoObject
                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }

        // fields are separated by ',' so we need to split them.
        var fieldAfterSplit = fields.Split(',');

        foreach (var field in fieldAfterSplit)
        {
            // Trim each field, as it might contain leading or trailing spaces.
            // Can't trim var in foreach, so use another var
            var propertyName = field.Trim();

            // Use reflection to get the property on the source object. we need to include public and instance,
            // b/c specifying a binding flag overwrites the already-existing binding flags.
            var propertyInfo = typeof(TSource)
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (propertyInfo == null)
                throw new Exception($"Property {propertyName} was not found on {typeof(TSource)}.");

            // GetValue returns the value of the property on the source object
            var propertyValue = propertyInfo.GetValue(source);

            // add the field to the ExpandoObject
            ((IDictionary<string, object?>)dataShapedObject)
                .Add(propertyInfo.Name, propertyValue);
        }

        // return the shaped object
        return dataShapedObject;
    }
}
