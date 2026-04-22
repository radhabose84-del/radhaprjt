using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.UpdateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Commands
{
    public sealed class UpdateServiceEntrySheetCommandHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderCommandRepository> _mockServiceRepo = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockServiceQuery = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateServiceEntrySheetCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateServiceEntrySheetCommandHandler CreateSut() =>
            new(
                _mockServiceRepo.Object, _mockServiceQuery.Object, _mockMapper.Object,
                _mockMediator.Object, _mockMisc.Object, _mockOutbox.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsExceptionRules()
        {
            var command = new UpdateServiceEntrySheetCommand { Id = 1, Data = null! };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Payload is required*");
        }

        [Fact]
        public async Task Handle_ZeroId_ThrowsExceptionRules()
        {
            var command = new UpdateServiceEntrySheetCommand
            {
                Id = 0,
                Data = new CreateServiceSheetDto()
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Valid Service Entry Sheet Id is required*");
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsExceptionRules()
        {
            _mockServiceRepo
                .Setup(r => r.GetServiceEntrySheetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO.ServiceEntrySheet?)null);

            var command = new UpdateServiceEntrySheetCommand
            {
                Id = 999,
                Data = new CreateServiceSheetDto()
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Service Entry Sheet 999 not found*");
        }
    }
}
