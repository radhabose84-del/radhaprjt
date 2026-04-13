using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteDocumentAssetMasterGeneral;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Commands
{
    public sealed class DeleteDocumentAssetMasterGeneralCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService = new(MockBehavior.Strict);
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteDocumentAssetMasterGeneralCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Loose);
        private readonly Mock<IAssetMasterGeneralCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private DeleteDocumentAssetMasterGeneralCommandHandler CreateSut() =>
            new(
                _mockFileUploadService.Object,
                _mockQueryRepo.Object,
                _mockLogger.Object,
                _mockIpAddressService.Object,
                _mockCommandRepo.Object,
                _mockCompanyLookup.Object,
                _mockUnitLookup.Object);

        private void SetupLookups()
        {
            _mockIpAddressService.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIpAddressService.Setup(s => s.GetUnitId()).Returns(1);

            _mockCompanyLookup
                .Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new CompanyLookupDto { CompanyId = 1, CompanyName = "Test Co" }
                });

            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Unit A" }
                });
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupLookups();

            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync("documents");

            _mockFileUploadService
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.RemoveAssetDocumentReferenceAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteDocumentAssetMasterGeneralCommand { assetPath = "doc.pdf" };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteFileOnce()
        {
            SetupLookups();

            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync("documents");

            _mockFileUploadService
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.RemoveAssetDocumentReferenceAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteDocumentAssetMasterGeneralCommand { assetPath = "doc.pdf" };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockFileUploadService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsRemoveDocumentReferenceOnce()
        {
            SetupLookups();

            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync("documents");

            _mockFileUploadService
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.RemoveAssetDocumentReferenceAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteDocumentAssetMasterGeneralCommand { assetPath = "doc.pdf" };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.RemoveAssetDocumentReferenceAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DocumentDirectoryEmpty_ThrowsException()
        {
            SetupLookups();

            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync(string.Empty);

            var command = new DeleteDocumentAssetMasterGeneralCommand { assetPath = "doc.pdf" };

            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeleteFileReturnsFalse_ThrowsException()
        {
            SetupLookups();

            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync("documents");

            _mockFileUploadService
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.RemoveAssetDocumentReferenceAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteDocumentAssetMasterGeneralCommand { assetPath = "doc.pdf" };

            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
