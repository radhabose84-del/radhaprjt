using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.PurchaseOrder.UploadPODocument;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Documents.Commands
{
    public sealed class UploadPODocumentCommandHandlerTests
    {
        private readonly Mock<IPODocumentQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UploadPODocumentCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UploadPODocumentCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullFile_ReturnsFailure()
        {
            var command = new UploadPODocumentCommand { File = null };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No file uploaded");
        }

        [Fact]
        public async Task Handle_EmptyFile_ReturnsFailure()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            var command = new UploadPODocumentCommand { File = mockFile.Object };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
