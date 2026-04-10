using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Commands
{
    public sealed class UpdatePurchaseIndentCommandHandlerTests
    {
        private readonly Mock<IPurchaseIndentCommand> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseIndentQuery> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseUnitOfWork> _mockUnitOfWork = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdatePurchaseIndentCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdatePurchaseIndentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockQueryRepo.Object, _mockMiscRepo.Object, _mockOutbox.Object,
                _mockUnitOfWork.Object, _mockLogger.Object);

        private void SetupHappyPath()
        {
            var header = new IndentHeader
            {
                Id = 1,
                IndentNumber = "IND001",
                IndentDetails = new List<IndentDetail>(),
                StatusId = 1
            };

            _mockMapper
                .Setup(m => m.Map<IndentHeader>(It.IsAny<object>()))
                .Returns(header);

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(header);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<IndentHeader>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockUnitOfWork
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var command = new UpdatePurchaseIndentCommand
            {
                Id = 1,
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsDraft = 1,
                IndentDetails = new List<IndentDetailUpdateDto>()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            var command = new UpdatePurchaseIndentCommand
            {
                Id = 1,
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsDraft = 1,
                IndentDetails = new List<IndentDetailUpdateDto>()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<IndentHeader>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = new UpdatePurchaseIndentCommand
            {
                Id = 1,
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                IsDraft = 1,
                IndentDetails = new List<IndentDetailUpdateDto>()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
