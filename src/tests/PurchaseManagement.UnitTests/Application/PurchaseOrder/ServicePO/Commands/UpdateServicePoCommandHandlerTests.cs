using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update;
using System.ComponentModel.DataAnnotations;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Commands
{
    public sealed class UpdateServicePoCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateServicePoCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateServicePoCommandHandler CreateSut() =>
            new(
                _mockMapper.Object, _mockRepo.Object, _mockQueryRepo.Object,
                _mockIp.Object, _mockTz.Object, _mockOutbox.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsValidationException()
        {
            var command = new UpdateServicePoCommand { Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Body is required*");
        }

        [Fact]
        public async Task Handle_ZeroId_ThrowsValidationException()
        {
            var command = new UpdateServicePoCommand
            {
                Data = new PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO.CreateServicePurchaseOrderDto { Id = 0 }
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Service PO id is required*");
        }
    }
}
