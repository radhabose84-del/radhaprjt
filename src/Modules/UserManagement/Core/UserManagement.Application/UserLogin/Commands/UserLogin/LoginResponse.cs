using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.UserLogin.Commands.UserLogin
{
    public class LoginResponse
    {
        public string? Token { get; set; }

        public int? PartyId { get; set; }
        public FirstTimeUserStatus IsFirstTimeUser { get; set; }

        public string? Message { get; set; }  
    }
}