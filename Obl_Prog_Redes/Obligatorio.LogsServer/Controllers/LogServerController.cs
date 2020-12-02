using Common.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio.LogsServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Obligatorio.LogsServer.Controllers
{
    [Route("api/logs")]
    public class LogServerController : Controller
    {

        // GET: LogServerControlle
        [HttpGet]
        public ActionResult GetAll()
        {
            IEnumerable<ILog> logs = LogServer.GetInstance().logs;
            return Ok(LogModel.ToModel(logs));
        }
    }
}
