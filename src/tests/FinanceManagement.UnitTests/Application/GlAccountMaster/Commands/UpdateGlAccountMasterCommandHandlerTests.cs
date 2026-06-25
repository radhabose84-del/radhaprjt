using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Commands
{
    public sealed class UpdateGlAccountMasterCommandHandlerTests
    {
        private readonly Mock<IGlAccountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IGlAccountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IGlobalCoaPropagationService> _mockPropagation = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateGlAccountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockPropagation.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.GlAccountMaster>(It.IsAny<object>()))
                .Returns(new FinanceManagement.Domain.Entities.GlAccountMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.GlAccountMaster>()))
                .ReturnsAsync(5);
        }

        [Fact]
        public async Task Handle_PropagatesUpdate_ToCopies()
        {
            SetupHappyPath();

            await CreateSut().Handle(
                new UpdateGlAccountMasterCommand { Id = 5, AccountName = "Cash", IsActive = 1 },
                CancellationToken.None);

            _mockPropagation.Verify(p => p.PropagateUpdateAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateLinkedAccount_Throws_AndDoesNotPropagate()
        {
            _mockQueryRepo.Setup(r => r.IsGlAccountLinkedAsync(5)).ReturnsAsync(true);

            var act = async () => await CreateSut().Handle(
                new UpdateGlAccountMasterCommand { Id = 5, AccountName = "Cash", IsActive = 0 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*linked*");
            _mockPropagation.Verify(p => p.PropagateUpdateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
