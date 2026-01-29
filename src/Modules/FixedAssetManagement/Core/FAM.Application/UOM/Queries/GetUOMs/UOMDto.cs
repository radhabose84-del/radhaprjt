using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.UOM.Queries.GetUOMs
{
    public class UOMDto
    {

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? UOMName { get; set; }
        public int SortOrder { get; set; }
        public int UOMTypeId { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
    }
}