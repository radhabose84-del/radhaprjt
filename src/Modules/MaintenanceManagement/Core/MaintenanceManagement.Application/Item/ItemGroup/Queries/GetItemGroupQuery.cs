using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Item.ItemGroup.Queries
{
    public class GetItemGroupQuery : IRequest<List<GetItemGroupDto>>
    {
        public string? OldUnitId { get; set; } 
    }
}