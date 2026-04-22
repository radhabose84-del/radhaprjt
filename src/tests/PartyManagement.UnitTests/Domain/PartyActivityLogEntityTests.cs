using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyActivityLogEntityTests
    {
        [Fact]
        public void PartyActivityLog_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PartyActivityLog)).Should().BeFalse();
        }

        [Fact]
        public void PartyActivityLog_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new PartyActivityLog
            {
                Id = 1,
                PartyId = 10,
                TableName = "PartyMaster",
                ColumnName = "PartyName",
                OldValue = "Old Name",
                NewValue = "New Name",
                ActionType = "Update",
                ChangedBy = 1,
                ChangedByName = "admin",
                ChangedIp = "127.0.0.1",
                ChangedOn = now
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.TableName.Should().Be("PartyMaster");
            entity.ColumnName.Should().Be("PartyName");
            entity.OldValue.Should().Be("Old Name");
            entity.NewValue.Should().Be("New Name");
            entity.ActionType.Should().Be("Update");
            entity.ChangedBy.Should().Be(1);
            entity.ChangedByName.Should().Be("admin");
            entity.ChangedIp.Should().Be("127.0.0.1");
            entity.ChangedOn.Should().Be(now);
        }

        [Fact]
        public void PartyActivityLog_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PartyActivityLog
            {
                ColumnName = null,
                OldValue = null,
                NewValue = null
            };

            entity.ColumnName.Should().BeNull();
            entity.OldValue.Should().BeNull();
            entity.NewValue.Should().BeNull();
        }

        [Fact]
        public void PartyActivityLog_DefaultStrings_ShouldBeEmpty()
        {
            var entity = new PartyActivityLog();

            entity.TableName.Should().Be(string.Empty);
            entity.ActionType.Should().Be(string.Empty);
            entity.ChangedByName.Should().Be(string.Empty);
            entity.ChangedIp.Should().Be(string.Empty);
        }
    }
}
