using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetTransferIssueBuilders
    {
        public static CreateAssetTransferIssueCommand ValidCreateCommand() =>
            new CreateAssetTransferIssueCommand
            {
                AssetTransferIssueHdrDto = new AssetTransferIssueHdrDto
                {
                    DocDate = DateTimeOffset.UtcNow,
                    TransferType = 1,
                    FromUnitId = 1,
                    ToUnitId = 2,
                    FromDepartmentId = 1,
                    ToDepartmentId = 2,
                    FromCustodianId = 1,
                    ToCustodianId = 2
                }
            };

        public static AssetTransferDto ValidDto(int id = 1) =>
            new AssetTransferDto
            {
                Id = id,
                FromUnitId = 1,
                ToUnitId = 2,
                FromDepartmentId = 1,
                ToDepartmentId = 2,
                FromCustodianId = 1,
                ToCustodianId = 2,
                TransferType = 1
            };

        public static GetAllTransferDtlDto ValidDtlDto(int id = 1) =>
            new GetAllTransferDtlDto
            {
                Id = id,
                AssetTransferId = 10,
                AssetId = 1,
                AssetCode = "AST001",
                AssetName = "Test Asset",
                AssetValue = 50000m
            };
    }
}
