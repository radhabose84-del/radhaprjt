using AutoMapper;
using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Interfaces;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetAllocation.Commands
{
    public sealed class CreateBudgetAllocationCommandHandlerTests
    {
        private readonly Mock<IBudgetAllocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateBudgetAllocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockIp.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.BudgetAllocation>(It.IsAny<CreateBudgetAllocationDto>()))
                .Returns(BudgetAllocationBuilders.ValidEntity(newId));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetAllocation>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsLastId()
        {
            SetupHappyPath(5);
            var command = BudgetAllocationBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateForEachDto()
        {
            SetupHappyPath();
            var dtos = new List<CreateBudgetAllocationDto>
            {
                BudgetAllocationBuilders.ValidCreateDto(),
                BudgetAllocationBuilders.ValidCreateDto(budgetGroupId: 2)
            };
            var command = BudgetAllocationBuilders.ValidCreateCommand(dtos);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetAllocation>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEventForEachDto()
        {
            SetupHappyPath();
            var dtos = new List<CreateBudgetAllocationDto>
            {
                BudgetAllocationBuilders.ValidCreateDto(),
                BudgetAllocationBuilders.ValidCreateDto(budgetGroupId: 2)
            };
            var command = BudgetAllocationBuilders.ValidCreateCommand(dtos);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_EmptyList_ThrowsExceptionRules()
        {
            var command = new CreateBudgetAllocationCommand { createBudgetAllocations = new List<CreateBudgetAllocationDto>() };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*No Budget Allocations provided*");
        }

        [Fact]
        public async Task Handle_NullList_ThrowsExceptionRules()
        {
            var command = new CreateBudgetAllocationCommand { createBudgetAllocations = null! };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
