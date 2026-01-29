using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommand : IRequest<ApiResponseDTO<string>>
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}