using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered
{
    public class AssetTransferDto
    {
        public int Id { get; set; }
        public DateTimeOffset DocDate { get; set; }
        public int TransferType { get; set; }
        public string? TransferTypeName { get; set; }
        public int FromUnitId { get; set; }
        public string? FromUnitName { get; set; }
        public int ToUnitId { get; set; }
        public string? ToUnitName { get; set; }
        public int FromDepartmentId { get; set; }
        public string? FromDepartmentName { get; set; }
        public int ToDepartmentId { get; set; }
        public string? ToDepartmentName { get; set; }
        public int FromCustodianId { get; set; }
        public int ToCustodianId { get; set; }
        public string? Status { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedIP { get; set; }
        public string? ModifiedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedIP { get; set; }
        public string? AuthorizedByName { get; set; }
        public DateTimeOffset? AuthorizedDate { get; set; }
        public string? AuthorizedIP { get; set; }
        public string? FromCustodianName { get; set; }
        public string? ToCustodianName { get; set; }
        public byte AckStatus { get; set; }
        public string? GatePassNo { get; set; }
        
    }
}