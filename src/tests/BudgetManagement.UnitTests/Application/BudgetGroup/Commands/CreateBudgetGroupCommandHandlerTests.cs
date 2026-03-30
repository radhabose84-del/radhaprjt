using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Command.CreateBudgetGroup;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetGroup.Commands
{
    public sealed class CreateBudgetGroupCommandHandlerTests
    {
        private readonly Mock<IBudgetGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterQueryRepo = new(MockBehavior.Strict);

        private CreateBudgetGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockMiscMasterQueryRepo.Object);

        private void SetupMiscMasterLookups()
        {
            // Percentage allocation rule
            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypePercentage))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 100, Code = "Percentage" });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeSpindle))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 101, Code = "Spindle" });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeRequest))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 102, Code = "Request" });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeAnnual))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 10, Code = "Annual" });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeMonthly))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 11, Code = "Monthly" });
        }

        private void SetupHappyPath(int newId = 1)
        {
            SetupMiscMasterLookups();

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAndUnitDepartmentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.BudgetGroup>(It.IsAny<CreateBudgetGroupCommand>()))
                .Returns(BudgetGroupBuilders.ValidEntity(0));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetGroup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var sut = CreateSut();

            var result = await sut.Handle(BudgetGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsException()
        {
            SetupMiscMasterLookups();

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAndUnitDepartmentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(BudgetGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(BudgetGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "CreateBudgetGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            SetupHappyPath(0);
            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(BudgetGroupBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*creation failed*");
        }

        [Fact]
        public async Task Handle_InvalidBudgetType_ThrowsException()
        {
            SetupMiscMasterLookups();

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAndUnitDepartmentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.BudgetGroup>(It.IsAny<CreateBudgetGroupCommand>()))
                .Returns(BudgetGroupBuilders.ValidEntity(0));

            var sut = CreateSut();
            var command = BudgetGroupBuilders.ValidCreateCommand(budgetTypeId: 999);

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Budget Type*");
        }
    }
}
