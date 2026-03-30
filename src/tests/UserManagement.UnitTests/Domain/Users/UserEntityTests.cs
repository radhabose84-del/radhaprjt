using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain.Users
{
    public class UserEntityTests
    {
        // --- Standard entity tests per CLAUDE.md Rule #23 ---

        [Fact]
        public void User_DefaultIsActive_ShouldBeInactive()
        {
            // UserManagement BaseEntity does not initialize IsActive — enum defaults to 0 (Inactive)
            var entity = new User();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void User_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            // IsDelete enum: NotDeleted = 0 (default)
            var entity = new User();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void User_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(User)).Should().BeTrue();
        }

        [Fact]
        public void User_Properties_ShouldBeAssignable()
        {
            var entity = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                EmailId = "john@example.com",
                Mobile = "9876543210",
                DepartmentId = 5,
                UserType = 1,
                IsLocked = 0,
                PartyId = 10,
                EntityId = 3,
                UserGroupId = 2
            };

            entity.UserId.Should().Be(1);
            entity.FirstName.Should().Be("John");
            entity.LastName.Should().Be("Doe");
            entity.UserName.Should().Be("johndoe");
            entity.EmailId.Should().Be("john@example.com");
            entity.Mobile.Should().Be("9876543210");
            entity.DepartmentId.Should().Be(5);
            entity.UserType.Should().Be(1);
            entity.IsLocked.Should().Be(0);
            entity.PartyId.Should().Be(10);
            entity.EntityId.Should().Be(3);
            entity.UserGroupId.Should().Be(2);
        }

        [Fact]
        public void User_NullableProperties_ShouldAcceptNull()
        {
            var entity = new User
            {
                FirstName = null,
                LastName = null,
                UserName = null,
                EmailId = null,
                Mobile = null,
                PasswordHash = null,
                UserType = null,
                PartyId = null,
                EntityId = null,
                UserGroupId = null
            };

            entity.FirstName.Should().BeNull();
            entity.LastName.Should().BeNull();
            entity.UserName.Should().BeNull();
            entity.EmailId.Should().BeNull();
            entity.Mobile.Should().BeNull();
            entity.PasswordHash.Should().BeNull();
            entity.UserType.Should().BeNull();
            entity.PartyId.Should().BeNull();
            entity.EntityId.Should().BeNull();
            entity.UserGroupId.Should().BeNull();
        }

        [Fact]
        public void User_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new User
            {
                UserRoleAllocations = new List<UserRoleAllocation>(),
                Passwords = new List<PasswordLog>(),
                UserCompanies = new List<UserCompany>(),
                UserUnits = new List<UserUnit>(),
                UserDivisions = new List<UserDivision>(),
                UserDepartments = new List<UserDepartment>()
            };

            entity.UserRoleAllocations.Should().NotBeNull();
            entity.Passwords.Should().NotBeNull();
            entity.UserCompanies.Should().NotBeNull();
            entity.UserUnits.Should().NotBeNull();
            entity.UserDivisions.Should().NotBeNull();
            entity.UserDepartments.Should().NotBeNull();
        }

        // --- Password-specific domain logic tests ---

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetPassword_WhenNullOrWhitespace_Throws(string? bad)
        {
            // arrange
            var user = new User();

            // act
            Action act = () => user.SetPassword(bad!);

            // assert
            act.Should().Throw<ArgumentException>()
               .WithParameterName("password")
               .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        public void SetPassword_Stores_BCrypt_Hash_That_Verifies()
        {
            // arrange
            var user = new User();
            var plain = "Secur3P@ss!";

            // act
            user.SetPassword(plain);

            // assert
            user.PasswordHash.Should().NotBeNullOrWhiteSpace();
            // BCrypt hashes typically start with $2a$, $2b$, or $2y$
            user.PasswordHash!.StartsWith("$2").Should().BeTrue("BCrypt hashes start with $2*");
            BCrypt.Net.BCrypt.Verify(plain, user.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public void SetPassword_With_Different_Inputs_Produces_Different_Hashes()
        {
            // arrange
            var user = new User();

            // act
            user.SetPassword("First#123");
            var firstHash = user.PasswordHash!;
            user.SetPassword("Second#456");
            var secondHash = user.PasswordHash!;

            // assert
            firstHash.Should().NotBe(secondHash);
            BCrypt.Net.BCrypt.Verify("First#123", firstHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("Second#456", secondHash).Should().BeTrue();
        }

        [Fact]
        public void SetPassword_Can_Reset_To_New_Value()
        {
            // arrange
            var user = new User();
            user.SetPassword("Old#Pass1");
            var oldHash = user.PasswordHash!;

            // act
            user.SetPassword("New#Pass2");

            // assert
            user.PasswordHash.Should().NotBeNullOrWhiteSpace();
            user.PasswordHash!.Should().NotBe(oldHash);
            BCrypt.Net.BCrypt.Verify("New#Pass2", user.PasswordHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("Old#Pass1", user.PasswordHash).Should().BeFalse();
        }
    }
}
