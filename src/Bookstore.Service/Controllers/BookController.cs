using Microsoft.AspNetCore.Mvc;
using Rhetos;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Host.AspNet.RestApi.Filters;

namespace Bookstore.Service.Controllers
{
    /// <summary>
    /// Example of a custom controller.
    /// It is an alternative to "Action" DSL concept, see "Action InsertManyBooks" in DSL script "Batches.rhe".
    /// This controller extends the existing REST API (rest/Bookstore/Book) with additional web methods.
    /// The controller uses service filters from Rhetos.RestGenerator:
    /// 1. ApiExceptionFilter returns JSON error response on exception, with a user message for Rhetos.UserException, same as Rhetos REST API,
    /// 2. ApiCommitOnSuccessFilter commits Rhetos unit of work (IUnitOfWork) on successful web request.
    /// </summary>
    [Route("rest/Bookstore/Book/[action]")] // Same base root as generic Rhetos REST controller.
    [ServiceFilter(typeof(ApiExceptionFilter))] // Same error response format as Rhetos REST (UserMessage and SystemMessage). Optional.
    [ServiceFilter(typeof(ApiCommitOnSuccessFilter))] // Automatically commit unit of work on response 200, rollback otherwise. Optional.
    [ApiExplorerSettings(GroupName = "v1")] // Avoids overriding Swagger information on Rhetos REST (optional) by using a separate document for custom methods. Optional.
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
        public ActionResult<Book> GetCustomBook()
        {
            return new Book { Title = "some custom book" };
        }

        [HttpPost]
        public void CustomInsert(int numberOfBooks, string titlePrefix)
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
