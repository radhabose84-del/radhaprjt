using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.ExcelImport.MiscMaster
{
    public class MiscMasterImportDto
    {
          public int Id { get; set; }
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        
    }
}