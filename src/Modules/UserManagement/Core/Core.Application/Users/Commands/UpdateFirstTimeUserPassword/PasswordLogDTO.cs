using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Users.Commands.CreateFirstTimeUserPassword
{
    public class PasswordLogDTO
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Message { get; set; }
    }
}