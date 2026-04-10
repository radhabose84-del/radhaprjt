using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Commands.CreateStoHeader;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StoHeader.Commands
{
    public sealed class CreateStoHeaderCommandHandlerTests
    {
        private readonly Mock<IStoHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateStoHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockDocSeqLookup.Object,
                _mockIpService.Object, _mockOutbox.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.StoHeader>(It.IsAny<CreateStoHeaderCommand>()))
                .Returns(new SalesManagement.Domain.Entities.StoHeader());

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(10);

            _mockDocSeqLookup
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "STO0001" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.StoHeader>(), It.IsAny<int>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateStoHeaderCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(new CreateStoHeaderCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateStoHeaderCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "STO_HEADER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoTransactionType_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.StoHeader>(It.IsAny<CreateStoHeaderCommand>()))
                .Returns(new SalesManagement.Domain.Entities.StoHeader());

            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockDocSeqLookup
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new CreateStoHeaderCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
