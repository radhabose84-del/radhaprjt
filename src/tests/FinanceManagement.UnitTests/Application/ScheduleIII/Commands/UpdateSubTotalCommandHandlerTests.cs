using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class UpdateSubTotalCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateSubTotalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateSubTotalCommand ValidCommand() =>
            new()
            {
                Id = 2,
                FormulaName = "EBITDA",
                IncludeOtherIncome = true,
                DisplayOrder = 3,
                IsActive = 1
            };

        private void SetupHappyPath(
            FinanceManagement.Domain.Entities.ScheduleIIISubTotal? mapped = null, int result = 2)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(It.IsAny<UpdateSubTotalCommand>()))
                .Returns(mapped ?? new FinanceManagement.Domain.Entities.ScheduleIIISubTotal());
            _mockCommandRepo
                .Setup(r => r.UpdateSubTotalAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>()))
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
        public async Task Handle_ValidCommand_PassesMappedHeaderToRepo()
        {
            var mapped = new FinanceManagement.Domain.Entities.ScheduleIIISubTotal { Id = 2, IncludeOtherIncome = true };
            FinanceManagement.Domain.Entities.ScheduleIIISubTotal? captured = null;
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(It.IsAny<UpdateSubTotalCommand>()))
                .Returns(mapped);
            _mockCommandRepo
                .Setup(r => r.UpdateSubTotalAsync(It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>()))
                .Callback<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(e => captured = e)
                .ReturnsAsync(2);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured.Should().BeSameAs(mapped);
            captured!.IncludeOtherIncome.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesUpdateAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_SUBTOTAL_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
