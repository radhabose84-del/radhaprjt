using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Item.ItemGroup.Queries
{
    public class GetItemGroupQuery : IRequest<List<GetItemGroupDto>>
    {
        public string? OldUnitId { get; set; } 
    }
}