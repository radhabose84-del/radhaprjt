using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Arrival.Commands.UpdateArrival;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Entities.Arrival;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.Arrival.Commands
{
    public sealed class UpdateArrivalCommandHandlerTests
    {
        private readonly Mock<IArrivalCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateArrivalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper.Setup(m => m.Map<ArrivalHeader>(It.IsAny<object>())).Returns(ArrivalBuilders.ValidEntity(id));
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<ArrivalHeader>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(id);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ArrivalBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ArrivalBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<ArrivalHeader>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ArrivalBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
