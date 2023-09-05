using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Extensions;

public static class IEnumerableExtensions
{
    // ExpandoObject represent a dynamic object which its
    // memeber can be dynamically added and removed at runtime
    public static IEnumerable<ExpandoObject> 
        ShapeData<TSource>(this IEnumerable<TSource> source, string? fields)
    {
        if (source == null) 
            throw new ArgumentNullException(nameof(source));

        // Create a list to hold our ExpandoObject
        var expandoObjectList = new List<ExpandoObject>();

        // Create a list with PropertyInfo objects on TSource. Reflection is expensive, so rather
        // than doing it or each object in the list, we can do it once and reuse the resutls.
        // After all part of the reflection is on type of the object (TSource), not on instance
        var propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // all public properties should be in the ExpandoObject
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
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

                // add propertyInfo to list
                propertyInfoList.Add(propertyInfo);
            }
        }

        // run through the source objects.
        foreach (var sourceObject in source)
        {
            // create an ExpandoObject that will hold the selected properties & values
            var dataShapedObject = new ExpandoObject();

            foreach (var propertyInfo in propertyInfoList)
            {
                // GetValue returns the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(sourceObject);

                // add the field to the ExpandoObject
                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            // Add the ExpandoObject to the list
            expandoObjectList.Add(dataShapedObject);
        }

        // return the list
        return expandoObjectList;
    }
}
