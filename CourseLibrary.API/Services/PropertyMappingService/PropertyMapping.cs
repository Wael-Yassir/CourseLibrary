namespace CourseLibrary.API.Services.PropertyMappingService;

/// <summary>
/// Class to hold the source and destination types for mapping
/// </summary>
/// <typeparam name="TSource"></typeparam>
/// <typeparam name="TDestination"></typeparam>
public class PropertyMapping<TSource, TDestination> : IPropertyMapping
{
    public Dictionary<string, PropertyMappingValue> 
        MappingDictionary { get; set; }

    public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        MappingDictionary = mappingDictionary ?? 
            throw new ArgumentNullException(nameof(mappingDictionary));
    }
}
