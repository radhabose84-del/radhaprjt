using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Invoice.Commands.UpdateInvoice;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Invoice.Commands
{
    public sealed class UpdateInvoiceCommandHandlerTests
    {
        private readonly Mock<IInvoiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateInvoiceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMiscRepo.Object, _mockIpService.Object,
                _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int resultId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<InvoiceHeader>(It.IsAny<UpdateInvoiceCommand>()))
                .Returns(new InvoiceHeader());

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<InvoiceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(resultId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateInvoiceCommand { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateInvoiceCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "INVOICE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
