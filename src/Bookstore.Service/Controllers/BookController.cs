using Microsoft.AspNetCore.Mvc;
using Rhetos;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Filters;
using System.Threading.Tasks;

namespace Bookstore.Service.Controllers
{
    /// <summary>
    /// Example of a custom controller.
    /// It is an alternative to "Action" DSL concept, see "Action InsertManyBooks" in DSL script "Batches.rhe".
    /// The controller uses service filters from Rhetos.RestGenerator:
    /// 1. ApiExceptionFilter returns JSON error response on exception, with a user message for Rhetos.UserException, same as Rhetos REST API,
    /// 2. ApiCommitOnSuccessFilter commits Rhetos unit of work (IUnitOfWork) on successful web request.
    /// </summary>
    [Route("rest/Bookstore/Book/[action]")]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [ServiceFilter(typeof(ApiCommitOnSuccessFilter))]
    [ApiExplorerSettings(GroupName = "v1")] // Avoids overriding Swagger information on Rhetos REST.
    public class BookController : ControllerBase
    {
        private readonly Common.ExecutionContext _context;

        /// <summary>
        /// Components from Rhetos dependency injection container are available with IRhetosComponent interface.
        /// Components from ASP.NET DI container are available directly.
        /// </summary>
        public BookController(IRhetosComponent<Common.ExecutionContext> context)
        {
            _context = context.Value;
        }

        [HttpGet]
        public async Task<ActionResult<Book>> GetCustomBook()
        {
            return new Book { Title = "some custom book" };
        }

        [HttpPost]
        public void InsertManyBooks(int numberOfBooks, string titlePrefix)
        {
            for (int i = 0; i < numberOfBooks; i++)
            {
                string newTitle = titlePrefix + " - " + (i + 1);
                var newBook = new Bookstore.Book { Code = "+++", Title = newTitle };
                _context.Repository.Bookstore.Book.Insert(newBook);
            }
        }
    }
}
