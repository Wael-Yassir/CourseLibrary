namespace CourseLibrary.API.Services.PropertyMappingService;

public class PropertyMappingValue
{
    public IEnumerable<string> DestinationPropeties { get; private set; }
    public bool Revert { get; private set; }

    public PropertyMappingValue
        (IEnumerable<string> destinationPropeties, bool revert = false)
    {
        DestinationPropeties = destinationPropeties ??
            throw new ArgumentNullException(nameof(destinationPropeties));
        
        Revert = revert;
    }
}