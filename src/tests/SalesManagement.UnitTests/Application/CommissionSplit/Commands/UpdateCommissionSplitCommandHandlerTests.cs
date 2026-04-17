using AutoMapper;
using MediatR;
using SalesManagement.Domain.Events;
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.UpdateCommissionSplit;

namespace SalesManagement.UnitTests.Application.CommissionSplit.Commands
{
    public sealed class UpdateCommissionSplitCommandHandlerTests
    {
        private readonly Mock<ICommissionSplitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateCommissionSplitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int rowsAffected = 1, bool isLinked = false)
        {
            _mockQueryRepo.Setup(r => r.IsCommissionSplitLinkedAsync(It.IsAny<int>())).ReturnsAsync(isLinked);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.CommissionSplit>(It.IsAny<UpdateCommissionSplitCommand>()))
                .Returns(new SalesManagement.Domain.Entities.CommissionSplit());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.CommissionSplit>()))
                .ReturnsAsync(rowsAffected);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateCommissionSplitCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Inactivate_When_Linked_ThrowsExceptionRules()
        {
            SetupHappyPath(isLinked: true);

            Func<Task> act = async () => await CreateSut().Handle(
                new UpdateCommissionSplitCommand { Id = 1, IsActive = 0 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*linked with other records*");
        }

        [Fact]
        public async Task Handle_Inactivate_When_NotLinked_Succeeds()
        {
            SetupHappyPath(isLinked: false);
            var result = await CreateSut().Handle(
                new UpdateCommissionSplitCommand { Id = 1, IsActive = 0 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithDetails_PopulatesChildCollection()
        {
            SetupHappyPath();
            SalesManagement.Domain.Entities.CommissionSplit? captured = null;
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.CommissionSplit>()))
                .Callback<SalesManagement.Domain.Entities.CommissionSplit>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(new UpdateCommissionSplitCommand
            {
                Id = 1, IsActive = 1,
                Details = new()
                {
                    new() { RoleId = 1, ShareTypeId = 2, ShareValue = 100m }
                }
            }, CancellationToken.None);

            captured!.CommissionSplitDetails.Should().ContainSingle();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateCommissionSplitCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "COMMISSION_SPLIT_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
