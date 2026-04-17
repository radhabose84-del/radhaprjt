using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.PurchaseOrder.DeletePODocument;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Documents.Commands
{
    public sealed class DeletePODocumentCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileService = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private DeletePODocumentCommandHandler CreateSut() =>
            new(_mockFileService.Object, _mockIp.Object, _mockRepo.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object);

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ReturnsFailure()
        {
            var command = new DeletePODocumentCommand { PODocumentPath = "test.pdf" };
            _mockIp.Setup(i => i.GetCompanyId()).Returns(1);
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockCompanyLookup.Setup(l => l.GetAllCompanyAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.CompanyLookupDto>());
            _mockUnitLookup.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Users.UnitLookupDto?)null);
            _mockRepo.Setup(r => r.GetDocumentDirectoryAsync())
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
