using AutoMapper;
using MediatR;
using SalesManagement.Domain.Events;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit;

namespace SalesManagement.UnitTests.Application.CommissionSplit.Commands
{
    public sealed class CreateCommissionSplitCommandHandlerTests
    {
        private readonly Mock<ICommissionSplitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateCommissionSplitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.CommissionSplit>(It.IsAny<CreateCommissionSplitCommand>()))
                .Returns(new SalesManagement.Domain.Entities.CommissionSplit { SplitCode = "CS001" });
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.CommissionSplit>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateCommissionSplitCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var result = await CreateSut().Handle(new CreateCommissionSplitCommand(), CancellationToken.None);
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_WithDetails_PopulatesChildCollection()
        {
            SetupHappyPath();
            SalesManagement.Domain.Entities.CommissionSplit? captured = null;
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.CommissionSplit>()))
                .Callback<SalesManagement.Domain.Entities.CommissionSplit>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(new CreateCommissionSplitCommand
            {
                SplitName = "Split A",
                Details = new()
                {
                    new() { RoleId = 1, ShareTypeId = 2, ShareValue = 25m },
                    new() { RoleId = 3, ShareTypeId = 2, ShareValue = 75m }
                }
            }, CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.CommissionSplitDetails.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NullDetails_DoesNotPopulateChildren()
        {
            SetupHappyPath();
            SalesManagement.Domain.Entities.CommissionSplit? captured = null;
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.CommissionSplit>()))
                .Callback<SalesManagement.Domain.Entities.CommissionSplit>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(new CreateCommissionSplitCommand { SplitName = "X", Details = null }, CancellationToken.None);

            captured!.CommissionSplitDetails.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WithCorrectActionCode()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateCommissionSplitCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "COMMISSION_SPLIT_CREATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
