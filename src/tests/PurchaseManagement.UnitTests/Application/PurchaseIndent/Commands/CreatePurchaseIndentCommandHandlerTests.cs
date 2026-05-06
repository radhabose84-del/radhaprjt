using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Commands
{
    public sealed class CreatePurchaseIndentCommandHandlerTests
    {
        private readonly Mock<IPurchaseIndentCommand> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseIndentQuery> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseUnitOfWork> _mockUnitOfWork = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreatePurchaseIndentCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreatePurchaseIndentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockMiscRepo.Object, _mockQueryRepo.Object, _mockOutbox.Object,
                _mockUnitOfWork.Object, _mockLogger.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var header = new IndentHeader
            {
                Id = newId,
                IndentNumber = "IND001",
                IndentDetails = new List<IndentDetail>()
            };

            _mockMapper
                .Setup(m => m.Map<IndentHeader>(It.IsAny<object>()))
                .Returns(header);

            _mockMapper
                .Setup(m => m.Map<IndentReverseMapDto>(It.IsAny<object>()))
                .Returns(new IndentReverseMapDto());

            _mockQueryRepo
                .Setup(r => r.GeneratePurchaseIndentNumberAsync(It.IsAny<int>()))
                .ReturnsAsync("IND001");

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<IndentHeader>()))
                .ReturnsAsync(header);

            _mockUnitOfWork
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var command = new CreatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsDraft = 1,
                IndentDetails = new List<IndentDetailDto>()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = new CreatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsDraft = 1,
                IndentDetails = new List<IndentDetailDto>()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<IndentHeader>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = new CreatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsDraft = 1,
                IndentDetails = new List<IndentDetailDto>()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AuditPublish_RunsAfterCommit()
        {
            // Audit log must run AFTER CommitAsync (post-commit, outside the UoW try/catch).
            SetupHappyPath();

            int order = 0;
            int commitOrder = 0;
            int auditOrder = 0;

            _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Callback(() => commitOrder = ++order)
                .Returns(Task.CompletedTask);

            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Callback(() => auditOrder = ++order)
                .Returns(Task.CompletedTask);

            var command = new CreatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1, UnitId = 1, DepartmentId = 1, IsDraft = 1,
                IndentDetails = new List<IndentDetailDto>()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            commitOrder.Should().BeGreaterThan(0);
            auditOrder.Should().BeGreaterThan(commitOrder,
                "audit log must publish AFTER CommitAsync");
        }

        [Fact]
        public async Task Handle_AuditPublishThrows_DoesNotRollbackOrThrow()
        {
            // Audit failure (e.g., MongoDB unreachable) must NOT roll back the SQL
            // commit and must NOT bubble out as a 500 to the user.
            SetupHappyPath(newId: 7);

            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Mongo down"));

            var command = new CreatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1, UnitId = 1, DepartmentId = 1, IsDraft = 1,
                IndentDetails = new List<IndentDetailDto>()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(7, "the SQL commit should still be reflected in the response");
            _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never,
                "an audit failure must not trigger rollback");
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesIndentNumber()
        {
            SetupHappyPath();
            var command = new CreatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsDraft = 1,
                IndentDetails = new List<IndentDetailDto>()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GeneratePurchaseIndentNumberAsync(It.IsAny<int>()),
                Times.Once);
        }
    }
}
