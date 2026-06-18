using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class CreateLineItemCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateLineItemCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateLineItemCommand ValidCommand() =>
            new() { SectionId = 5, LineCode = "INV", LineName = "Inventories" };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>(It.IsAny<CreateLineItemCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ScheduleIIISectionItem());
            _mockCommandRepo
                .Setup(r => r.CreateLineItemAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateLineItemAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.ActionCode == "S3_LINEITEM_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
