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
