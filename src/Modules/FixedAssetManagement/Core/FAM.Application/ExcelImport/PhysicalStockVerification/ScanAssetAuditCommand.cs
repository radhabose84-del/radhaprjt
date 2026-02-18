using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.ExcelImport.PhysicalStockVerification
{
    public class ScanAssetAuditCommand : IRequest<ApiResponseDTO<bool>>
    {
        public string? UnitName { get; set; }
        public int AuditCycle { get; set; }
        public string? DepartmentName { get; set; }
        public string? AssetCode { get; set; }

    }
}