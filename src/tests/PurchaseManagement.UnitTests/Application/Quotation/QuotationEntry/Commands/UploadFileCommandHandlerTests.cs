using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Item.ItemDetail.Commands.UploadItemImage;
using PurchaseManagement.Application.Quotation.QuotationEntry.Commands.UploadItemImage;

namespace PurchaseManagement.UnitTests.Application.Quotation.QuotationEntry.Commands
{
    public sealed class UploadFileCommandHandlerTests
    {
        private readonly Mock<ILogger<UploadFileCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IQuotationCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private UploadFileCommandHandler CreateSut() =>
            new(_mockLogger.Object, _mockRepo.Object, _mockIp.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object);

        [Fact]
        public async Task Handle_NullFile_ReturnsFailure()
        {
            var command = new UploadFileCommand { File = null };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No file uploaded");
        }

        [Fact]
        public async Task Handle_EmptyFile_ReturnsFailure()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            var command = new UploadFileCommand { File = mockFile.Object };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ReturnsFailure()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            var command = new UploadFileCommand { File = mockFile.Object };
            _mockRepo.Setup(r => r.GetBaseDirectoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Base directory not configured");
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
