using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Application.Users.Queries.GetUserById  
{
    public class GetUserByIdQuery : IRequest<UserByIdDTO>
    {
        public int UserId { get; set; }
    }
}   