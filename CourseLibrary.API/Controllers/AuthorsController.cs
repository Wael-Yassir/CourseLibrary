
using AutoMapper;
using CourseLibrary.API.Extensions;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using CourseLibrary.API.Services.PropertyCheckerService;
using CourseLibrary.API.Services.PropertyMappingService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IPropertyMappingService _propertyMappingService;
    private readonly IPropertyCheckerService _propertyCheckerService;

    // it is used as return when something goes wrong, as a way to pass more details to the client
    private readonly ProblemDetailsFactory _problemDetailsFactory;      

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper,
        IPropertyMappingService propertyMappingService,
        IPropertyCheckerService propertyCheckerService,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _propertyMappingService = propertyMappingService ??
            throw new ArgumentNullException(nameof(propertyMappingService));
        _propertyCheckerService = propertyCheckerService ??
            throw new ArgumentNullException(nameof(propertyCheckerService));
        _problemDetailsFactory = problemDetailsFactory ??
            throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    [HttpGet(Name = nameof(GetAuthors))]
    [HttpHead]
    public async Task<IActionResult>
        GetAuthors([FromQuery] AuthorResourceParameters resourceParameters)
    {
        if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Entities.Author>(resourceParameters.OrderBy))
            return BadRequest();

        if(!_propertyCheckerService.TypeHasProperty<AuthorDto>(resourceParameters.Fields)) 
            return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(
                        HttpContext,
                        statusCode: 400,
                        detail: $"Not all requested data shaping fields exist on the" +
                        $" resource: {resourceParameters.Fields}"));

        var authorsPageVM = await _courseLibraryRepository
            .GetAuthorsAsync(resourceParameters);

        var previousPageLink = authorsPageVM.HasPrevious 
            ? CreateAuthorResourceUri(resourceParameters, ResourceUriType.PREVIOUS_PAGE)
            : null;

        var nextPageLink = authorsPageVM.HasNext
            ? CreateAuthorResourceUri(resourceParameters, ResourceUriType.NEXT_PAGE)
            : null;

        var paginationMetaData = new
        {
            pageNumber = authorsPageVM.CurrentPage,
            pageSize = authorsPageVM.PageSize,
            totalCount = authorsPageVM.TotalCount,
            totalPages = authorsPageVM.TotalPages,
            previousPageLink,
            nextPageLink,
        };

        HttpContext.Response.Headers
            .Add("X-Pagination", JsonSerializer.Serialize(paginationMetaData));

        var authorToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorsPageVM)
            .ShapeData(resourceParameters.Fields);
        
        return Ok(authorToReturn);
    }

    [HttpGet("{authorId}", Name = nameof(GetAuthor))]
    public async Task<IActionResult> 
        GetAuthor(Guid authorId, string? fields)
    {
        if (!_propertyCheckerService.TypeHasProperty<AuthorDto>(fields))
            return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(
                        HttpContext,
                        statusCode: 400,
                        detail: $"Not all requested data shaping fields exist on the resource: {fields}"));

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        var authorToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
            .ShapeData(fields);

        return Ok(authorToReturn);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute(nameof(GetAuthor),
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }

    // In Options, only an Allow header is added with all available methods names
    [HttpOptions]
    public IActionResult GetAuthorsOptions()
    {
        Request.Headers.Add("Allow", "GET,HEAD,PUT,OPTIONS");
        return Ok();
    }

    private string? CreateAuthorResourceUri(
        AuthorResourceParameters resourceParameters, ResourceUriType uriType)
    {
        switch (uriType)
        {
            case ResourceUriType.PREVIOUS_PAGE:
                return Url.Link(nameof(GetAuthors),
                    new
                    {
                        fields = resourceParameters.Fields,
                        orderBy = resourceParameters.OrderBy,
                        pageNumber = resourceParameters.PageNumber - 1,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });
                
            case ResourceUriType.NEXT_PAGE:
                return Url.Link(nameof(GetAuthors),
                    new
                    {
                        fields = resourceParameters.Fields,
                        orderBy = resourceParameters.OrderBy,
                        pageNumber = resourceParameters.PageNumber + 1,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });

            default:
                return Url.Link(nameof(GetAuthors),
                    new
                    {
                        fields = resourceParameters.Fields,
                        orderBy = resourceParameters.OrderBy,
                        pageNumber = resourceParameters.PageNumber,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });
        }
    }
}
