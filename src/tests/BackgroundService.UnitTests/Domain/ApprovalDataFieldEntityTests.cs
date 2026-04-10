using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalDataFieldEntityTests
    {
        [Fact]
        public void ApprovalDataField_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ApprovalDataField();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ApprovalDataField_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ApprovalDataField();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ApprovalDataField_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ApprovalDataField)).Should().BeTrue();
        }

        [Fact]
        public void ApprovalDataField_Properties_ShouldBeAssignable()
        {
            var entity = new ApprovalDataField
            {
                Id = 1,
                FieldKey = "TotalAmount",
                JsonPath = "$.header.totalAmount",
                ValueTypeId = 2,
                ScopeId = 3
            };

            entity.Id.Should().Be(1);
            entity.FieldKey.Should().Be("TotalAmount");
            entity.JsonPath.Should().Be("$.header.totalAmount");
            entity.ValueTypeId.Should().Be(2);
            entity.ScopeId.Should().Be(3);
        }
    }
}
