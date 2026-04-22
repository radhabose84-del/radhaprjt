using FAM.Domain.Common;
using FAM.Domain.Entities;
using MongoDB.Bson;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class AuditLogsEntityTests
    {
        [Fact]
        public void AuditLogs_ShouldInheritFromAuditLogBase()
        {
            typeof(AuditLogBase).IsAssignableFrom(typeof(AuditLogs)).Should().BeTrue();
        }

        [Fact]
        public void AuditLogs_Properties_ShouldBeAssignable()
        {
            var objectId = ObjectId.GenerateNewId();
            var entity = new AuditLogs
            {
                Id = objectId,
                Action = "Create",
                Details = "Asset created successfully",
                Module = "AssetMaster"
            };

            entity.Id.Should().Be(objectId);
            entity.Action.Should().Be("Create");
            entity.Details.Should().Be("Asset created successfully");
            entity.Module.Should().Be("AssetMaster");
        }

        [Fact]
        public void AuditLogs_NullableProperties_ShouldAcceptNull()
        {
            var entity = new AuditLogs();

            entity.Action.Should().BeNull();
            entity.Details.Should().BeNull();
            entity.Module.Should().BeNull();
        }

        [Fact]
        public void AuditLogs_BaseProperties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new AuditLogs
            {
                MachineName = "SERVER01",
                IPAddress = "192.168.1.1",
                OS = "Windows 11",
                Browser = "Chrome",
                CreatedAt = now,
                CreatedBy = 1,
                CreatedByName = "admin"
            };

            entity.MachineName.Should().Be("SERVER01");
            entity.IPAddress.Should().Be("192.168.1.1");
            entity.OS.Should().Be("Windows 11");
            entity.Browser.Should().Be("Chrome");
            entity.CreatedAt.Should().Be(now);
            entity.CreatedBy.Should().Be(1);
            entity.CreatedByName.Should().Be("admin");
        }
    }
}
