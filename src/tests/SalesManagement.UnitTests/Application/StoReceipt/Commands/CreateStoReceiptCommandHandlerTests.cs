using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StoReceipt.Commands
{
    public sealed class CreateStoReceiptCommandHandlerTests
    {
        private readonly Mock<IStoReceiptCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateStoReceiptCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMiscRepo.Object, _mockDocSeqLookup.Object,
                _mockIpService.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<StoReceiptHeader>(It.IsAny<CreateStoReceiptCommand>()))
                .Returns(new StoReceiptHeader { StoReceiptDetails = new List<StoReceiptDetail>() });

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSeqLookup
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "SR0001" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<StoReceiptHeader>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateStoReceiptCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(new CreateStoReceiptCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateStoReceiptCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "STORECEIPT_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoTransactionType_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<StoReceiptHeader>(It.IsAny<CreateStoReceiptCommand>()))
                .Returns(new StoReceiptHeader());

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new CreateStoReceiptCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
