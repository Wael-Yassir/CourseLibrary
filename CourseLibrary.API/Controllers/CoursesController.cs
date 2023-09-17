
using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors/{authorId}/courses")]

[HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
[HttpCacheValidation(MustRevalidate = true)]

// if Marvin.Http.Headers is used, no need to put the following attribute
// [ResponseCache(CacheProfileName = "240SecCashProfile")] 
public class CoursesController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    public CoursesController(ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesForAuthor(Guid authorId)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var coursesForAuthorFromRepo = await _courseLibraryRepository.GetCoursesAsync(authorId);
        return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
    }

    // To add cashing, we need to first specify if a resource can be cashable or not by adding Cashe-Control
    // Header using [ResponseCashe] attribute, and secondly by adding a cashe store middleware.
    // [ResponseCache(Duration = 120)]     // duration in seconds.
    // if Marvin.Http.Headers is used, no need to put the above attribute

    [HttpGet("{courseId}", Name = nameof(GetCourseForAuthor))]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 1000)]
    [HttpCacheValidation(MustRevalidate = true)]
    public async Task<ActionResult<CourseDto>> GetCourseForAuthor(Guid authorId, Guid courseId)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<CourseDto>(courseForAuthorFromRepo));
    }


    [HttpPost]
    public async Task<ActionResult<CourseDto>> CreateCourseForAuthor(
            Guid authorId, CourseForCreationDto course)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseEntity = _mapper.Map<Entities.Course>(course);
        _courseLibraryRepository.AddCourse(authorId, courseEntity);
        await _courseLibraryRepository.SaveAsync();

        var courseToReturn = _mapper.Map<CourseDto>(courseEntity);

        // Using CreatedAtAction will result adding Location header at the response
        // to state how to get he created resource
        return CreatedAtAction(
            nameof(GetCourseForAuthor), new { authorId, courseId = courseToReturn.Id }, courseToReturn);
    }


    [HttpPut("{courseId}")]
    public async Task<IActionResult> UpdateCourseForAuthor(
        Guid authorId, Guid courseId, CourseForUpdateDto course)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);
        if (courseForAuthorFromRepo == null)
        {
            return NotFound();
        }

        _mapper.Map(course, courseForAuthorFromRepo);

        _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
        await _courseLibraryRepository.SaveAsync();

        return NoContent();
    }

    [HttpPatch("{courseId}")]
    public async Task<IActionResult> PartiallyUpdateCourseForAuthor(
        Guid authorId, Guid courseId, JsonPatchDocument<CourseForUpdateDto> patchDocument)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
            return NotFound();

        var courseForAuthorFromRepo =
            await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
            return NotFound();

        var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);

        patchDocument.ApplyTo(courseToPatch, ModelState);

        // by default, ValidationProblem() does not use the configured InvalidModelStateResponse
        // defined in the StartupHelperExtensions, but we can override it.
        if (!TryValidateModel(courseToPatch))
            return ValidationProblem(ModelState);

        _mapper.Map(courseToPatch, courseForAuthorFromRepo);
        _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
        await _courseLibraryRepository.SaveAsync();

        return NoContent();
    }

    [HttpDelete("{courseId}")]
    public async Task<ActionResult> DeleteCourseForAuthor(Guid authorId, Guid courseId)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
        {
            return NotFound();
        }

        _courseLibraryRepository.DeleteCourse(courseForAuthorFromRepo);
        await _courseLibraryRepository.SaveAsync();

        return NoContent();
    }

    public override ActionResult ValidationProblem(
        [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
    {
        var options = HttpContext.RequestServices
            .GetRequiredService<IOptions<ApiBehaviorOptions>>();

        return (ActionResult)options.Value
            .InvalidModelStateResponseFactory(ControllerContext);
    }

}