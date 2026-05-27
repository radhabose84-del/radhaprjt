using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DeliveryScoreRule.Commands
{
    public sealed class UpdateDeliveryScoreRuleCommandHandlerTests
    {
        private readonly Mock<IDeliveryScoreRuleCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IDeliveryScoreRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateDeliveryScoreRuleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper.Setup(m => m.Map<PurchaseManagement.Domain.Entities.VendorEvaluation.DeliveryScoreRule>(It.IsAny<object>())).Returns(DeliveryScoreRuleBuilders.ValidEntity());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.DeliveryScoreRule>())).ReturnsAsync(result);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(DeliveryScoreRuleBuilders.ValidUpdateCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsAffectedRows()
        {
            SetupHappyPath(result: 1);
            var result = await CreateSut().Handle(DeliveryScoreRuleBuilders.ValidUpdateCommand(), CancellationToken.None);
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(DeliveryScoreRuleBuilders.ValidUpdateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.VendorEvaluation.DeliveryScoreRule>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(DeliveryScoreRuleBuilders.ValidUpdateCommand(), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
