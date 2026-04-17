using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Commands
{
    public sealed class CreateServiceEntrySheetCommandHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderCommandRepository> _mockServiceRepo = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockServiceQuery = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateServiceEntrySheetCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateServiceEntrySheetCommandHandler CreateSut() =>
            new(
                _mockServiceRepo.Object, _mockServiceQuery.Object, _mockMapper.Object,
                _mockMediator.Object, _mockIp.Object, _mockTz.Object,
                _mockOutbox.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullPayload_ThrowsExceptionRules()
        {
            var command = new CreateServiceEntrySheetCommand { CreateServiceSheet = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Payload is required*");
        }

        [Fact]
        public async Task Handle_PONotFound_ThrowsExceptionRules()
        {
            var dto = new CreateServiceSheetDto { PurchaseOrderId = 999, VendorId = 1 };

            _mockServiceQuery
                .Setup(r => r.GetServicePOHeaderForSesAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServicePOHeaderForSesDto?)null);

            var command = new CreateServiceEntrySheetCommand { CreateServiceSheet = dto };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*PO 999 not found*");
        }

        [Fact]
        public async Task Handle_VendorMismatch_ThrowsExceptionRules()
        {
            var dto = new CreateServiceSheetDto { PurchaseOrderId = 1, VendorId = 99 };

            _mockServiceQuery
                .Setup(r => r.GetServicePOHeaderForSesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServicePOHeaderForSesDto { VendorId = 1 });

            var command = new CreateServiceEntrySheetCommand { CreateServiceSheet = dto };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Vendor mismatch*");
        }
    }
}
