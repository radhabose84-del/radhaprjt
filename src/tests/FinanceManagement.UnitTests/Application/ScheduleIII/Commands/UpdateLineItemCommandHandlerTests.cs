using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class UpdateLineItemCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateLineItemCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateLineItemCommand ValidCommand() =>
            new() { Id = 14, SectionId = 5, LineName = "Inventories (revised)", NoteReference = "Note 8", IsActive = 1 };

        private void SetupHappyPath(int result = 14)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>(It.IsAny<UpdateLineItemCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ScheduleIIISectionItem());
            _mockCommandRepo
                .Setup(r => r.UpdateLineItemAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>()))
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateLineItemAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesUpdateAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update" && e.ActionCode == "S3_LINEITEM_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
