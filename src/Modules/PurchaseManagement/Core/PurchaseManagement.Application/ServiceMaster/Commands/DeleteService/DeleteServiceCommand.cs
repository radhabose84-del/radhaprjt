using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Commands.DeleteService
{
    public class DeleteServiceCommand : IRequest<bool>
    {
        public int Id { get; set; }
        
    }
}