using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Common;
using MediatR;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Application.BudgetGroup.Commands
{
    public sealed class DeleteBudgetGroupCommandHandlerTests
    {
        private readonly Mock<IBudgetGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IBudgetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteBudgetGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsId()
        {
            var entity = BudgetGroupBuilders.ValidEntity();

            _mockCommandRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(BudgetGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsException()
        {
            _mockCommandRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetGroup?)null);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(new DeleteBudgetGroupCommand { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_HasChildren_ThrowsException()
        {
            var entity = BudgetGroupBuilders.ValidEntity();
            _mockCommandRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(BudgetGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Parent Budget Group*");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = BudgetGroupBuilders.ValidEntity();
            _mockCommandRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(BudgetGroupBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "DeleteBudgetGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
