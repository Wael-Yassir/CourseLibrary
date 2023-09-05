using CourseLibrary.API.Models;
using CourseLibrary.API.Entities;

namespace CourseLibrary.API.Services.PropertyMappingService;

public class PropertyMappingService : IPropertyMappingService
{
    // Will create a mapping dictionary for every entities
    private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping
        = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new(new[] { "Id" }) },
            { "MainCategory", new(new[] { "MainCategory" }) },
            { "Age", new(new[] {"DateOfBirth" }, true) },
            { "Name", new(new[] { "FirstName", "LastName" }) },
        };

    // To resolve both TSource, and TDestination, a marker interface need to be used
    // a marker interface is an interface with that contain no methods inside it.
    private readonly IList<IPropertyMapping> _propertyMapping
        = new List<IPropertyMapping>();

    public PropertyMappingService()
    {
        _propertyMapping.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
    }

    public Dictionary<string, PropertyMappingValue>
        GetPropertyMapping<TSource, TDestination>()
    {
        // get matching mapping
        var matchingMapping = _propertyMapping
            .OfType<PropertyMapping<TSource, TDestination>>();

        if (matchingMapping.Count() == 1)
            return matchingMapping.First().MappingDictionary;

        throw new Exception($"Cannot find exact property mapping instance " +
            $"for <{typeof(TSource)}, {typeof(TDestination)}");
    }

    public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
    {
        var propertyMapping = GetPropertyMapping<TSource, TDestination>();

        if(string.IsNullOrWhiteSpace(fields)) 
            return true;

        // the string is separated by ',' so we need to split it
        var fieldsAfterSplit = fields.Split(',');

        // run through the fields clauses
        foreach (var field in fieldsAfterSplit)
        {
            var trimmedField = field.Trim();

            // remove everything after the first " " - if the fields are coming
            // orderBy string, this part must be ignored
            var indexOfFirstSpace = trimmedField.IndexOf(' ');
            var propertyName = indexOfFirstSpace == -1
                ? trimmedField
                : trimmedField.Remove(indexOfFirstSpace);

            // find the matching property
            if (!propertyMapping.ContainsKey(propertyName))
            {
                return false;
            }
        }

        return true;
    }
}
