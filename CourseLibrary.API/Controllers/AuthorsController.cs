
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ICourseLibraryRepository _courseLibraryRepository;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet(Name = nameof(GetAuthors))]
    [HttpHead]
    public async Task<ActionResult<IEnumerable<AuthorDto>>>
        GetAuthors([FromQuery] AuthorResourceParameters resourceParameters)
    { 
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

        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsPageVM));
    }

    [HttpGet("{authorId}", Name = nameof(GetAuthor))]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
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
                        pageNumber = resourceParameters.PageNumber - 1,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });
                
            case ResourceUriType.NEXT_PAGE:
                return Url.Link(nameof(GetAuthors),
                    new
                    {
                        pageNumber = resourceParameters.PageNumber + 1,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });

            default:
                return Url.Link(nameof(GetAuthors),
                    new
                    {
                        pageNumber = resourceParameters.PageNumber,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });
        }
    }
}
