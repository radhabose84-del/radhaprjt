#nullable disable
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new DeleteMiscTypeMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object);

        private void SetupSoftDelete(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
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
            var result = await CreateSut().Handle(new DeleteMiscTypeMasterCommand(1), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteAsync_Once()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();
            await CreateSut().Handle(new DeleteMiscTypeMasterCommand(1), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();
            await CreateSut().Handle(new DeleteMiscTypeMasterCommand(1), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "MISC_TYPE_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_AuditEvent_ContainsRequestId()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();
            await CreateSut().Handle(new DeleteMiscTypeMasterCommand(1), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
