using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Model;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private IUrlHelper _urlHelper;

        public BooksController(ILibraryRepository libraryRepository, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetBooksForAuthor")]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var booksForAuthor = _libraryRepository.GetBooksForAuthor(authorId);
            var bookToReturn = Mapper.Map<IEnumerable<BookDto>>(booksForAuthor);
            bookToReturn = bookToReturn.Select(book =>
            {
                book = CreateLinksForBook(book);
                return book;
            });
            var wrapper = new LinkedCollectionRecourceWrapperDto<BookDto>(bookToReturn);
            return Ok(CreateLinksForBooks(wrapper));
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuth = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuth == null)
            {
                return NotFound();
            }
            var bookToReturn = Mapper.Map<BookDto>(bookForAuth);
            return Ok(CreateLinksForBook(bookToReturn));
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (book.Title == book.Description)
            {
                ModelState.AddModelError(nameof(BookForCreationDto),
                    "The provided description must be different from title");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookEntity = Mapper.Map<Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Someting wronb on book save");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new {authorId, id = bookToReturn.Id},
                CreateLinksForBook(bookToReturn));
        }

        [HttpDelete("{id}", Name = "DeleteBookForAuthor")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthFromRepo == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteBook(bookForAuthFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleteing book {id} for author {authorId} failed on save!");
            }
            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateBookForAuthor")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (book.Title == book.Description)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided description must be different from title");
            }
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthFromRepo == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                _libraryRepository.UpdateBookForAuthor(bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception("FAILED!");
                }
                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new {authorId, id = bookToReturn.Id}, bookToReturn);
                //return NotFound();
            }
            Mapper.Map(book, bookForAuthFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception("FAILED!");
            }
            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdatedBookForAuthor")]
        public IActionResult PartiallyUpdatedBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }


            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthFromRepo == null) // create new book with a provided id
            {
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto, ModelState);
                if (bookDto.Title == bookDto.Description)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto),
                        "The provided description must be different from title");
                }
                TryValidateModel(bookDto);
                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }
                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception("WRONG ON SAVE");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new {authorId, id}, bookToReturn);

                //return NotFound();
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthFromRepo);

            patchDoc.ApplyTo(bookToPatch, ModelState);

            if (bookToPatch.Title == bookToPatch.Description)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided description must be different from title");
            }
            TryValidateModel(bookToPatch);
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            //Validation
            Mapper.Map(bookToPatch, bookForAuthFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception("WRONG ON SAVE");
            }
            return NoContent();
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            book.Links.Add(new LinkDto(_urlHelper.Link("GetBookForAuthor", new {id = book.Id}), "self", "GET"));
            book.Links.Add(new LinkDto(_urlHelper.Link("DeleteBookForAuthor", new {id = book.Id}), "delete_book",
                "DELETE"));
            book.Links.Add(
                new LinkDto(_urlHelper.Link("UpdateBookForAuthor", new {id = book.Id}), "update_book", "PUT"));
            book.Links.Add(new LinkDto(_urlHelper.Link("GetBookForAuthor", new {id = book.Id}), "partialy_update_book",
                "PATCH"));

            return book;
        }

        private LinkedCollectionRecourceWrapperDto<BookDto> CreateLinksForBooks(
            LinkedCollectionRecourceWrapperDto<BookDto> booksWraper)
        {
            booksWraper.Links.Add(
                new LinkDto(_urlHelper.Link("GetBooksForAuthor", new { }), "self", "GET")
            );

            return booksWraper;
        }
    }
}