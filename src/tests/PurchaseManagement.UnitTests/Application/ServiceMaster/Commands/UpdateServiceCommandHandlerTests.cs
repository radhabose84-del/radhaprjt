using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.UnitTests.TestData;
using MediatR;

namespace PurchaseManagement.UnitTests.Application.ServiceMaster.Commands
{
    public sealed class UpdateServiceCommandHandlerTests
    {
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IServiceCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateServiceCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = ServiceMasterBuilders.ValidEntity(id);
            var dto = ServiceMasterBuilders.ValidDto(id);

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(It.IsAny<UpdateServiceCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(id, It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetServiceMasterDto>(entity))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ServiceMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ServiceMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ServiceMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
