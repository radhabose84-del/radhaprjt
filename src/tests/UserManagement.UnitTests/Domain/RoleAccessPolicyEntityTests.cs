using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class RoleAccessPolicyEntityTests
    {
        [Fact]
        public void RoleAccessPolicy_Properties_ShouldBeAssignable()
        {
            var entity = new RoleAccessPolicy
            {
                Id             = 1,
                AccessPolicyId = 2,
                RoleId         = 3,
                ValueId        = 10
            };

            entity.Id.Should().Be(1);
            entity.AccessPolicyId.Should().Be(2);
            entity.RoleId.Should().Be(3);
            entity.ValueId.Should().Be(10);
        }

        [Fact]
        public void RoleAccessPolicy_NavigationProperties_AcceptNull()
        {
            var entity = new RoleAccessPolicy
            {
                AccessPolicy = null,
                UserRole     = null
            };

            entity.AccessPolicy.Should().BeNull();
            entity.UserRole.Should().BeNull();
        }

        [Fact]
        public void RoleAccessPolicy_NavigationProperties_AreAssignable()
        {
            var policy = new AccessPolicy { Id = 1, PolicyCode = "AP001", PolicyName = "Test" };
            var entity = new RoleAccessPolicy
            {
                Id             = 1,
                AccessPolicyId = 1,
                RoleId         = 1,
                ValueId        = 5,
                AccessPolicy   = policy
            };

            entity.AccessPolicy.Should().NotBeNull();
            entity.AccessPolicy!.PolicyCode.Should().Be("AP001");
        }

        [Fact]
        public void RoleAccessPolicy_DoesNotInheritBaseEntity()
        {
            typeof(RoleAccessPolicy).BaseType.Should().Be(typeof(object));
        }
    }
}
