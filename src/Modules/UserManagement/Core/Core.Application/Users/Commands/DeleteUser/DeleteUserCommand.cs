using Core.Application.Common.HttpResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int UserId { get; set; }
    }
}
