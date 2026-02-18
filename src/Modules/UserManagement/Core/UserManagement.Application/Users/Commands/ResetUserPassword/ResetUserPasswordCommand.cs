using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Users.Commands.ResetUserPassword
{
    public class ResetUserPasswordCommand : IRequest<ApiResponseDTO<string>>
    {
       // public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? VerificationCode { get; set; }
        public string? Password { get; set; }
        
    }
}