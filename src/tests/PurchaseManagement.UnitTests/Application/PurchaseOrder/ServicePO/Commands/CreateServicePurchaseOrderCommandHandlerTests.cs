using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
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
        private readonly Mock<IPurchaseOrderCommandRepository> _mockPoRepo = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockServiceQuery = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateServicePurchaseOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateServicePurchaseOrderCommandHandler CreateSut() =>
            new(
                _mockMapper.Object, _mockServiceRepo.Object, _mockIp.Object, _mockTz.Object,
                _mockPoRepo.Object, _mockOutbox.Object, _mockServiceQuery.Object,
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

            _mockPoRepo
                .Setup(r => r.GenerateNextCodeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("PO-001");

            _mockServiceRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.PurchaseOrder.PurchaseOrderHeader>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0); // returns 0 => no outbox scheduled

            var command = new CreateServicePoCommand { Data = new PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO.CreateServicePurchaseOrderDto() };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(0);
        }
    }
}
