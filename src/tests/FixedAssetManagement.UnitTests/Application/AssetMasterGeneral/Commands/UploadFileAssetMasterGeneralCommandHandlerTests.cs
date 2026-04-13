using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Commands
{
    public sealed class UploadFileAssetMasterGeneralCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IAssetMasterGeneralCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UploadFileAssetMasterGeneralCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private UploadFileAssetMasterGeneralCommandHandler CreateSut() =>
            new(
                _mockFileUploadService.Object,
                _mockMediator.Object,
                _mockMapper.Object,
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockLogger.Object,
                _mockIpAddressService.Object,
                _mockCompanyLookup.Object,
                _mockUnitLookup.Object);

        private static IFormFile CreateMockFile(string fileName = "test.jpg", long length = 1024)
        {
            var mock = new Mock<IFormFile>(MockBehavior.Loose);
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(length);
            return mock.Object;
        }

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadFileAssetMasterGeneralCommand { File = null };

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

            var command = new UploadFileAssetMasterGeneralCommand { File = emptyFile.Object };

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BaseDirectoryEmpty_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetBaseDirectoryAsync())
                .ReturnsAsync(string.Empty);

            var command = new UploadFileAssetMasterGeneralCommand
            {
                File = CreateMockFile()
            };

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BaseDirectoryEmpty_CallsGetBaseDirectoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetBaseDirectoryAsync())
                .ReturnsAsync(string.Empty);

            var command = new UploadFileAssetMasterGeneralCommand
            {
                File = CreateMockFile()
            };

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockQueryRepo.Verify(r => r.GetBaseDirectoryAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_NullFile_DoesNotCallRepository()
        {
            var command = new UploadFileAssetMasterGeneralCommand { File = null };

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockQueryRepo.Verify(r => r.GetBaseDirectoryAsync(), Times.Never);
        }
    }
}
