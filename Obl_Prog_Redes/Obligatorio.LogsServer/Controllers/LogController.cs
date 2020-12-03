using Common.Logs;
using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogsServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Obligatorio.LogsServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
        public class LogServerController : Controller
        {
        // GET: LogServerControlle
        [HttpGet]
        public async Task<ActionResult<List<LogModel>>> Get()
        {
            var logs = await GetLogs();

            if (logs.Count < 0)
                return NotFound();
            return logs;
        }

            public async Task<List<LogModel>> GetLogs()
            {
                List<LogModel> logs = new List<LogModel>();
                foreach (ILog log in LogServer.GetInstance().logs)
                {
                    logs.Add(LogModel.ToModel(log));
                }
                //logs.Add(new LogModel() { Type = "Info", Message = "Hola", User = "Juan" });
                return logs;
            }
        }
}
