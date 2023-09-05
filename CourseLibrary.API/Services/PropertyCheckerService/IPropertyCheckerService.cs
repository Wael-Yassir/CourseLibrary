namespace CourseLibrary.API.Services.PropertyCheckerService
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperty<T>(string? fields);
    }
}