using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Arrival.Commands.CreateArrival;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Entities.Arrival;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.Arrival.Commands
{
    public sealed class CreateArrivalCommandHandlerTests
    {
        private readonly Mock<IArrivalCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateArrivalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockDocSeq.Object, _mockIp.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "ARV-2025-0006" });
            _mockMapper
                .Setup(m => m.Map<ArrivalHeader>(It.IsAny<object>()))
                .Returns(ArrivalBuilders.ValidEntity(newId));
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<ArrivalHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ArrivalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(ArrivalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ArrivalBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<ArrivalHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesArrivalNumber()
        {
            SetupHappyPath();
            await CreateSut().Handle(ArrivalBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockDocSeq.Verify(d => d.GenerateDocumentNumber(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ArrivalBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsPayloadWeightsVerbatim()
        {
            ArrivalHeader? captured = null;
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "ARV-2025-0006" });
            _mockMapper.Setup(m => m.Map<ArrivalHeader>(It.IsAny<object>())).Returns(ArrivalBuilders.ValidEntity());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<ArrivalHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<ArrivalHeader, int, CancellationToken>((e, _, __) => captured = e)
                .ReturnsAsync(1);

            // NetWeight / WeightDifference come from the payload (mapped), not computed by the handler.
            await CreateSut().Handle(ArrivalBuilders.ValidCreateCommand(), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.NetWeight.Should().Be(20000m);
            captured.WeightDifference.Should().Be(-100m);
        }
    }
}
