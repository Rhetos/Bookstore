using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rhetos;
using Rhetos.Logging;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System;
using System.Linq;

namespace Bookstore.Service.Controllers
{
    [Route("Custom/[action]")]
    public class CustomController : ControllerBase
    {
        private readonly IProcessingEngine processingEngine;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<CustomController> aspNetLogger;
        private readonly Rhetos.Logging.ILogger rhetosLogger;

        public CustomController(IRhetosComponent<IProcessingEngine> processingEngine, IRhetosComponent<IUnitOfWork> unitOfWork,
            ILogger<CustomController> aspNetLogger, IRhetosComponent<ILogProvider> rhetosLogProvider)
        {
            this.processingEngine = processingEngine.Value;
            this.unitOfWork = unitOfWork.Value;
            this.aspNetLogger = aspNetLogger;
            rhetosLogger = rhetosLogProvider.Value.GetLogger(GetType().Name);
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
    }
}
