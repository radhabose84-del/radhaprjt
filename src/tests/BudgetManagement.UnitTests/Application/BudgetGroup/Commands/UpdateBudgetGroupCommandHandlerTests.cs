using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Command.UpdateBudgetGroup;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using MediatR;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Application.BudgetGroup.Commands
{
    public sealed class UpdateBudgetGroupCommandHandlerTests
    {
        private readonly Mock<IBudgetGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IBudgetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterQueryRepo = new(MockBehavior.Strict);

        private UpdateBudgetGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockMiscMasterQueryRepo.Object);

        private void SetupMiscMasterLookups()
        {
            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypePercentage))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 100 });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeSpindle))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 101 });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeRequest))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 102 });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeAnnual))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 10 });

            _mockMiscMasterQueryRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeMonthly))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 11 });
        }

        private void SetupHappyPath(int resultId = 1)
        {
            SetupMiscMasterLookups();

            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BudgetGroupBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<BudgetManagement.Domain.Entities.BudgetGroup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            SetupHappyPath(1);
            var sut = CreateSut();

            var result = await sut.Handle(BudgetGroupBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsException()
        {
            SetupMiscMasterLookups();

            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetGroup?)null);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(BudgetGroupBuilders.ValidUpdateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsException()
        {
            SetupMiscMasterLookups();

            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BudgetGroupBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(BudgetGroupBuilders.ValidUpdateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var sut = CreateSut();

            await sut.Handle(BudgetGroupBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "UpdateBudgetGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
