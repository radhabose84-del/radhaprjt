using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Application.MRS.Queries.GetMrsPending
{
    public class MrsPendingDto
    {
         public int Id { get; set; }
        public string? MrsNo { get; set; }
        public DateTimeOffset MrsDate { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }

        // Request Category Info
        public int RequestCategoryId { get; set; }
        public string? RequestCategoryName { get; set; }

        // Status Info
        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        // Department Info
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int SubDepartmentId { get; set; }
        public string? SubDepartmentName { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public byte IsEdit { get; set; }
        public List<GetMrsPendingDetailDto> MrsDetails { get; set; } = new();
        public class GetMrsPendingDetailDto
        {
        public int MrsDetailId { get; set; }
        public int MrsHeaderId { get; set; }
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public string? UomName { get; set; }
        public decimal RequestQuantity { get; set; }
        public int? CostCenterId { get; set; }
        public int? FinanceCode { get; set; }
     }
    }
}