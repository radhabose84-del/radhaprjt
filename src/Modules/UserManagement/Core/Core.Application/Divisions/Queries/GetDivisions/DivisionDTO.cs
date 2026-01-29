using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Core.Domain.Entities;
using Core.Application.Common.Mappings;
using Microsoft.AspNetCore.Http;
using Core.Application.Common;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Divisions.Queries.GetDivisions
{
    public class DivisionDTO
    {
        public int Id { get; set; }
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public int CompanyId { get; set; }
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}