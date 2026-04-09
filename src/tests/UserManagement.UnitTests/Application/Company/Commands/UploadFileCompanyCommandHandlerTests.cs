using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Companies.Commands.UploadFileCompany;

namespace UserManagement.UnitTests.Application.Company.Commands
{
    public sealed class UploadFileCompanyCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UploadFileCompanyCommandHandler CreateSut() =>
            new(_mockFileUpload.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
