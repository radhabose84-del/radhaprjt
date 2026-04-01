using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.CreateRepacking;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Application.Repacking.Commands
{
    public sealed class CreateRepackingCommandHandlerTests
    {
        private readonly Mock<IRepackingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateRepackingCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockDocSeq.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<RepackingHeader>(It.IsAny<CreateRepackingCommand>()))
                .Returns(new RepackingHeader());
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(6);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(6))
                .ReturnsAsync(new List<string> { "REPACK-001" });
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<RepackingHeader>(), 6))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(new CreateRepackingCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(99);
            var result = await CreateSut().Handle(new CreateRepackingCommand(), CancellationToken.None);
            result.Data.Should().Be(99);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateRepackingCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<RepackingHeader>(), 6), Times.Once);
        }
    }
}
