using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhitomi.Database;
using nhitomi.Documentation;
using nhitomi.Models;
using nhitomi.Models.Queries;
using nhitomi.Models.Validation;
using nhitomi.Scrapers;

namespace nhitomi.Controllers
{
    /// <summary>
    /// Contains endpoints for searching books and downloading images.
    /// </summary>
    [Route("books")]
    public class BookController : nhitomiControllerBase
    {
        readonly IBookService _books;
        readonly ISnapshotService _snapshots;
        readonly IVoteService _votes;

        public BookController(IBookService books, ISnapshotService snapshots, IVoteService votes)
        {
            _books     = books;
            _snapshots = snapshots;
            _votes     = votes;
        }

        /// <summary>
        /// Retrieves book information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        [HttpGet("{id}", Name = "getBook")]
        public async Task<ActionResult<Book>> GetAsync(string id)
        {
            var result = await _books.GetAsync(id);

            if (!result.TryPickT0(out var book, out _))
                return ResultUtilities.NotFound(id);

            return book.Convert();
        }

        /// <summary>
        /// Retrieves book content information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        [HttpGet("{id}/contents/{contentId}", Name = "getBookContent")]
        public async Task<ActionResult<BookContent>> GetContentAsync(string id, string contentId)
        {
            var result = await _books.GetContentAsync(id, contentId);

            if (!result.TryPickT0(out var value, out _))
                return ResultUtilities.NotFound(id, contentId);

            var (_, content) = value;

            return content.Convert();
        }

        public class GetByLinkRequest
        {
            [Required]
            public string Link { get; set; }
        }

        public class GetByLinkResponse
        {
            [Required]
            public GetByLinkMatch[] Matches { get; set; }

            public class GetByLinkMatch
            {
                [Required]
                public Book Book { get; set; }

                [Required, nhitomiId]
                public string SelectedContentId { get; set; }
            }
        }

        /// <summary>
        /// Finds books that were scraped from an URL.
        /// </summary>
        /// <param name="request">Get by link request.</param>
        [HttpPost("search/link", Name = "getBooksByLink")]
        public async Task<GetByLinkResponse> GetByLinkAsync(GetByLinkRequest request) => new GetByLinkResponse
        {
            Matches = await _books.GetByLinkAsync(request.Link).Select(x =>
            {
                var (book, content) = x;

                return new GetByLinkResponse.GetByLinkMatch
                {
                    Book              = book.Value.Convert(),
                    SelectedContentId = content.Id
                };
            }).ToArrayAsync()
        };

        /// <summary>
        /// Updates book information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="model">New book information.</param>
        /// <param name="reason">Reason for this action.</param>
        [HttpPut("{id}", Name = "updateBook"), RequireUser(Unrestricted = true)]
        public async Task<ActionResult<Book>> UpdateAsync(string id, BookBase model, [FromQuery] string reason = null)
        {
            var result = await _books.UpdateAsync(id, model, new SnapshotArgs
            {
                Committer = User,
                Event     = SnapshotEvent.AfterModification,
                Reason    = reason,
                Source    = SnapshotSource.User
            });

            if (!result.TryPickT0(out var book, out _))
                return ResultUtilities.NotFound(id);

            return book.Convert();
        }

