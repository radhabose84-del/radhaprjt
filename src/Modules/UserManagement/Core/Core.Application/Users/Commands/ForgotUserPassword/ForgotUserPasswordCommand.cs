using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Users.Queries.GetUsers;
using MediatR;

namespace Core.Application.Users.Commands.ForgotUserPassword
{
    public class ForgotUserPasswordCommand : IRequest<ApiResponseDTO<ForgotPasswordResponse>>
    {
         public string? UserName { get; set; }
    }
}