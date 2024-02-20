using Bookstore.Service.Test.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhetos.Utilities;
using System.Globalization;
using System.Threading;

namespace Bookstore.Service.Test
{
    [TestClass]
    public class LocalizationTest
    {
        [TestMethod]
        public void LocalizationContextDemo()
        {
            using (var scope = TestScope.Create())
            {
                var defaultLocalizer = scope.Resolve<ILocalizer>();
                var chapterLocalizer = scope.Resolve<ILocalizer<Bookstore.Chapter>>();
                var bookLocalizer = scope.Resolve<ILocalizer<Bookstore.Book>>();

                // In file 'Localization\en.po' the property name AuthorID is translated to 'Author' by default,
                // but in the context of "Bookstore.Book" it is translated to "Author of the book".

                Assert.AreEqual("Author", defaultLocalizer["AuthorID"]);
                Assert.AreEqual("Author", chapterLocalizer["AuthorID"]);
                Assert.AreEqual("Author of the book", bookLocalizer["AuthorID"]);
            }
        }

        [TestMethod]
        public void LocalizationTextDefaultLanguage()
        {
            // Default language in unit tests is based on Thread.CurrentThread.CurrentUICulture,
            // not on DefaultRequestCulture specified in Startup.cs or Program.cs.
            using var scope = TestScope.Create();
            var localizer = scope.Resolve<ILocalizer>();
            Assert.AreEqual("Number of pages", localizer["NumberOfPages"]);
        }

        [TestMethod]
        public void LocalizationTextOverrideLanguage()
        {
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("hr-HR");
            // Based on CurrentUICulture, localization will use hr.po instead of en.po.
            try
            {
                using var scope = TestScope.Create();
                var localizer = scope.Resolve<ILocalizer>();
                Assert.AreEqual("Broj stranica", localizer["NumberOfPages"]);
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = oldCulture;
            }
        }
    }
}