        /// <summary>
        /// Updates book content information.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        /// <param name="model">New content information.</param>
        /// <param name="reason">Reason for this action.</param>
        [HttpPut("{id}/contents/{contentId}", Name = "updateBookContent"), RequireUser(Unrestricted = true)]
        public async Task<ActionResult<BookContent>> UpdateContentAsync(string id, string contentId, BookContentBase model, [FromQuery] string reason = null)
        {
            var result = await _books.UpdateContentAsync(id, contentId, model, new SnapshotArgs
            {
                Committer = User,
                Event     = SnapshotEvent.AfterModification,
                Reason    = reason,
                Source    = SnapshotSource.User
            });

            if (!result.TryPickT0(out var value, out _))
                return ResultUtilities.NotFound(id, contentId);

            var (_, content) = value;

            return content.Convert();
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="reason">Reason for this action.</param>
        [HttpDelete("{id}", Name = "deleteBook"), RequireHuman, RequireUser(Unrestricted = true), RequireReason]
        public async Task<ActionResult> DeleteAsync(string id, [FromQuery] string reason = null)
        {
            await _books.DeleteAsync(id, new SnapshotArgs
            {
                Committer = User,
                Event     = SnapshotEvent.BeforeDeletion,
                Reason    = reason,
                Source    = SnapshotSource.User
            });

            return Ok();
        }

        /// <summary>
        /// Deletes a content of a book.
        /// </summary>
        /// <remarks>
        /// If the content being deleted is the only content left in the book, the entire book will be deleted.
        /// </remarks>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        /// <param name="reason">Reason for this action.</param>
        [HttpDelete("{id}/contents/{contentId}", Name = "deleteBookContent"), RequireHuman, RequireUser(Unrestricted = true), RequireReason]
        public async Task<ActionResult> DeleteContentAsync(string id, string contentId, [FromQuery] string reason = null)
        {
            await _books.DeleteContentAsync(id, contentId, new SnapshotArgs
            {
                Committer = User,
                Event     = SnapshotEvent.BeforeDeletion,
                Reason    = reason,
                Source    = SnapshotSource.User
            });

            return Ok();
        }

        /// <summary>
        /// Retrieves book page image.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        /// <param name="index">Zero-based page index.</param>
        [HttpGet("{id}/contents/{contentId}/pages/{index}", Name = "getBookImage"), ProducesFile, AllowAnonymous]
        public async Task<ActionResult> GetImageAsync(string id, string contentId, int index)
        {
            var result = await _books.GetContentAsync(id, contentId);

            if (!result.TryPickT0(out var value, out _))
                return ResultUtilities.NotFound(id, contentId);

            var (book, content) = value;

            return new BookScraperImageResult(book, content, index);
        }

        /// <summary>
        /// Retrieves book thumbnail image.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="contentId">Content ID.</param>
        [HttpGet("{id}/contents/{contentId}/pages/0/thumb", Name = "getBookThumbnail"), ProducesFile, AllowAnonymous]
        public async Task<ActionResult> GetThumbnailAsync(string id, string contentId)
        {
            var result = await _books.GetContentAsync(id, contentId);

            if (!result.TryPickT0(out var value, out _))
                return ResultUtilities.NotFound(id, contentId);

            var (book, content) = value;

            return new BookScraperImageResult(book, content, 0)
            {
                Thumbnail = true
            };
        }

        /// <summary>
        /// Searches for books matching the given query.
        /// </summary>
        /// <param name="query">Book information query.</param>
        [HttpPost("search", Name = "searchBooks")]
        public async Task<SearchResult<Book>> SearchAsync(BookQuery query)
            => (await _books.SearchAsync(query)).Project(b => b.Convert());

        /// <summary>
        /// Finds autocomplete suggestions for books matching the given query.
        /// </summary>
        /// <param name="query">Book suggestion query.</param>
        [HttpPost("suggest", Name = "suggestBooks")]
        public async Task<BookSuggestResult> SuggestAsync(SuggestQuery query)
            => await _books.SuggestAsync(query);

        /// <summary>
        /// Retrieves a snapshot of book information.
        /// </summary>
        /// <param name="id">Snapshot ID.</param>
        [HttpGet("snapshots/{id}", Name = "getBookSnapshot")]
        public async Task<ActionResult<Snapshot>> GetSnapshotAsync(string id)
        {
            var result = await _snapshots.GetAsync(ObjectType.Book, id);

            if (!result.TryPickT0(out var snapshot, out _))
                return ResultUtilities.NotFound(id);

            return snapshot.Convert();
        }

        /// <summary>
        /// Retrieves book information at the time of a snapshot.
        /// </summary>
        /// <param name="id">Snapshot ID.</param>
        [HttpGet("snapshots/{id}/value", Name = "getBookSnapshotValue")]
        public async Task<ActionResult<Book>> GetSnapshotValueAsync(string id)
        {
            var result = await _snapshots.GetAsync(ObjectType.Book, id);

            if (!result.TryPickT0(out var snapshot, out _))
                return ResultUtilities.NotFound(id);

            var valueResult = await _snapshots.GetValueAsync<DbBook>(snapshot);

            if (!valueResult.TryPickT0(out var book, out _))
                return ResultUtilities.NotFound(id);

            return book.Convert();
        }

        /// <summary>
        /// Searches for snapshots of books matching the specified query.
        /// </summary>
        /// <param name="query">Snapshot information query.</param>
        [HttpPost("snapshots/search", Name = "searchBookSnapshots")]
        public async Task<SearchResult<Snapshot>> SearchSnapshotsAsync(SnapshotQuery query)
            => (await _snapshots.SearchAsync(ObjectType.Book, query)).Project(s => s.Convert());

        public class RollbackRequest
        {
            /// <summary>
            /// ID of the snapshot to rollback the target object to.
            /// </summary>
            [Required, nhitomiId]
            public string SnapshotId { get; set; }
        }

        /// <summary>
        /// Reverts book information to a previous snapshot.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="request">Rollback request.</param>
        /// <param name="reason">Reason for this action.</param>
        [HttpPost("{id}/snapshots/rollback", Name = "revertBook"), RequireUser(Unrestricted = true), RequireReason]
        public async Task<ActionResult<Book>> RollBackAsync(string id, RollbackRequest request, [FromQuery] string reason = null)
        {
            var result = await _snapshots.GetAsync(ObjectType.Book, request.SnapshotId);

            if (!result.TryPickT0(out var snapshot, out _))
                return ResultUtilities.NotFound(request.SnapshotId);

            var rollbackResult = await _snapshots.RollbackAsync<DbBook>(snapshot, new SnapshotArgs
            {
                Committer = User,
                Event     = SnapshotEvent.AfterRollback,
                Reason    = reason,
                Rollback  = snapshot,
                Source    = SnapshotSource.User
            });

            if (!rollbackResult.TryPickT0(out var value, out _))
                return ResultUtilities.NotFound(id);

            var (book, _) = value;

            return book.Convert();
        }

        /// <summary>
        /// Gets the vote on a book.
        /// </summary>
        /// <param name="id">Book ID.</param>
        [HttpGet("{id}/vote", Name = "getBookVote")]
        public async Task<ActionResult<Vote>> GetVoteAsync(string id)
        {
            var result = await _votes.GetAsync(UserId, new nhitomiObject(ObjectType.Book, id));

            if (!result.TryPickT0(out var vote, out _))
                return ResultUtilities.NotFound(id);

            return vote.Convert();
        }

        /// <summary>
        /// Sets the vote on a book, overwriting one if already set.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <param name="model">Vote information.</param>
        [HttpPut("{id}/vote", Name = "setBookVote")]
        public async Task<ActionResult<Vote>> SetVoteAsync(string id, VoteBase model)
        {
            var result = await _books.GetAsync(id);

            if (!result.TryPickT0(out var book, out _))
                return ResultUtilities.NotFound(id);

            var vote = await _votes.SetAsync(UserId, book, model.Type);

            return vote.Convert();
        }

        /// <summary>
        /// Removes the vote from a book.
        /// </summary>
        /// <param name="id">Book ID.</param>
        [HttpDelete("{id}/vote", Name = "unsetBookVote")]
        public async Task<ActionResult> UnsetVoteAsync(string id)
        {
            await _votes.UnsetAsync(UserId, new nhitomiObject(ObjectType.Book, id));

            return Ok();
        }
    }
}