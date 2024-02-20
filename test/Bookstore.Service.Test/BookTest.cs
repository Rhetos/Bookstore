using Autofac;
using Bookstore.Service.Test.Tools;
using Common.Queryable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhetos;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.TestCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Service.Test
{
    [TestClass]
    public class BookTest
    {
        /// <summary>
        /// BookInfo.NumberOfChapters is persisted in a cache table and should be automatically updated
        /// when a chapter is added or deleted.
        /// </summary>
        [TestMethod]
        public void AutomaticallyUpdateNumberOfChapters()
        {
            using (var scope = TestScope.Create())
            {
                var repository = scope.Resolve<Common.DomRepository>();

                var book = new Book { Title = Guid.NewGuid().ToString() };
                repository.Bookstore.Book.Insert(book);

                int? readNumberOfChapters() => repository.Bookstore.BookInfo
                    .Query(bi => bi.ID == book.ID)
                    .Select(bi => bi.NumberOfChapters)
                    .Single();

                Assert.AreEqual(0, readNumberOfChapters());

                var c1 = new Chapter { BookID = book.ID, Heading = "c1" };
                var c2 = new Chapter { BookID = book.ID, Heading = "c2" };
                var c3 = new Chapter { BookID = book.ID, Heading = "c3" };

                repository.Bookstore.Chapter.Insert(c1);
                Assert.AreEqual(1, readNumberOfChapters());

                repository.Bookstore.Chapter.Insert(c2, c3);
                Assert.AreEqual(3, readNumberOfChapters());

                repository.Bookstore.Chapter.Delete(c1);
                Assert.AreEqual(2, readNumberOfChapters());

                repository.Bookstore.Chapter.Delete(c2, c3);
                Assert.AreEqual(0, readNumberOfChapters());
            }
        }

        [TestMethod]
        public void CommonMisspellingValidation()
        {
            using (var scope = TestScope.Create())
            {
                var repository = scope.Resolve<Common.DomRepository>();

                var book = new Book { Title = "x curiousity y" };

                TestUtility.ShouldFail<UserException>(
                    () => repository.Bookstore.Book.Insert(book),
                    "It is not allowed to enter misspelled word");
            }
        }

        [TestMethod]
        public void CommonMisspellingValidation_DirectFilter()
        {
            using (var scope = TestScope.Create())
            {
                var repository = scope.Resolve<Common.DomRepository>();

                var books = new[]
                {
                    new Bookstore_Book { Title = "spirit" },
                    new Bookstore_Book { Title = "opportunity" },
                    new Bookstore_Book { Title = "curiousity" },
                    new Bookstore_Book { Title = "curiousity2" }
                }.AsQueryable();

                var invalidBooks = repository.Bookstore.Book.Filter(books, new CommonMisspelling());

                Assert.AreEqual("curiousity, curiousity2", TestUtility.DumpSorted(invalidBooks, book => book.Title));
            }
        }

        [TestMethod]
        public void OverrideSystemComponentsForTesting()
        {
            // Application's DI container can be configured with custom components.
            // For example, system log monitor and fake mail sender as 'mock' objects
            // for testing Book entity insert operation.
            
            var systemLog = new List<string>();
            var mailSenderMock = new FakeMailSender();

            using (var scope = TestScope.Create(builder =>
                {
                    builder.ConfigureLogMonitor(systemLog); // Overrides system logging, see the code inside ConfigureLogMonitor.
                    builder.RegisterInstance(mailSenderMock).As<IMailSender>(); // Override IMailSender implementation from mail application.
                }))
            {
                var context = scope.Resolve<Common.ExecutionContext>();
                var repository = context.Repository;

                // Initializing data in order to have clean setup for testing mail notifications and log analysis.

                var testUserWithMail = new Common.Principal
                {
                    Name = "TestUserWithMail_" + Guid.NewGuid(),
                    NewBookNotificationEmailAddress = "testemail@example.com"
                };
                repository.Common.Principal.Insert(testUserWithMail);

                systemLog.Clear();

                // Inserting a new book:

                var book = new Book { Title = "TestBook" };
                repository.Bookstore.Book.Insert(book);

                // Inserting a book should automatically generate records in computed tables BookInfo and BookRating.
                // Here we can analyze the system log (see CreateLogMonitorDelegate) and review if it contains entries
                // for automatic update of those tables.

                string systemLogReport = string.Join(Environment.NewLine, systemLog);
                Console.WriteLine(systemLogReport); // Developers can manually review log in test output.
                TestUtility.AssertContains(systemLogReport, new[]
                {
                    "ComputeBookInfo",
                    "ComputeBookRating"
                });

                // Inserting a book should result with sending a notification mail.
                // Using out fake IMailSender implementation to test if the insert operation triggered the notification.

                var sentEmail = mailSenderMock.MailLog // Expecting only one notification e-mail.
                    .Single(mail => mail.ToEmailAddress == testUserWithMail.NewBookNotificationEmailAddress);

                TestUtility.AssertContains(sentEmail.Message, book.Title);
            }
        }

        /// <summary>
        /// This type of tests verify that there are not issues with deadlocks or incorrect computed data on parallel operations.
        /// This test also demonstrate how to manage (rare) situations when test needs to commit data into database.
        /// </summary>
        [TestMethod]
        public void ParallelCodeGeneration()
        {
            DeleteUnitTestBooks(); // This test needs to commit changes, so it is required to clean up any remaining previous test data, in case the test was canceled without ClassCleanup.

            // Prepare test data:

            var books = new[]
            {
                // Using specific prefix to reduce chance of conflicts with any existing data.
                new Book { Code = $"{UnitTestBookCodePrefix}+++", Title = Guid.NewGuid().ToString() },
                new Book { Code = $"{UnitTestBookCodePrefix}+++", Title = Guid.NewGuid().ToString() },
                new Book { Code = $"{UnitTestBookCodePrefix}ABC+", Title = Guid.NewGuid().ToString() },
                new Book { Code = $"{UnitTestBookCodePrefix}ABC+", Title = Guid.NewGuid().ToString() }
            };

            // Insert in parallel:

            for (int retry = 0; retry < 3; retry++) // Running the test multiple times to avoid false positive, since the results are nondeterministic.
            {
                Parallel.ForEach(books, book =>
                {
                    // Each scope represent one web request of the main application, executed in its own separate transaction.
                    // The main application should support parallel web requests.
                    using (var scope = TestScope.Create())
                    {
                        var repository = scope.Resolve<Common.DomRepository>();
                        repository.Bookstore.Book.Insert(book);
                        scope.CommitAndClose(); // Changes are committed to database, to make the test with parallel transactions more realistic.
                    }
                });

                // Review the inserted data:

                using (var scope = TestScope.Create())
                {
                    var repository = scope.Resolve<Common.DomRepository>();
                    var booksFromDb = repository.Bookstore.Book.Load(book => book.Code.StartsWith(UnitTestBookCodePrefix));
                    Assert.AreEqual(
                        $"{UnitTestBookCodePrefix}001, {UnitTestBookCodePrefix}002, {UnitTestBookCodePrefix}ABC1, {UnitTestBookCodePrefix}ABC2",
                        TestUtility.DumpSorted(booksFromDb, book => book.Code));
                }

                DeleteUnitTestBooks();
            }
        }

        private const string UnitTestBookCodePrefix = "UnitTestBooks";

        private void DeleteUnitTestBooks()
        {
            using (var scope = TestScope.Create())
            {
                var repository = scope.Resolve<Common.DomRepository>();
                var testBooks = repository.Bookstore.Book.Load(book => book.Code.StartsWith(UnitTestBookCodePrefix));
                repository.Bookstore.Book.Delete(testBooks);
                scope.CommitAndClose();
            }
        }
    }
}
