using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class UserSignatureEntityTests
    {
        [Fact]
        public void UserSignature_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new UserSignature();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UserSignature_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new UserSignature();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UserSignature_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UserSignature)).Should().BeTrue();
        }

        [Fact]
        public void UserSignature_Properties_ShouldBeAssignable()
        {
            var entity = new UserSignature
            {
                Id = 5,
                UserId = 10,
                FileName = "vishal-10.png",
                OriginalFileName = "output-onlinepngtools.png",
                FilePath = "Resources\\UserManagement\\UserSignatures\\vishal-10.png",
                FileType = "image/png",
                FileSize = 51498
            };

            entity.Id.Should().Be(5);
            entity.UserId.Should().Be(10);
            entity.FileName.Should().Be("vishal-10.png");
            entity.OriginalFileName.Should().Be("output-onlinepngtools.png");
            entity.FilePath.Should().Be("Resources\\UserManagement\\UserSignatures\\vishal-10.png");
            entity.FileType.Should().Be("image/png");
            entity.FileSize.Should().Be(51498);
        }

        [Fact]
        public void UserSignature_NullableStringProperties_ShouldAcceptNull()
        {
            var entity = new UserSignature
            {
                FileName = null,
                OriginalFileName = null,
                FilePath = null,
                FileType = null
            };

            entity.FileName.Should().BeNull();
            entity.OriginalFileName.Should().BeNull();
            entity.FilePath.Should().BeNull();
            entity.FileType.Should().BeNull();
        }

        [Fact]
        public void UserSignature_NavigationProperty_User_ShouldBeAssignable()
        {
            var user = new User
            {
                UserId = 7,
                FirstName = "Jane",
                LastName = "Doe",
                EmailId = "jane@example.com"
            };

            var entity = new UserSignature
            {
                UserId = 7,
                User = user
            };

            entity.User.Should().NotBeNull();
            entity.User.UserId.Should().Be(7);
            entity.User.FirstName.Should().Be("Jane");
        }
    }
}
