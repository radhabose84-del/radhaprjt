using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Commands.Users
{
    public class DeleteUserCommand
    {
        public int UserId { get; set; }
        public string Reason { get; set; }
    }
}