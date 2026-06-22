using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre;

namespace FinanceManagement.UnitTests.Application.ProfitCentre.Commands
{
    public sealed class CreateProfitCentreCommandHandlerTests
    {
        private readonly Mock<IProfitCentreCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateProfitCentreCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateProfitCentreCommand ValidCommand() =>
            new() { ProfitCentreCode = "PCSPIN", ProfitCentreName = "Spinning", LevelId = 62 };

        private void SetupHappyPath(int companyId = 1, int newId = 1)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(companyId);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ProfitCentre>(It.IsAny<CreateProfitCentreCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ProfitCentre());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.ProfitCentre>()))
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
        public async Task Handle_ValidCommand_SetsCompanyFromSession()
        {
            FinanceManagement.Domain.Entities.ProfitCentre? captured = null;
            _mockIp.Setup(s => s.GetCompanyId()).Returns(9);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ProfitCentre>(It.IsAny<CreateProfitCentreCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ProfitCentre());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.ProfitCentre>()))
                .Callback<FinanceManagement.Domain.Entities.ProfitCentre>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured!.CompanyId.Should().Be(9);
        }

        [Fact]
        public async Task Handle_NoActiveCompany_Throws()
        {
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
                        e.ActionCode == "PROFIT_CENTRE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MidYearJustification_PublishesMidYearAuditNote()
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.MidYearJustification = "Added mid-year for the cotton line";

            await CreateSut().Handle(cmd, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PROFIT_CENTRE_MIDYEAR_ADD"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoMidYearJustification_DoesNotPublishMidYearNote()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PROFIT_CENTRE_MIDYEAR_ADD"),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
