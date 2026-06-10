using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.FreightRfq.Commands
{
    public sealed class CreateFreightRfqCommandHandlerTests
    {
        private readonly Mock<IFreightRfqCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateFreightRfqCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(It.IsAny<object>()))
                .Returns(FreightRfqBuilders.ValidEntity(newId));
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>()))
                .ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessWithNewId()
        {
            SetupHappyPath(newId: 42);

            var result = await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
