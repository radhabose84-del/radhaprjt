using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class CreateSubTotalCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateSubTotalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateSubTotalCommand ValidCommand() =>
            new()
            {
                FormulaName = "Gross Profit",
                IncludeOtherIncome = false,
                DisplayOrder = 5
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(It.IsAny<CreateSubTotalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ScheduleIIISubTotal());
            _mockCommandRepo
                .Setup(r => r.CreateSubTotalAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessAndId()
        {
            SetupHappyPath(newId: 7);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesMappedHeaderToRepo()
        {
            var mapped = new FinanceManagement.Domain.Entities.ScheduleIIISubTotal { FormulaName = "Gross Profit" };
            FinanceManagement.Domain.Entities.ScheduleIIISubTotal? captured = null;
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(It.IsAny<CreateSubTotalCommand>()))
                .Returns(mapped);
            _mockCommandRepo
                .Setup(r => r.CreateSubTotalAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>()))
                .Callback<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured.Should().BeSameAs(mapped);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_SUBTOTAL_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
