using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICourseLibraryRepository _courseRepository;

        public AuthorCollectionsController(
            ICourseLibraryRepository courseRepository, IMapper mapper)
        {
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            _courseRepository = courseRepository
                ?? throw new ArgumentNullException(nameof(courseRepository));
        }

        // To get a collection of authors from post Location Header we have two approach
        // [1]: pass the guids separated by comma: guid1, guid2, guid3, ...
        // [2]: pass the guids as key-value pairs: key1=value1, key2=value2, ...
        [HttpGet("({authorIds})", Name = nameof(GetAuthorCollection))]
        public async Task<ActionResult<IEnumerable<AuthorDto>>>
            GetAuthorCollection(
                [ModelBinder(BinderType = typeof(ArrayModelBinder))]
                [FromRoute] IEnumerable<Guid> authorIds)
        {
            var authorEntities = await _courseRepository.GetAuthorsAsync(authorIds);

            // check if all author Ids had been found
            if (authorIds.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>>
            GetAuthorCollection([FromQuery] string authorIds)
        {
            var ids = authorIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => new Guid(id));

            var authorEntities = await _courseRepository.GetAuthorsAsync(ids);

            // check if all author Ids had been found
            if (authorIds.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<AuthorDto>>>
            CreateAuthorCollection(IEnumerable<AuthorForCreationDto> authorCollection)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var author in authorEntities)
            {
                _courseRepository.AddAuthor(author);
            }

            await _courseRepository.SaveAsync();
            
            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var authorIdsAdString = string.Join(',', authorEntities.Select(author => author.Id));

            return CreatedAtRoute(
                nameof(GetAuthorCollection), 
                new { authorIds = authorIdsAdString },
                authorsToReturn);
        }
    }
}
