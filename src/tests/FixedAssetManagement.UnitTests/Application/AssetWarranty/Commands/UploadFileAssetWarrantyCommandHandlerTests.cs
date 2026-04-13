using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UploadAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Commands
{
    public sealed class UploadFileAssetWarrantyCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IAssetWarrantyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetWarrantyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UploadFileAssetWarrantyCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockGeneralQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private UploadFileAssetWarrantyCommandHandler CreateSut() =>
            new(
                _mockFileUploadService.Object,
                _mockMediator.Object,
                _mockMapper.Object,
                _mockCommandRepo.Object,
                _mockLogger.Object,
                _mockQueryRepo.Object,
                _mockIpService.Object,
                _mockGeneralQueryRepo.Object,
                _mockCompanyLookup.Object,
                _mockUnitLookup.Object);

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadFileAssetWarrantyCommand { File = null, AssetCode = "AST001" };
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyFile_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            var command = new UploadFileAssetWarrantyCommand { File = mockFile.Object, AssetCode = "AST001" };
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyAssetCode_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            var command = new UploadFileAssetWarrantyCommand { File = mockFile.Object, AssetCode = "" };
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_AssetNotFound_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);

            _mockCommandRepo
                .Setup(r => r.GetByAssetCodeAsync(It.IsAny<string>()))
                .ReturnsAsync((AssetWarrantyDTO?)null);

            var command = new UploadFileAssetWarrantyCommand { File = mockFile.Object, AssetCode = "AST001" };
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_AssetNotFound_DoesNotCallFileUploadService()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);

            _mockCommandRepo
                .Setup(r => r.GetByAssetCodeAsync(It.IsAny<string>()))
                .ReturnsAsync((AssetWarrantyDTO?)null);

            var command = new UploadFileAssetWarrantyCommand { File = mockFile.Object, AssetCode = "AST001" };
            var sut = CreateSut();

            try { await sut.Handle(command, CancellationToken.None); } catch { /* expected */ }

            _mockFileUploadService.Verify(
                f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
