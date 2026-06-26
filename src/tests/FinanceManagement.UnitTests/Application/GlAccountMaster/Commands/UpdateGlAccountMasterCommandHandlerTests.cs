using Contracts.Events.Coa;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaRead;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.Common.Interfaces.IIntegrationEvents;
using FinanceManagement.Application.CoaRead.Dto;
using FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Commands
{
    public sealed class UpdateGlAccountMasterCommandHandlerTests
    {
        private readonly Mock<IGlAccountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IGlAccountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IGlobalCoaPropagationService> _mockPropagation = new(MockBehavior.Loose);
        private readonly Mock<ICoaReadQueryRepository> _mockCoaRead = new(MockBehavior.Loose);
        private readonly Mock<IIntegrationEventPublisher> _mockIntegration = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        public UpdateGlAccountMasterCommandHandlerTests() => _mockIp.Setup(s => s.GetCompanyId()).Returns(1);

        private UpdateGlAccountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockPropagation.Object,
                _mockCoaRead.Object, _mockIntegration.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(bool wasActive = true)
        {
            _mockCoaRead.Setup(r => r.GetPostingInfoByIdAsync(1, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountPostingInfo { Id = 5, AccountCode = "1001", AccountName = "Cash", IsActive = wasActive });
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
        public async Task Handle_ActiveAccount_NoDeactivationEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(
                new UpdateGlAccountMasterCommand { Id = 5, AccountName = "Cash", IsActive = 1 },
                CancellationToken.None);

            _mockIntegration.Verify(p => p.PublishWithinSlaAsync(It.IsAny<GlAccountDeactivatedEvent>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ActiveToInactive_PublishesDeactivationEvent()
        {
            SetupHappyPath(wasActive: true);

            await CreateSut().Handle(
                new UpdateGlAccountMasterCommand { Id = 5, AccountName = "Cash", IsActive = 0 },
                CancellationToken.None);

            _mockIntegration.Verify(p => p.PublishWithinSlaAsync(
                It.Is<GlAccountDeactivatedEvent>(e => e.AccountId == 5 && e.AccountCode == "1001" && e.CompanyId == 1),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AlreadyInactive_NoDeactivationEvent()
        {
            SetupHappyPath(wasActive: false);

            await CreateSut().Handle(
                new UpdateGlAccountMasterCommand { Id = 5, AccountName = "Cash", IsActive = 0 },
                CancellationToken.None);

            _mockIntegration.Verify(p => p.PublishWithinSlaAsync(It.IsAny<GlAccountDeactivatedEvent>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
