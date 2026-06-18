using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.LockStructure;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class LockStructureCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        public LockStructureCommandHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1001);
            _mockIp.Setup(x => x.GetDivisionId()).Returns(7);
        }

        private LockStructureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_Locked_ReturnsSuccess()
        {
            _mockCommandRepo.Setup(r => r.LockStructureAsync(1001, 7)).ReturnsAsync(true);

            var result = await CreateSut().Handle(new LockStructureCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Locked_PublishesLockAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.LockStructureAsync(1001, 7)).ReturnsAsync(true);

            await CreateSut().Handle(new LockStructureCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Lock" && e.ActionCode == "S3_STRUCTURE_LOCK"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFoundOrNoStatus_ThrowsExceptionRules()
        {
            _mockCommandRepo.Setup(r => r.LockStructureAsync(1001, 7)).ReturnsAsync(false);

            Func<Task> act = async () =>
                await CreateSut().Handle(new LockStructureCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
