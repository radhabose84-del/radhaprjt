using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory
{
    public class DeleteMaintenanceCategoryCommand : IRequest<int> 
    {
        public int Id { get; set; }
    }
}