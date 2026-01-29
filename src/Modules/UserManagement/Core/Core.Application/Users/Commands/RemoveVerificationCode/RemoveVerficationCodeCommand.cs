using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Users.Commands.RemoveVerificationCode
{
    public class RemoveVerficationCodeCommand : IRequest<ApiResponseDTO<bool>>
    {
        public string? UserName { get; set; }
    }
}