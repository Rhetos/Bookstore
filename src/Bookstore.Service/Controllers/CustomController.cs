using Bookstore.Service.Controllers.Rhetos520;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rhetos;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Logging;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using Rhetos.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Bookstore.Service.Controllers
{
    [Route("Custom/[action]")]
    public class CustomController : ControllerBase
    {
        private readonly IProcessingEngine processingEngine;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<CustomController> aspNetLogger;
        private readonly Rhetos.Logging.ILogger rhetosLogger;
        private readonly Common.ExecutionContext executionContext;
        private readonly ServerCommandsUtility520 serverCommandsUtility;

        public CustomController(
            IRhetosComponent<IProcessingEngine> processingEngine,
            IRhetosComponent<IUnitOfWork> unitOfWork,
            ILogger<CustomController> aspNetLogger,
            IRhetosComponent<ILogProvider> rhetosLogProvider,
            IRhetosComponent<Common.ExecutionContext> executionContext,
            IRhetosComponent<ServerCommandsUtility520> serverCommandsUtility)
        {
            this.processingEngine = processingEngine.Value;
            this.unitOfWork = unitOfWork.Value;
            this.aspNetLogger = aspNetLogger;
            this.rhetosLogger = rhetosLogProvider.Value.GetLogger(GetType().Name);
            this.executionContext = executionContext.Value;
            this.serverCommandsUtility = serverCommandsUtility.Value;
        }

        [HttpGet]
        public ActionResult<string> DatabaseLog(bool throwException)
        {
            var addLogCommand = new ExecuteActionCommandInfo
            {
                Action = new Common.AddToLog { Action = "CustomAddLog" }
            };
            processingEngine.Execute(addLogCommand);

            // This controller does not use ServiceFilter ApiExceptionFilter,
            // so the response will contain a HTML page with the exception details,
            // instead of UserMessage/SystemMessage error object.
            if (throwException)
                throw new Exception("Test exception.");

            var readLogCommand = new ReadCommandInfo
            {
                DataSource = "Common.LogReader",
                Top = 10,
                OrderByProperties = new[] { new OrderByProperty { Property = "Created", Descending = true } },
                ReadRecords = true
            };
            var readResult = processingEngine.Execute(readLogCommand);

            string report = string.Join(Environment.NewLine, readResult.Records.Cast<Common.LogReader>().Select(r => $"{r.Created}: {r.Action} {r.TableName}"));

            // This controller does not use ServiceFilter ApiCommitOnSuccessFilter,
            // so the unitOfWork needs to be committed manually and the end of the operation.
            unitOfWork.CommitAndClose();

            return report;
        }

        [HttpGet]
        public ActionResult<string> SystemLog()
        {
            aspNetLogger.LogTrace("ASP.NET log trace.");
            aspNetLogger.LogInformation("ASP.NET log info.");
            aspNetLogger.LogWarning("ASP.NET log warning.");
            aspNetLogger.LogError("ASP.NET log error.");

            rhetosLogger.Trace("Rhetos log trace.");
            rhetosLogger.Info("Rhetos log info.");
            rhetosLogger.Warning("Rhetos log warning.");
            rhetosLogger.Error("Rhetos log error.");

            return "Review the application's log file, console output or Windows event log.";
        }

        [HttpGet]
        public string DemoProcessingEngine()
        {
            // PERMISSIONS: When using IProcessingEngine, it will automatically verify user's claims and row permissions.

            var result = processingEngine.Execute(
                new ReadCommandInfo
                {
                    DataSource = "AuthenticationDemo.UserInfoReport",
                    ReadRecords = true
                });

            var records = (IEnumerable<AuthenticationDemo.UserInfoReport>)result.Records;

            return "UserInfo:" + Environment.NewLine
                + string.Join(Environment.NewLine, records.Select(record => $"{record.Key}: {record.Value}"));
        }

        [HttpGet]
        public ActionResult<object> DemoCustomRead(string titlePrefix)
        {
            // PERMISSIONS: When using repository classes directly, we need to manually verify user's claims and row permissions.

            if (!UserHasClaim("DemoRowPermissions2.Document", "DemoCustomRead"))
                return StatusCode((int)HttpStatusCode.Forbidden, "Not allowed (claim).");

            if (titlePrefix == null)
                titlePrefix = "";
            var documents = executionContext.Repository.DemoRowPermissions2.Document.Query(doc => doc.Title.StartsWith(titlePrefix)).Take(3).ToSimple().ToArray();

            if (!serverCommandsUtility.ForEntity("DemoRowPermissions2.Document").UserHasReadRowPermissions(documents))
                return StatusCode((int)HttpStatusCode.Forbidden, "Not allowed (read row permissions).");

            return documents;
        }

        [HttpGet]
        public ActionResult<object> DemoCustomWrite(
            DemoRowPermissions2.Document[] insertItems,
            DemoRowPermissions2.Document[] updateItems,
            DemoRowPermissions2.Document[] deleteItems)
        {
            // PERMISSIONS: When using repository classes directly, we need to manually verify user's claims and row permissions.
            // The method does not need to have all three arguments, but this sample shows how to verify each type of operation differently
            // with UserHasWriteRowPermissionsBeforeSave and/or UserHasWriteRowPermissionsAfterSave.

            if (!UserHasClaim("DemoRowPermissions2.Document", "DemoCustomWrite"))
                return StatusCode((int)HttpStatusCode.Forbidden, "Not allowed (claim).");

            var entityCommandsUtility = serverCommandsUtility.ForEntity("DemoRowPermissions2.Document");
            if (!entityCommandsUtility.UserHasWriteRowPermissionsBeforeSave(deleteItems, updateItems))
                return StatusCode((int)HttpStatusCode.Forbidden, "Not allowed (write row permissions on existing data).");

            executionContext.Repository.DemoRowPermissions2.Document.Save(
                insertItems, updateItems, deleteItems,
                checkUserPermissions: true); // IMPORTANT: Set checkUserPermissions to activate additional validations in the repository class such as DenyUserEdit.

            if (!entityCommandsUtility.UserHasWriteRowPermissionsAfterSave(insertItems, updateItems))
                return StatusCode((int)HttpStatusCode.Forbidden, "Not allowed (write row permissions on new data).");

            unitOfWork.CommitAndClose();
            return Ok();
        }

        private bool UserHasClaim(string resource, string right)
        {
            var claim = new Claim(resource, right);
            return executionContext.AuthorizationManager.GetAuthorizations(new[] { claim }).Single();
        }
    }
}
