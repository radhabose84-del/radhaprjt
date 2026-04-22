using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetTransferIssueApprovalEntityTests
    {
        [Fact]
        public void AssetTransferIssueApproval_Properties_ShouldBeAssignable()
        {
            var docDate = DateTimeOffset.UtcNow;
            var entity = new AssetTransferIssueApproval
            {
                Id = 1,
                DocDate = docDate,
                TransferType = "Internal",
                FromUnitname = "Unit A",
                ToUnitname = "Unit B",
                FromDepartment = "IT",
                ToDepartment = "Finance",
                FromCustodianId = 10,
                ToCustodianId = 20,
                FromCustodianName = "John",
                ToCustodianName = "Jane",
                Status = "Pending",
                AssetId = 5,
                AssetCode = "AST001",
                AssetName = "Laptop",
                AssetValue = 75000m
            };

            entity.Id.Should().Be(1);
            entity.DocDate.Should().Be(docDate);
            entity.TransferType.Should().Be("Internal");
            entity.FromUnitname.Should().Be("Unit A");
            entity.ToUnitname.Should().Be("Unit B");
            entity.FromDepartment.Should().Be("IT");
            entity.ToDepartment.Should().Be("Finance");
            entity.FromCustodianId.Should().Be(10);
            entity.ToCustodianId.Should().Be(20);
            entity.FromCustodianName.Should().Be("John");
            entity.ToCustodianName.Should().Be("Jane");
            entity.Status.Should().Be("Pending");
            entity.AssetId.Should().Be(5);
            entity.AssetCode.Should().Be("AST001");
            entity.AssetName.Should().Be("Laptop");
            entity.AssetValue.Should().Be(75000m);
        }

        [Fact]
        public void AssetTransferIssueApproval_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetTransferIssueApproval
            {
                DocDate = DateTimeOffset.UtcNow
            };

            entity.TransferType.Should().BeNull();
            entity.FromUnitname.Should().BeNull();
            entity.ToUnitname.Should().BeNull();
            entity.FromDepartment.Should().BeNull();
            entity.ToDepartment.Should().BeNull();
            entity.FromCustodianName.Should().BeNull();
            entity.ToCustodianName.Should().BeNull();
            entity.Status.Should().BeNull();
            entity.AssetCode.Should().BeNull();
            entity.AssetName.Should().BeNull();
        }
    }
}
