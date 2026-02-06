using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Users.Commands.RemoveVerificationCode
{
    public class RemoveVerficationCodeCommand : IRequest<ApiResponseDTO<bool>>
    {
        public string? UserName { get; set; }
    }
}