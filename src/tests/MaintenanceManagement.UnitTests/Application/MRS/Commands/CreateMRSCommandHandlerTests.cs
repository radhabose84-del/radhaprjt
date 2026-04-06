using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Command.CreateMRS;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MRS.Commands
{
    public sealed class CreateMRSCommandHandlerTests
    {
        private readonly Mock<IMRSCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMRSQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMRSCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private static HeaderRequest ValidHeader() => new()
        {
            Divcode = "DIV001",
            IrDate = DateTime.Now.Date,
            Depcode = "DEP001",
            SubDepcode = "SUBDEP001",
            Refno = "REF001",
            MaintenanceType = "Corrective",
            Details = new List<DetailRequest>
            {
                new() { ItemCode = "ITEM001", CatCode = "CAT001", CcCode = "CC001", QtyReqd = 5m }
            }
        };

        private static CreateMRSCommand ValidCommand() => new() { Header = ValidHeader() };

        private void SetupHappyPath(int newId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.InsertMRSAsync(It.IsAny<HeaderRequest>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            SetupHappyPath(newId: 42);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsInsertMRSAsyncOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.InsertMRSAsync(It.IsAny<HeaderRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "MRS"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InsertReturnsZero_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.InsertMRSAsync(It.IsAny<HeaderRequest>()))
                .ReturnsAsync(0);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*MRS Creation Failed*");
        }

        [Fact]
        public async Task Handle_InsertReturnsZero_StillPublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.InsertMRSAsync(It.IsAny<HeaderRequest>()))
                .ReturnsAsync(0);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            try { await CreateSut().Handle(ValidCommand(), CancellationToken.None); } catch { }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
