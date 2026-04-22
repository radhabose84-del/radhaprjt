using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AssetAuditEntityTests
    {
        [Fact]
        public void AssetAudit_Properties_ShouldBeAssignable()
        {
            var auditDate = DateTimeOffset.UtcNow;
            var entity = new AssetAudit
            {
                Id = 1,
                AssetCode = "AST001",
                AssetName = "Laptop",
                UnitName = "Unit A",
                Department = "IT",
                AuditorName = "John Doe",
                AuditDate = auditDate,
                AuditFinancialYear = "2025-2026",
                AuditTypeId = 10,
                UploadedFileId = 5,
                SourceFileName = "scan.pdf",
                ScanType = "Barcode",
                Status = "Completed",
                UnitId = 3,
                CreatedBy = 1,
                CreatedDate = auditDate,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1"
            };

            entity.Id.Should().Be(1);
            entity.AssetCode.Should().Be("AST001");
            entity.AssetName.Should().Be("Laptop");
            entity.UnitName.Should().Be("Unit A");
            entity.Department.Should().Be("IT");
            entity.AuditorName.Should().Be("John Doe");
            entity.AuditDate.Should().Be(auditDate);
            entity.AuditFinancialYear.Should().Be("2025-2026");
            entity.AuditTypeId.Should().Be(10);
            entity.UploadedFileId.Should().Be(5);
            entity.SourceFileName.Should().Be("scan.pdf");
            entity.ScanType.Should().Be("Barcode");
            entity.Status.Should().Be("Completed");
            entity.UnitId.Should().Be(3);
            entity.CreatedBy.Should().Be(1);
            entity.CreatedDate.Should().Be(auditDate);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public void AssetAudit_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AssetAudit
            {
                UnitId = 1,
                AuditDate = DateTimeOffset.UtcNow
            };

            entity.AssetCode.Should().BeNull();
            entity.AssetName.Should().BeNull();
            entity.UnitName.Should().BeNull();
            entity.Department.Should().BeNull();
            entity.AuditorName.Should().BeNull();
            entity.AuditFinancialYear.Should().BeNull();
            entity.AuditTypeId.Should().BeNull();
            entity.UploadedFileId.Should().BeNull();
            entity.SourceFileName.Should().BeNull();
            entity.ScanType.Should().BeNull();
            entity.Status.Should().BeNull();
            entity.CreatedBy.Should().BeNull();
            entity.CreatedDate.Should().BeNull();
            entity.CreatedByName.Should().BeNull();
            entity.CreatedIP.Should().BeNull();
        }

        [Fact]
        public void AssetAudit_NavigationProperty_ShouldBeAssignable()
        {
            var miscMaster = new MiscMaster();
            var entity = new AssetAudit
            {
                AuditDate = DateTimeOffset.UtcNow,
                UnitId = 1,
                AuditTypeMiscType = miscMaster
            };

            entity.AuditTypeMiscType.Should().BeSameAs(miscMaster);
        }
    }
}
