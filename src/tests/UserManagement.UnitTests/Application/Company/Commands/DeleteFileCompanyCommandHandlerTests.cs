using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Companies.Commands.DeleteFileCompany;

namespace UserManagement.UnitTests.Application.Company.Commands
{
    public sealed class DeleteFileCompanyCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Strict);

        private DeleteFileCompanyCommandHandler CreateSut() =>
            new(_mockFileUpload.Object);

        [Fact]
        public async Task Handle_ValidLogo_ReturnsTrue()
        {
            var command = new DeleteFileCompanyCommand { Logo = "/path/logo.png" };
            _mockFileUpload.Setup(f => f.DeleteFileAsync("/path/logo.png")).ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InvalidLogo_ReturnsFalse()
        {
            var command = new DeleteFileCompanyCommand { Logo = "/invalid/path.png" };
            _mockFileUpload.Setup(f => f.DeleteFileAsync("/invalid/path.png")).ReturnsAsync(false);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
