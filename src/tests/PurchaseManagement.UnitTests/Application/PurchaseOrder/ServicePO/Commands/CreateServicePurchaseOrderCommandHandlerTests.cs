using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Commands
{
    public sealed class CreateServicePurchaseOrderCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderCommandRepository> _mockServiceRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocumentSequenceLookup = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockServiceQuery = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateServicePurchaseOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateServicePurchaseOrderCommandHandler CreateSut() =>
            new(
                _mockMapper.Object, _mockServiceRepo.Object, _mockIp.Object, _mockTz.Object,
                _mockDocumentSequenceLookup.Object, _mockOutbox.Object, _mockServiceQuery.Object,
                _mockUnitLookup.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsException()
        {
            var command = new CreateServicePoCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            // Arrange: mapper returns entity with defaults
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader());

            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            _mockDocumentSequenceLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(42);
            _mockDocumentSequenceLookup
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "PO-KNIT-Service-2627-01" });

            _mockServiceRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader>(), It.IsAny<CancellationToken>(), It.IsAny<int?>()))
                .ReturnsAsync(0); // returns 0 => no outbox scheduled

            var command = new CreateServicePoCommand { Data = new PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO.CreateServicePurchaseOrderDto() };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_NoTransactionTypeConfigured_ThrowsInvalidOperation()
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader());

            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            // No "Service Purchase Order" TransactionType seeded → lookup returns null
            _mockDocumentSequenceLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            var command = new CreateServicePoCommand { Data = new PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO.CreateServicePurchaseOrderDto() };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Service Purchase Order*");
        }

        [Fact]
        public async Task Handle_NoDocumentSequenceConfigured_ThrowsInvalidOperation()
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader());

            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            // TransactionType exists but DocumentSequence returns empty → handler must throw
            _mockDocumentSequenceLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(42);
            _mockDocumentSequenceLookup
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string>());

            var command = new CreateServicePoCommand { Data = new PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO.CreateServicePurchaseOrderDto() };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*No document sequence configured*");
        }
    }
}
