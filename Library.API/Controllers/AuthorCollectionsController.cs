using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Model;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        public string OrderBy { get; set; } = "Name";

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection == null)
            {
                return BadRequest();
            }
            var authorsToSave = Mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var a in authorsToSave)
            {
                _libraryRepository.AddAuthor(a);
            }
            if (!_libraryRepository.Save())
            {
                throw new Exception("Creeating author collection failed on save");
            }
            var authorCollToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorsToSave);
            var idsAsStrings = string.Join(",",
                authorCollToReturn.Select(a => a.Id));
            return CreatedAtRoute("GetAuthorCollection", new {ids = idsAsStrings}, authorCollToReturn);
        }

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorIds = ids as IList<Guid> ?? ids.ToList();
            var authorEntities = _libraryRepository.GetAuthors(authorIds);
            if (authorIds.Count != authorEntities.Count())
            {
                return NotFound();
            }
            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authFromRepo = _libraryRepository.GetAuthor(id);
            if (authFromRepo == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteAuthor(authFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting author {id} failed on save!");
            }
            return NoContent();
        }
    }
}