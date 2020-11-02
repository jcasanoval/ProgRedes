﻿using System;
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