using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter
{
    public class UpdateCostCenterCommand :IRequest<int>
    {
        public int Id { get; set; }
        public string? CostCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public string? ResponsiblePerson { get; set; }
        public decimal? BudgetAllocated { get; set; }
        public string? Remarks { get; set; }
        public byte IsActive { get; set; }
    }
}