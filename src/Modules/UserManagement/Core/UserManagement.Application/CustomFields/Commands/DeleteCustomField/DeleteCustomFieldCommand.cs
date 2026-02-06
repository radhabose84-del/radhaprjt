using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.CustomFields.Commands.DeleteCustomField
{
    public class DeleteCustomFieldCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}