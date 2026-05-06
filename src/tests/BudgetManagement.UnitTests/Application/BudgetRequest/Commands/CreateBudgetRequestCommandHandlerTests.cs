using AutoMapper;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.Common.Interfaces.IOutbox;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Commands
{
    public sealed class CreateBudgetRequestCommandHandlerTests
    {
        private readonly Mock<IBudgetRequestCommandRepository> _mockBudgetRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IBudgetRequestQueryRepository> _mockBudgetQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateBudgetRequestCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IBudgetUnitOfWork> _mockUow = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);

        private CreateBudgetRequestCommandHandler CreateSut() =>
            new(_mockBudgetRepo.Object, _mockMiscRepo.Object, _mockMapper.Object,
                _mockMediator.Object, _mockBudgetQueryRepo.Object, _mockLogger.Object,
                _mockUnitLookup.Object, _mockCompanyLookup.Object, _mockIp.Object,
                _mockOutbox.Object, _mockUow.Object, _mockFyLookup.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = BudgetRequestBuilders.ValidEntity(newId);
            entity.RequestCode = "REQ-2025-001";

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.BudgetRequest>(It.IsAny<CreateBudgetRequestCommand>()))
                .Returns(entity);

            _mockBudgetRepo
                .Setup(r => r.GenerateCodeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("REQ-2025-001");

            _mockBudgetRepo
                .Setup(r => r.AddAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockBudgetRepo
                .Setup(r => r.GetByIdBudgetRequestWorkFlowAsync(It.IsAny<int>()))
                .ReturnsAsync(new BudgetRequestWorkFlowDto());

            _mockMapper
                .Setup(m => m.Map<CreateBudgetRequestReverseDto>(It.IsAny<BudgetRequestWorkFlowDto>()))
                .Returns(new CreateBudgetRequestReverseDto());

            _mockOutbox
                .Setup(o => o.ScheduleWithoutSaveAsync(It.IsAny<object>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsEntityId()
        {
            SetupHappyPath(42);
            var command = BudgetRequestBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsync()
        {
            SetupHappyPath();
            var command = BudgetRequestBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockBudgetRepo.Verify(
                r => r.AddAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_BeginsTransaction()
        {
            SetupHappyPath();
            var command = BudgetRequestBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockUow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CommitsTransaction()
        {
            SetupHappyPath();
            var command = BudgetRequestBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FileOperations_RunAfterCommit()
        {
            // Asserts that file system operations (TryMoveImageAsync) happen
            // post-commit. The first DB call inside TryMoveImageAsync is
            // _budgetQueryRepo.GetBaseDirectoryAsync — if it runs before
            // CommitAsync then file ops are inside the UoW try block (the bug).
            SetupHappyPath();

            int order = 0;
            int commitOrder = 0;
            int baseDirOrder = 0;

            _mockUow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Callback(() => commitOrder = ++order)
                .Returns(Task.CompletedTask);

            _mockBudgetQueryRepo.Setup(r => r.GetBaseDirectoryAsync(It.IsAny<CancellationToken>()))
                .Callback(() => baseDirOrder = ++order)
                .ReturnsAsync("BudgetRequest");

            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>());

            var command = BudgetRequestBuilders.ValidCreateCommand();
            command.ImagePath = "any-temp-filename.jpg";

            await CreateSut().Handle(command, CancellationToken.None);

            commitOrder.Should().BeGreaterThan(0, "CommitAsync should have been called");
            baseDirOrder.Should().BeGreaterThan(commitOrder,
                "file system operations must run AFTER CommitAsync, not inside the transaction");
        }

        [Fact]
        public async Task Handle_AuditPublish_RunsAfterCommit()
        {
            // Audit log must run AFTER CommitAsync — capture call order to prove it.
            SetupHappyPath();

            int order = 0;
            int commitOrder = 0;
            int auditOrder = 0;

            _mockUow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Callback(() => commitOrder = ++order)
                .Returns(Task.CompletedTask);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Callback(() => auditOrder = ++order)
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(BudgetRequestBuilders.ValidCreateCommand(), CancellationToken.None);

            commitOrder.Should().BeGreaterThan(0);
            auditOrder.Should().BeGreaterThan(commitOrder,
                "audit log must publish AFTER CommitAsync");
        }

        [Fact]
        public async Task Handle_AuditPublishThrows_DoesNotRollbackOrThrow()
        {
            // Audit failure (e.g., MongoDB unreachable) must NOT roll back the SQL
            // commit and must NOT bubble out as a 500 to the user.
            SetupHappyPath();

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Mongo down"));

            var result = await CreateSut().Handle(BudgetRequestBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().BeGreaterThan(0, "the SQL commit should still be reflected in the response");
            _mockUow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never,
                "an audit failure must not trigger rollback");
        }

        [Fact]
        public async Task Handle_NoMatchingFinancialYear_ThrowsApplicationException()
        {
            var entity = BudgetRequestBuilders.ValidEntity();
            entity.FinancialYearId = 0;

            _mockMapper
                .Setup(m => m.Map<BudgetManagement.Domain.Entities.BudgetRequest>(It.IsAny<CreateBudgetRequestCommand>()))
                .Returns(entity);

            _mockFyLookup
                .Setup(l => l.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto>());

            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var command = BudgetRequestBuilders.ValidCreateCommand(financialYearId: 0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ApplicationException>();
        }
    }
}
