using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Notification.Exceptions
{
    public class ExceptionRules : Exception
    {
        public ExceptionRules(string message) : base(message) { }
    }
}