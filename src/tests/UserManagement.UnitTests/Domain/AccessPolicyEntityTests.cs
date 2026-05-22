using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class AccessPolicyEntityTests
    {
        [Fact]
        public void AccessPolicy_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AccessPolicy();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AccessPolicy_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AccessPolicy();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AccessPolicy_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AccessPolicy)).Should().BeTrue();
        }

        [Fact]
        public void AccessPolicy_Properties_ShouldBeAssignable()
        {
            var entity = new AccessPolicy
            {
                Id         = 1,
                PolicyCode = "AP001",
                PolicyName = "Test Policy",
                EntityName = "SalesOrder",
                FieldName  = "SalesOrderTypeId"
            };

            entity.Id.Should().Be(1);
            entity.PolicyCode.Should().Be("AP001");
            entity.PolicyName.Should().Be("Test Policy");
            entity.EntityName.Should().Be("SalesOrder");
            entity.FieldName.Should().Be("SalesOrderTypeId");
        }

        [Fact]
        public void AccessPolicy_RoleAccessPolicies_DefaultsToEmptyCollection()
        {
            var entity = new AccessPolicy();
            entity.RoleAccessPolicies.Should().NotBeNull();
            entity.RoleAccessPolicies.Should().BeEmpty();
        }

        [Fact]
        public void AccessPolicy_RoleAccessPolicies_IsAssignable()
        {
            var entity = new AccessPolicy();
            var child = new RoleAccessPolicy { Id = 1, AccessPolicyId = 1, RoleId = 1, ValueId = 10 };
            entity.RoleAccessPolicies.Add(child);

            entity.RoleAccessPolicies.Should().HaveCount(1);
        }
    }
}
