using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Application.UserLogin.Commands.UserLogin
{
    public class UserLoginCommand : IRequest<ApiResponseDTO<LoginResponse>>
    {
        // public LoginRequest Request { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }

}