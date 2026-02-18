using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.Reports.AssetTransferReport
{
    public class AssetTransferDetailsDto 
    {
        public int TransferId { get; set; }
        public DateTimeOffset DocDate { get; set; }
        public string? TransferTypeDesc { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public int FromUnitId { get; set; }
        public string? FromUnitName { get; set; }
        public int ToUnitId { get; set; }
        public string? ToUnitName { get; set; }
        public int FromDepartmentId { get; set; }
        public string? FromDepartmentName { get; set; }
        public int ToDepartmentId { get; set; }
        public string? ToDepartmentName { get; set; }
        public string? FromCustodianName { get; set; }
        public string? ToCustodianName { get; set; }
        public string? Status { get; set; }
    }
}