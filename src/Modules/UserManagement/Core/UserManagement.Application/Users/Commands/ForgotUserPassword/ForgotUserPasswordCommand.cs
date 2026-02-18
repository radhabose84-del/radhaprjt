using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;

namespace UserManagement.Application.Users.Commands.ForgotUserPassword
{
    public class ForgotUserPasswordCommand : IRequest<ApiResponseDTO<ForgotPasswordResponse>>
    {
         public string? UserName { get; set; }
    }
}