using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.SaveAssetDocument;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FluentValidation;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Commands
{
    public sealed class SaveAssetDocumentCommandHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetMasterGeneralCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UploadDocumentAssetMasterGeneralCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private SaveAssetDocumentCommandHandler CreateSut() =>
            new(
                _mockQueryRepo.Object,
                _mockLogger.Object,
                _mockIpAddressService.Object,
                _mockCommandRepo.Object,
                _mockCompanyLookup.Object,
                _mockUnitLookup.Object);

        private void SetupLookupsAndDirectory()
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

            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync("documents");
        }

        [Fact]
        public async Task Handle_NullAssetPath_ThrowsValidationException()
        {
            var command = new SaveAssetDocumentCommand
            {
                Id = 1,
                AssetCode = "AST001",
                assetPath = null
            };

            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));

            ex.Message.Should().Contain("No file uploaded");
        }

        [Fact]
        public async Task Handle_EmptyAssetPath_ThrowsValidationException()
        {
            var command = new SaveAssetDocumentCommand
            {
                Id = 1,
                AssetCode = "AST001",
                assetPath = string.Empty
            };

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidAssetPath_ReturnsTrue()
        {
            SetupLookupsAndDirectory();

            var command = new SaveAssetDocumentCommand
            {
                Id = 1,
                AssetCode = "AST001",
                assetPath = "nonexistent-temp-file.pdf"
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidAssetPath_CallsGetDocumentDirectoryOnce()
        {
            SetupLookupsAndDirectory();

            var command = new SaveAssetDocumentCommand
            {
                Id = 1,
                AssetCode = "AST001",
                assetPath = "nonexistent-temp-file.pdf"
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetDocumentDirectoryAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_NullAssetPath_DoesNotCallRepository()
        {
            var command = new SaveAssetDocumentCommand
            {
                Id = 1,
                AssetCode = "AST001",
                assetPath = null
            };

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockQueryRepo.Verify(r => r.GetDocumentDirectoryAsync(), Times.Never);
        }
    }
}
