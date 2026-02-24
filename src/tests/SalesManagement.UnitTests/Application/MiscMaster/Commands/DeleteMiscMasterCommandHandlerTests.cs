#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.DeleteMiscMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class DeleteMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(MiscMasterBuilders.ValidDto(id: id));

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(1);

            var result = await CreateSut().Handle(new DeleteMiscMasterCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(new DeleteMiscMasterCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEventWithCorrectCodes()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(new DeleteMiscMasterCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "MISC_MASTER_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── Not Found (GetByIdAsync returns null) ─────────────────────────────

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.MiscMaster.Dto.MiscMasterDto)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(new DeleteMiscMasterCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                     .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallSoftDelete()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.MiscMaster.Dto.MiscMasterDto)null);

            var sut = CreateSut();
            try { await sut.Handle(new DeleteMiscMasterCommand(99), CancellationToken.None); } catch { /* expected */ }

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.MiscMaster.Dto.MiscMasterDto)null);

            var sut = CreateSut();
            try { await sut.Handle(new DeleteMiscMasterCommand(99), CancellationToken.None); } catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ── SoftDelete Returns False ───────────────────────────────────────────

        [Fact]
        public async Task Handle_SoftDeleteReturnsFalse_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(MiscMasterBuilders.ValidDto());

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(new DeleteMiscMasterCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
