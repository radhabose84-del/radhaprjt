using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Commands.CreateCostCentre;

namespace FinanceManagement.UnitTests.Application.CostCentre.Commands
{
    public sealed class CreateCostCentreCommandHandlerTests
    {
        private readonly Mock<ICostCentreCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateCostCentreCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateCostCentreCommand ValidCommand() =>
            new() { CostCentreCode = "STP", CostCentreName = "Plant", CentreLevelId = 59 };

        private void SetupHappyPath(int unitId = 1, int companyId = 1, int newId = 1)
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(unitId);
            _mockIp.Setup(s => s.GetCompanyId()).Returns(companyId);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.CostCentre>(It.IsAny<CreateCostCentreCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.CostCentre());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.CostCentre>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
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
        public async Task Handle_ValidCommand_SetsUnitAndCompanyFromSession()
        {
            FinanceManagement.Domain.Entities.CostCentre? captured = null;
            _mockIp.Setup(s => s.GetUnitId()).Returns(7);
            _mockIp.Setup(s => s.GetCompanyId()).Returns(9);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.CostCentre>(It.IsAny<CreateCostCentreCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.CostCentre());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.CostCentre>()))
                .Callback<FinanceManagement.Domain.Entities.CostCentre>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured!.UnitId.Should().Be(7);
            captured.CompanyId.Should().Be(9);
        }

        [Fact]
        public async Task Handle_NoActiveUnit_Throws()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_NoActiveCompany_Throws()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockIp.Setup(s => s.GetCompanyId()).Returns((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "COST_CENTRE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
