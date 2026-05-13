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
            var bytes = new byte[] { 0x01, 0x02, 0x03 };
            var entity = new UserSignature
            {
                Id = 5,
                UserId = 10,
                SignatureImage = bytes,
                FileName = "sig.png",
                ContentType = "image/png",
                FileSizeBytes = bytes.Length
            };

            entity.Id.Should().Be(5);
            entity.UserId.Should().Be(10);
            entity.SignatureImage.Should().BeEquivalentTo(bytes);
            entity.FileName.Should().Be("sig.png");
            entity.ContentType.Should().Be("image/png");
            entity.FileSizeBytes.Should().Be(3);
        }

        [Fact]
        public void UserSignature_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UserSignature
            {
                SignatureImage = null,
                FileName = null,
                ContentType = null
            };

            entity.SignatureImage.Should().BeNull();
            entity.FileName.Should().BeNull();
            entity.ContentType.Should().BeNull();
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
