using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteFileAssetWarranty;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FluentValidation;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Commands
{
    public sealed class DeleteFileAssetWarrantyCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService = new(MockBehavior.Strict);
        private readonly Mock<IAssetWarrantyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteFileAssetWarrantyCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockGeneralQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IAssetWarrantyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        public DeleteFileAssetWarrantyCommandHandlerTests()
        {
            _mockCompanyLookup
                .Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new CompanyLookupDto { CompanyId = 1, CompanyName = "TestCompany" }
                });

            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "TestUnit" }
                });

            _mockIpService.Setup(i => i.GetCompanyId()).Returns(1);
            _mockIpService.Setup(i => i.GetUnitId()).Returns(1);
        }

        private DeleteFileAssetWarrantyCommandHandler CreateSut() =>
            new(
                _mockFileUploadService.Object,
                _mockCommandRepo.Object,
                _mockLogger.Object,
                _mockIpService.Object,
                _mockGeneralQueryRepo.Object,
                _mockQueryRepo.Object,
                _mockCompanyLookup.Object,
                _mockUnitLookup.Object);

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.GetBaseDirectoryAsync()).ReturnsAsync(string.Empty);

            var command = new DeleteFileAssetWarrantyCommand { assetPath = "file.png" };
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SuccessfulDelete_ReturnsTrue()
        {
            _mockQueryRepo.Setup(r => r.GetBaseDirectoryAsync()).ReturnsAsync("WarrantyFiles");
            _mockFileUploadService
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.RemoveAssetWarrantyAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteFileAssetWarrantyCommand { assetPath = "file.png" };
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SuccessfulDelete_CallsDeleteFileOnce()
        {
            _mockQueryRepo.Setup(r => r.GetBaseDirectoryAsync()).ReturnsAsync("WarrantyFiles");
            _mockFileUploadService
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.RemoveAssetWarrantyAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteFileAssetWarrantyCommand { assetPath = "file.png" };
            await CreateSut().Handle(command, CancellationToken.None);

            _mockFileUploadService.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsException()
        {
            _mockQueryRepo.Setup(r => r.GetBaseDirectoryAsync()).ReturnsAsync("WarrantyFiles");
            _mockFileUploadService
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.RemoveAssetWarrantyAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteFileAssetWarrantyCommand { assetPath = "file.png" };
            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
