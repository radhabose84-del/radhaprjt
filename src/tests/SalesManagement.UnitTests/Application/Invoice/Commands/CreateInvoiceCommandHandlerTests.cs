using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Invoice.Commands
{
    public sealed class CreateInvoiceCommandHandlerTests
    {
        private readonly Mock<IInvoiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<SalesManagement.Application.Common.Interfaces.IOutbox.IOutboxEventPublisher> _mockOutboxPublisher = new(MockBehavior.Loose);

        private CreateInvoiceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object,
                _mockDocSeqLookup.Object, _mockIpService.Object,
                _mockMediator.Object, _mockMapper.Object, _mockOutboxPublisher.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<InvoiceHeader>(It.IsAny<CreateInvoiceCommand>()))
                .Returns(new InvoiceHeader());

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSeqLookup
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "INV0001" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InvoiceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(newId);

            _mockCommandRepo
                .Setup(r => r.GetByIdInvoiceWorkFlowAsync(It.IsAny<int>()))
                .ReturnsAsync(new InvoiceWorkFlowDto());
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "INVOICE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoTransactionType_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<InvoiceHeader>(It.IsAny<CreateInvoiceCommand>()))
                .Returns(new InvoiceHeader());

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_SchedulesOutboxEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);

            _mockOutboxPublisher.Verify(p => p.ScheduleAsync(
                It.IsAny<Contracts.Commands.Workflow.CreateApprovalRequestCommand>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsInvoiceNoAndUnitIdOnEntity()
        {
            var entity = new InvoiceHeader();
            _mockMapper.Setup(m => m.Map<InvoiceHeader>(It.IsAny<CreateInvoiceCommand>()))
                .Returns(entity);

            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(5);

            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "INV/2026/00042" });

            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<InvoiceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(1);

            _mockCommandRepo.Setup(r => r.GetByIdInvoiceWorkFlowAsync(It.IsAny<int>()))
                .ReturnsAsync(new InvoiceWorkFlowDto());

            await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);

            entity.UnitId.Should().Be(5);
            entity.InvoiceNo.Should().Be("INV/2026/00042");
        }

        [Fact]
        public async Task Handle_NoDocumentSequence_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<InvoiceHeader>(It.IsAny<CreateInvoiceCommand>()))
                .Returns(new InvoiceHeader());

            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string>());  // empty → throws

            var act = async () => await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*No document sequence configured*");
        }

        [Fact]
        public async Task Handle_RepoThrows_PropagatesException_DoesNotScheduleOutbox()
        {
            _mockMapper.Setup(m => m.Map<InvoiceHeader>(It.IsAny<CreateInvoiceCommand>()))
                .Returns(new InvoiceHeader());

            _mockMiscRepo.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSeqLookup.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "INV0001" });

            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<InvoiceHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("DB write failed"));

            var act = async () => await CreateSut().Handle(new CreateInvoiceCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("DB write failed");

            _mockOutboxPublisher.Verify(p => p.ScheduleAsync(
                It.IsAny<Contracts.Commands.Workflow.CreateApprovalRequestCommand>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_PropagatesCancellationToken_ToOutboxPublisher()
        {
            SetupHappyPath();
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            await CreateSut().Handle(new CreateInvoiceCommand(), token);

            _mockOutboxPublisher.Verify(p => p.ScheduleAsync(
                It.IsAny<Contracts.Commands.Workflow.CreateApprovalRequestCommand>(),
                It.IsAny<Guid>(),
                token),
                Times.Once);
        }
    }
}
