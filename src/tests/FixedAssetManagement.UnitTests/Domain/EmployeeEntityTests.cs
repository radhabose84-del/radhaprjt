using FAM.Domain.Entities.AssetMaster;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class EmployeeEntityTests
    {
        [Fact]
        public void Employee_Properties_ShouldBeAssignable()
        {
            var entity = new Employee
            {
                Empcode = 1001,
                Empname = "John Doe"
            };

            entity.Empcode.Should().Be(1001);
            entity.Empname.Should().Be("John Doe");
        }

        [Fact]
        public void Employee_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Employee();

            entity.Empname.Should().BeNull();
        }

        [Fact]
        public void Employee_Empcode_DefaultShouldBeZero()
        {
            var entity = new Employee();

            entity.Empcode.Should().Be(0);
        }
    }
}
