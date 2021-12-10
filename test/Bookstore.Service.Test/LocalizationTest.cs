using Bookstore.Service.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhetos.Utilities;

namespace Bookstore.Service.Test
{
    [TestClass]
    public class LocalizationTest
    {
        /// <summary>
        /// BookInfo.NumberOfComments is persisted in a cache table and should be automatically updated
        /// each a comment is added or deleted.
        /// </summary>
        [TestMethod]
        public void LocalizationContextDemo()
        {
            using (var scope = TestScope.Create())
            {
                var defaultLocalizer = scope.Resolve<ILocalizer>();
                var commentLocalizer = scope.Resolve<ILocalizer<Bookstore.Comment>>();
                var bookLocalizer = scope.Resolve<ILocalizer<Bookstore.Book>>();

                // In file 'Localization\en.po' the property name AuthorID is translated to 'Author' by default,
                // but in the context of "Bookstore.Book" it is translated to "Author of the book".

                Assert.AreEqual("Author", defaultLocalizer["AuthorID"]);
                Assert.AreEqual("Author", commentLocalizer["AuthorID"]);
                Assert.AreEqual("Author of the book", bookLocalizer["AuthorID"]);
            }
        }
    }
}