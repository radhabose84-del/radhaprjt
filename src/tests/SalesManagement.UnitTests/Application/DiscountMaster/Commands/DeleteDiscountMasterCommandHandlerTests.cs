using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.DeleteDiscountMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DiscountMaster.Commands
{
    public class DeleteDiscountMasterCommandHandlerTests
    {
        private readonly Mock<IDiscountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteDiscountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupSoftDelete(int id = 1, bool result = true)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(new DeleteDiscountMasterCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteAsync_Once()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();

            await CreateSut().Handle(new DeleteDiscountMasterCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();

            await CreateSut().Handle(new DeleteDiscountMasterCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISCOUNT_MASTER_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            SetupSoftDelete(99, false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new DeleteDiscountMasterCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            SetupSoftDelete(99, false);

            var sut = CreateSut();
            try { await sut.Handle(new DeleteDiscountMasterCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
