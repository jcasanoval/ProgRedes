using Common.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Obligatorio.LogsServer.Models
{
    public class LogModel : Model<ILog, LogModel>
    {

        public string User { set; get; }
        public string Message { set; get; }

        public string Type { get; set; }

        public override ILog ToEntity()
        {
            ILog log = null;
            switch (Type)
            {
                case "Info":
                    log = new Info();
                    break;
                case "Warning":
                    log = new Warning();
                    break;
                case "Error":
                    log = new Error();
                    break;
            }
            log.User = this.User;
            log.Message = this.Message;
            return log;
        }


        protected override LogModel SetModel(ILog entity)
        {
            LogModel logModel = new LogModel();
            if (entity is Info)
            {
                logModel.Type = "Info";
            }
            else if (entity is Warning)
            {
                logModel.Type = "Warning";
            }
            else if (entity is Error)
            {
                logModel.Type = "Error";
            }
            logModel.User = entity.User;
            logModel.Message = entity.Message;
            return logModel;
        }
    }
}
