using System.Reflection;

namespace CourseLibrary.API.Services.PropertyCheckerService;

public class PropertyCheckerService : IPropertyCheckerService
{
    public bool TypeHasProperty<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return true;

        // the fields are separated by ',' so we need to split it.
        var fieldsAfterSplit = fields.Split(',');

        foreach (var field in fieldsAfterSplit)
        {
            // trim each field, as it might contain leading or trailing spaces
            // can't trim the var in foreach so use another var.
            var propertyNmae = field.Trim();

            // use reflection to check if property can be found on T
            var propertyInfo = typeof(T).GetProperty(propertyNmae,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            // return false if can't be found
            if (propertyInfo == null)
                return false;
        }

        // all checks out, return true
        return true;
    }
}
