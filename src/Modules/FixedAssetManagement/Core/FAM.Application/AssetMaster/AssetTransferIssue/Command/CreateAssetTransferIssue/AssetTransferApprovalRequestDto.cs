namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue
{
    public class AssetTransferApprovalRequestDto
    {
         public DateTimeOffset DocDate { get; set; }
         public int TransferType { get; set; }
         public int FromUnitId { get; set; }
         public string FromUnitName { get; set; } = default!;
         public int ToUnitId { get; set; }
         public string ToUnitName { get; set; } = default!;
         public int FromDepartmentId { get; set; }
         public string FromDepartmentName { get; set; } = default!;
         public int ToDepartmentId { get; set; }
         public string ToDepartmentName { get; set; } = default!;
         public int FromCustodianId { get; set; }
         public string? FromCustodianName { get; set; }
         public int ToCustodianId { get; set; }
         public string? ToCustodianName { get; set; }
         public string? GatePassNo { get; set; }
    }
}