using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotation.QuotationEntry.Commands.DeleteImage;

namespace PurchaseManagement.UnitTests.Application.Quotation.QuotationEntry.Commands
{
    public sealed class DeleteFileCommandHandlerTests
    {
        private readonly Mock<ILogger<DeleteFileCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IQuotationCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IFileUploadService> _mockFileService = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private DeleteFileCommandHandler CreateSut() =>
            new(_mockLogger.Object, _mockRepo.Object, _mockFileService.Object,
                _mockIp.Object, _mockUnitLookup.Object, _mockCompanyLookup.Object);

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsException()
        {
            var command = new DeleteFileCommand { imagePath = "test.jpg" };
            _mockRepo.Setup(r => r.GetBaseDirectoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);
            _mockIp.Setup(i => i.GetCompanyId()).Returns(1);
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockCompanyLookup.Setup(l => l.GetAllCompanyAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.CompanyLookupDto>());
            _mockUnitLookup.Setup(l => l.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("*Base directory not configured*");
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
