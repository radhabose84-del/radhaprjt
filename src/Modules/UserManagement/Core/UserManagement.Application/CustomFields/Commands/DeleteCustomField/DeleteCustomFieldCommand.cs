using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.CustomFields.Commands.DeleteCustomField
{
    public class DeleteCustomFieldCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}