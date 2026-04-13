using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Commands
{
    public sealed class UploadDocumentAssetMasterGeneralCommandHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UploadDocumentAssetMasterGeneralCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private UploadDocumentAssetMasterGeneralCommandHandler CreateSut() =>
            new(
                _mockQueryRepo.Object,
                _mockLogger.Object,
                _mockIpAddressService.Object,
                _mockCompanyLookup.Object,
                _mockUnitLookup.Object);

        private static IFormFile CreateMockFile(string fileName = "doc.pdf", long length = 1024)
        {
            var mock = new Mock<IFormFile>(MockBehavior.Loose);
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(length);
            return mock.Object;
        }

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadDocumentAssetMasterGeneralCommand { File = null };

            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));

            ex.Message.Should().Contain("No file uploaded");
        }

        [Fact]
        public async Task Handle_EmptyFile_ThrowsValidationException()
        {
            var emptyFile = new Mock<IFormFile>(MockBehavior.Loose);
            emptyFile.Setup(f => f.Length).Returns(0);

            var command = new UploadDocumentAssetMasterGeneralCommand { File = emptyFile.Object };

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DocumentDirectoryEmpty_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync(string.Empty);

            var command = new UploadDocumentAssetMasterGeneralCommand
            {
                File = CreateMockFile()
            };

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DocumentDirectoryEmpty_CallsGetDocumentDirectoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync(string.Empty);

            var command = new UploadDocumentAssetMasterGeneralCommand
            {
                File = CreateMockFile()
            };

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockQueryRepo.Verify(r => r.GetDocumentDirectoryAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_NullFile_DoesNotCallRepository()
        {
            var command = new UploadDocumentAssetMasterGeneralCommand { File = null };

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockQueryRepo.Verify(r => r.GetDocumentDirectoryAsync(), Times.Never);
        }
    }
}
