using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Commands
{
    public sealed class CreateGlAccountMasterCommandHandlerTests
    {
        private readonly Mock<IGlAccountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IGlobalCoaPropagationService> _mockPropagation = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateGlAccountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockPropagation.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 42)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.GlAccountMaster>(It.IsAny<object>()))
                .Returns(new FinanceManagement.Domain.Entities.GlAccountMaster());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.GlAccountMaster>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_StampsSessionCompany_AndReturnsNewId()
        {
            SetupHappyPath(newId: 55);

            var result = await CreateSut().Handle(
                new CreateGlAccountMasterCommand { AccountCode = "1001", AccountName = "Cash" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(55);
        }

        [Fact]
        public async Task Handle_GlobalAccount_FansOutToSubsidiaries()
        {
            SetupHappyPath(newId: 70);

            await CreateSut().Handle(
                new CreateGlAccountMasterCommand { AccountCode = "1001", AccountName = "Cash", IsGlobal = true },
                CancellationToken.None);

            _mockPropagation.Verify(p => p.FanOutNewGlobalAsync(70, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonGlobalAccount_DoesNotFanOut()
        {
            SetupHappyPath();

            await CreateSut().Handle(
                new CreateGlAccountMasterCommand { AccountCode = "1001", AccountName = "Cash", IsGlobal = false },
                CancellationToken.None);

            _mockPropagation.Verify(p => p.FanOutNewGlobalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoActiveCompany_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns((int?)null);

            var act = async () => await CreateSut().Handle(
                new CreateGlAccountMasterCommand { AccountCode = "1001" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*active company*");
        }
    }
}
