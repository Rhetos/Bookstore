using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Rhetos;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.Service.Controllers
{
    [Route("Custom/[action]")]
    public class CustomController : ControllerBase
    {
        private readonly IProcessingEngine processingEngine;
        private readonly IUnitOfWork unitOfWork;

        public CustomController(IRhetosComponent<IProcessingEngine> rhetosProcessingEngine, IRhetosComponent<IUnitOfWork> rhetosUnitOfWork)
        {
            processingEngine = rhetosProcessingEngine.Value;
            unitOfWork = rhetosUnitOfWork.Value;
        }

        [HttpGet]
        public ActionResult<string> AddLog(bool throwException)
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
    }
}
