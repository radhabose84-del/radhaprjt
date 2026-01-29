using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.UserLogin.Commands.UserLogin
{
    public class UserLockoutInfo
    {
              public int Attempts { get; set; }
            public bool IsLocked { get; set; }
            public DateTime? UnlockTime { get; set; }
    }
}