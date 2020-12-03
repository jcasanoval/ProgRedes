using System;
using System.Collections.Generic;
using System.Text;

namespace Common.CommandProtocol
{
    public class CommandConstants
    {
        public const int Exit = 0;
        public const int Login = 1;
        public const int Register = 2;
        public const int RequestLoggedUser = 3;
        public const int FinishSendingList = 4;
        public const int ACK = 5;
        public const int UserList = 6;
        public const int SendPicture = 7;
        public const int PictureList = 8;
        public const int CommentList = 9;
        public const int NewComment = 10;
        public const int Logs = 11;
        public const int LogInfo = 12;
        public const int LogWarning = 13;
        public const int LogError = 14;
        public const int Error = 99;
    }

    public class MessageConstants
    {
        public const String NoUserFound = "";
        public const String SuccessfulLogin = "LoginSuccess";
        public const String FailedLogin = "LoginFailure";
        public const String SuccessfulRegister = "RegisterSuccess";
        public const String FailedRegister = "RegistrationFailure";
    }
}
