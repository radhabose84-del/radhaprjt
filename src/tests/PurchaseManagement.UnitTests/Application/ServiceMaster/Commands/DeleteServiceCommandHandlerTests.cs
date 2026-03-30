using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.UnitTests.TestData;
using MediatR;

namespace PurchaseManagement.UnitTests.Application.ServiceMaster.Commands
{
    public sealed class DeleteServiceCommandHandlerTests
    {
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IServiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteServiceCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath()
        {
            var entity = ServiceMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(It.IsAny<DeleteServiceCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ServiceMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            var entity = ServiceMasterBuilders.ValidEntity();

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(It.IsAny<DeleteServiceCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ServiceMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsSoftDeleteOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ServiceMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ServiceMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
