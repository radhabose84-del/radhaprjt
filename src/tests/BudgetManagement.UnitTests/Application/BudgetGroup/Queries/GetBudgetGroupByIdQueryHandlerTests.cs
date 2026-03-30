using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupById;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetGroup.Queries
{
    public sealed class GetBudgetGroupByIdQueryHandlerTests
    {
        private readonly Mock<IBudgetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCCLookup = new(MockBehavior.Loose);

        private GetBudgetGroupByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockDeptLookup.Object,
                _mockUnitLookup.Object, _mockCCLookup.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsSuccess()
        {
            var dto = BudgetGroupBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetBudgetGroupByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetGroupDto?)null);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(
                new GetBudgetGroupByIdQuery { Id = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = BudgetGroupBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetBudgetGroupByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<BudgetManagement.Domain.Events.AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
