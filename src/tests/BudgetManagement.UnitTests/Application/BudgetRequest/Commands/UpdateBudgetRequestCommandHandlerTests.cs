using AutoMapper;
using BudgetManagement.Application.BudgetRequest.Commands.Update;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Commands
{
    public sealed class UpdateBudgetRequestCommandHandlerTests
    {
        private readonly Mock<IBudgetRequestCommandRepository> _mockBudgetRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IBudgetRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateBudgetRequestCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);

        private UpdateBudgetRequestCommandHandler CreateSut() =>
            new(_mockBudgetRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQueryRepo.Object, _mockLogger.Object, _mockIp.Object,
                _mockUnitLookup.Object, _mockCompanyLookup.Object, _mockFyLookup.Object);

        private void SetupHappyPath()
        {
            var entity = BudgetRequestBuilders.ValidEntity();
            entity.RequestMonthId = 1;
            entity.FinancialYearId = 1;

            _mockBudgetRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map(It.IsAny<UpdateBudgetRequestCommand>(), entity));

            _mockBudgetRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync()
        {
            SetupHappyPath();
            var command = BudgetRequestBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockBudgetRepo.Verify(
                r => r.UpdateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = BudgetRequestBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallUpdate()
        {
            _mockBudgetRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetRequest?)null);

            var command = BudgetRequestBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockBudgetRepo.Verify(
                r => r.UpdateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EditFlag1_IncrementsRevisionNumber()
        {
            var entity = BudgetRequestBuilders.ValidEntity();
            entity.RevisionNumber = 2;
            entity.RequestAmount = 10000m;
            entity.RequestMonthId = 1;
            entity.FinancialYearId = 1;

            _mockBudgetRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockBudgetRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BudgetManagement.Domain.Entities.BudgetRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = BudgetRequestBuilders.ValidUpdateCommand(editFlag: 1);

            await CreateSut().Handle(command, CancellationToken.None);

            entity.RevisionNumber.Should().Be(3);
        }
    }
}
