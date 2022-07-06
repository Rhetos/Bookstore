using Autofac;
using Bookstore.Service.Controllers;
using Bookstore.Service.Test.Tools;
using Common.Queryable;
using DemoRowPermissions2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    public class CustomControllerTest
    {
        [TestMethod]
        public void ControllerRowPermissionsRead()
        {
            string testUserName = "TestUser_" + Guid.NewGuid();

            using (var scope = TestScope.Create(builder =>
            {
                builder.ConfigureApplicationUser(testUserName); // Overrides user authentication with custom user info, see the code inside ConfigureApplicationUser.
                builder.ConfigureOptions<Rhetos.Utilities.AppSecurityOptions>(o => o.AllClaimsForAnonymous = false); // User authorization testing will fail in case the anonymous access is globally allowed.
                builder.RegisterType<CustomController>();
                builder.RegisterGeneric(typeof(StubRhetosComponent<>)).As(typeof(IRhetosComponent<>));
                builder.RegisterGeneric(typeof(TestMsLogger<>)).As(typeof(ILogger<>));
            })) 
            {
                var repository = scope.Resolve<Common.DomRepository>();
                var rpRepository = repository.DemoRowPermissions2;
                var customController = scope.Resolve<CustomController>();

                // Inserting the test data.

                var region = new Region { Name = "region_" + Guid.NewGuid() };
                rpRepository.Region.Insert(new[] { region });

                var division = new Division { Name = "division_" + Guid.NewGuid(), RegionID = region.ID };
                rpRepository.Division.Insert(new[] { division });

                var doc = new Document { Title = "doc_" + Guid.NewGuid(), DivisionID = division.ID };
                rpRepository.Document.Insert(doc);

                // Testing the row permissions for the *current* user (set by ConfigureApplicationUser above).

                Assert.AreEqual("403 Not allowed (claim).", Report(customController.DemoCustomRead(doc.Title)));

                var principal = repository.Common.Principal.Query(p => p.Name == testUserName).Single();
                var claimRead = repository.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomRead").Single();
                repository.Common.PrincipalPermission.Insert(new Common.PrincipalPermission
                {
                    PrincipalID = principal.ID,
                    ClaimID = claimRead.ID,
                    IsAuthorized = true
                });

                Assert.AreEqual("403 Not allowed (read row permissions).", Report(customController.DemoCustomRead(doc.Title)));

                var employee = new Employee { UserName = testUserName, DivisionID = division.ID };
                rpRepository.Employee.Insert(new[] { employee });

                Assert.AreEqual($"{doc.ID}", Report(customController.DemoCustomRead(doc.Title)));
            }
        }

        [TestMethod]
        public void ControllerRowPermissionsWrite()
        {
            string testName = GetType().Name;
            var testUserId = Guid.NewGuid();
            var testUserName = testName + "_" + testUserId;
            var principal = new Common.Principal { ID = testUserId, Name = testUserName };
            var region = new Region { ID = Guid.NewGuid(), Name = testName + "_" + Guid.NewGuid() };
            var division1 = new Division { ID = Guid.NewGuid(), Name = testName + "_1_" + Guid.NewGuid(), RegionID = region.ID };
            var division2 = new Division { ID = Guid.NewGuid(), Name = testName + "_2_" + Guid.NewGuid(), RegionID = region.ID };
            var document = new Document { ID = Guid.NewGuid(), Title = testName + "_1_" + Guid.NewGuid(), DivisionID = division1.ID };
            var documentSameDivision = new Document { ID = document.ID, Title = document.Title + " V2", DivisionID = division1.ID }; // Different version of the same document.
            var documentDifferentDivision = new Document { ID = document.ID, Title = document.Title + " V2", DivisionID = division2.ID }; // Different version of the same document.
            var employee = new Employee { ID = Guid.NewGuid(), UserName = testUserName, DivisionID = division1.ID };

            void Cleanup(Common.DomRepository r)
            {
                r.DemoRowPermissions2.Document.Delete(r.DemoRowPermissions2.Document.Query(item => item.Title.StartsWith(testName)));
                r.DemoRowPermissions2.Employee.Delete(r.DemoRowPermissions2.Employee.Query(item => item.UserName.StartsWith(testName)));
                r.DemoRowPermissions2.Division.Delete(r.DemoRowPermissions2.Division.Query(item => item.Name.StartsWith(testName)));
                r.DemoRowPermissions2.Region.Delete(r.DemoRowPermissions2.Region.Query(item => item.Name.StartsWith(testName)));
                r.Common.Principal.Delete(r.Common.Principal.Query(re => re.Name.StartsWith(testName)));
            }

            Assert.AreEqual(
                "403 Not allowed (claim)., document doesn't exist",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                    },
                    DocumentOperation.Insert, document, Cleanup));

            Assert.AreEqual(
                "403 Not allowed (write row permissions on new data)., document doesn't exist",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                        r.Common.Principal.Insert(principal);
                        var claim = r.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomWrite").Single();
                        r.Common.PrincipalPermission.Insert(new Common.PrincipalPermission { PrincipalID = principal.ID, ClaimID = claim.ID, IsAuthorized = true });
                    },
                    DocumentOperation.Insert, document, Cleanup));

            Assert.AreEqual(
                "200, document exists, title updated",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                        r.Common.Principal.Insert(principal);
                        var claim = r.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomWrite").Single();
                        r.Common.PrincipalPermission.Insert(new Common.PrincipalPermission { PrincipalID = principal.ID, ClaimID = claim.ID, IsAuthorized = true });
                        r.DemoRowPermissions2.Employee.Insert(new[] { employee }); // Employee has row permissions for division1.
                    },
                    DocumentOperation.Insert, document, Cleanup));

            Assert.AreEqual(
                "200, document exists, title updated",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                        r.Common.Principal.Insert(principal);
                        var claim = r.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomWrite").Single();
                        r.Common.PrincipalPermission.Insert(new Common.PrincipalPermission { PrincipalID = principal.ID, ClaimID = claim.ID, IsAuthorized = true });
                        r.DemoRowPermissions2.Employee.Insert(new[] { employee });
                        r.DemoRowPermissions2.Document.Insert(document);
                    },
                    DocumentOperation.Update, documentSameDivision, Cleanup));

            Assert.AreEqual(
                "403 Not allowed (write row permissions on new data)., document exists, title different",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                        r.Common.Principal.Insert(principal);
                        var claim = r.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomWrite").Single();
                        r.Common.PrincipalPermission.Insert(new Common.PrincipalPermission { PrincipalID = principal.ID, ClaimID = claim.ID, IsAuthorized = true });
                        r.DemoRowPermissions2.Employee.Insert(new[] { employee });
                        r.DemoRowPermissions2.Document.Insert(document);
                    },
                    DocumentOperation.Update, documentDifferentDivision, Cleanup));

            Assert.AreEqual(
                "403 Not allowed (write row permissions on existing data)., document exists, title different",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                        r.Common.Principal.Insert(principal);
                        var claim = r.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomWrite").Single();
                        r.Common.PrincipalPermission.Insert(new Common.PrincipalPermission { PrincipalID = principal.ID, ClaimID = claim.ID, IsAuthorized = true });
                        r.DemoRowPermissions2.Employee.Insert(new[] { employee });
                        r.DemoRowPermissions2.Document.Insert(documentDifferentDivision);
                    },
                    DocumentOperation.Update, document, Cleanup));

            Assert.AreEqual(
                "200, document doesn't exist",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                        r.Common.Principal.Insert(principal);
                        var claim = r.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomWrite").Single();
                        r.Common.PrincipalPermission.Insert(new Common.PrincipalPermission { PrincipalID = principal.ID, ClaimID = claim.ID, IsAuthorized = true });
                        r.DemoRowPermissions2.Employee.Insert(new[] { employee });
                        r.DemoRowPermissions2.Document.Insert(document);
                    },
                    DocumentOperation.Delete, document, Cleanup));

            Assert.AreEqual(
                "403 Not allowed (write row permissions on existing data)., document exists, title updated",
                TestControllerWrite(testUserName,
                    r =>
                    {
                        r.DemoRowPermissions2.Region.Insert(new[] { region });
                        r.DemoRowPermissions2.Division.Insert(new[] { division1, division2 });
                        r.Common.Principal.Insert(principal);
                        var claim = r.Common.Claim.Query(c => c.ClaimResource == "DemoRowPermissions2.Document" && c.ClaimRight == "DemoCustomWrite").Single();
                        r.Common.PrincipalPermission.Insert(new Common.PrincipalPermission { PrincipalID = principal.ID, ClaimID = claim.ID, IsAuthorized = true });
                        r.DemoRowPermissions2.Employee.Insert(new[] { employee });
                        r.DemoRowPermissions2.Document.Insert(documentDifferentDivision);
                    },
                    DocumentOperation.Delete, documentDifferentDivision, Cleanup));
        }

        string TestControllerWrite(
            string testUserName,
            Action<Common.DomRepository> setup,
            DocumentOperation operation,
            Document document,
            Action<Common.DomRepository> cleanup)
        {
            using (var scope = TestScope.Create())
            {
                var repository = scope.Resolve<Common.DomRepository>();
                setup.Invoke(repository);
                scope.CommitAndClose();
            }

            string controllerResult;
            using (var scope = TestScope.Create(builder =>
            {
                builder.ConfigureApplicationUser(testUserName); // Overrides user authentication with custom user info, see the code inside ConfigureApplicationUser.
                builder.ConfigureOptions<Rhetos.Utilities.AppSecurityOptions>(o => o.AllClaimsForAnonymous = false); // User authorization testing will fail in case the anonymous access is globally allowed.
                builder.RegisterType<CustomController>();
                builder.RegisterGeneric(typeof(StubRhetosComponent<>)).As(typeof(IRhetosComponent<>));
                builder.RegisterGeneric(typeof(TestMsLogger<>)).As(typeof(ILogger<>));
            }))
            {
                var customController = scope.Resolve<CustomController>();
                controllerResult = Report(customController.DemoCustomWrite(
                    operation == DocumentOperation.Insert ? new[] { document } : null,
                    operation == DocumentOperation.Update ? new[] { document } : null,
                    operation == DocumentOperation.Delete ? new[] { document } : null));
                // The controller commits the unit of work from the request scope, if successful.
            }

            string documentTitle;
            using (var scope = TestScope.Create())
            {
                var repository = scope.Resolve<Common.DomRepository>();
                var loadedDocument = repository.DemoRowPermissions2.Document.Query(new[] { document.ID }).SingleOrDefault();
                documentTitle =
                    loadedDocument == null ? "document doesn't exist"
                    : loadedDocument.Title == document.Title ? "document exists, title updated"
                    : "document exists, title different";
                cleanup.Invoke(repository);
                scope.CommitAndClose();
            }

            return controllerResult + ", " + documentTitle;
        }

        enum DocumentOperation { Insert, Update, Delete };

        private string Report(ActionResult<object> actionResult)
        {
            if (actionResult.Result is ObjectResult objectResult)
                return $"{objectResult.StatusCode} {objectResult.Value}";

            if (actionResult.Value is IEnumerable<IEntity> entities)
                return string.Join(", ", entities.Select(e => e.ID).OrderBy(x => x));

            if (actionResult.Result is StatusCodeResult statusCodeResult)
                return statusCodeResult.StatusCode.ToString();

            return actionResult.ToString();
        }
    }

    internal class StubRhetosComponent<T> : IRhetosComponent<T>
    {
        private readonly T component;
        public StubRhetosComponent(T component) => this.component = component;
        public T Value => component;
    }

    internal class TestMsLogger<T> : ILogger<T>
    {
        private readonly Rhetos.Logging.ILogProvider rhetosLogProvider;
        public TestMsLogger(Rhetos.Logging.ILogProvider rhetosLogProvider) => this.rhetosLogProvider = rhetosLogProvider;
        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            rhetosLogProvider
                .GetLogger(typeof(T).FullName)
                .Write(Rhetos.Logging.EventType.Info, () => $"{logLevel} {eventId} {formatter(state, exception)}");
    }
}
