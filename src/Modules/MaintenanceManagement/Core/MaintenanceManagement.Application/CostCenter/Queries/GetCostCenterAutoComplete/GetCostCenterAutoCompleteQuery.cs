using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterAutoComplete
{
    public class GetCostCenterAutoCompleteQuery : IRequest<List<CostCenterAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}