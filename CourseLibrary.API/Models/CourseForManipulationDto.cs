using CourseLibrary.API.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescritpion]
    public abstract class CourseForManipulationDto //: IValidatableObject
    {
        [Required()]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1500)]
        public virtual string? Description { get; set; } = string.Empty;

        /*
        // Not executed when one of the data validation annotation already raised
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Title == Description)
            {
                yield return new ValidationResult(
                    "The provided description should be different from the title.", 
                    new[] { "Course" });
            }
        }
        */
    }
}
