using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetNextRawMaterialPONumber;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Queries
{
    public sealed class GetNextRawMaterialPONumberQueryHandlerTests
    {
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private GetNextRawMaterialPONumberQueryHandler CreateSut() =>
            new(_mockDocSeq.Object, _mockIp.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(string next = "RAW/2025/0003", string? last = "RAW/2025/0002")
        {
            _mockIp.Setup(i => i.GetUnitId()).Returns(37);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(8);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(8))
                .ReturnsAsync(new List<string> { next });
            _mockQueryRepo.Setup(r => r.GetLastPONumberAsync()).ReturnsAsync(last);
        }

        [Fact]
        public async Task Handle_ReturnsLastAndNext()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(new GetNextRawMaterialPONumberQuery(), CancellationToken.None);

            result.NextNumber.Should().Be("RAW/2025/0003");
            result.LastNumber.Should().Be("RAW/2025/0002");
        }

        [Fact]
        public async Task Handle_NoExistingDocs_LastIsNull()
        {
            SetupHappyPath(next: "RAW/2025/0001", last: null);

            var result = await CreateSut().Handle(new GetNextRawMaterialPONumberQuery(), CancellationToken.None);

            result.LastNumber.Should().BeNull();
            result.NextNumber.Should().Be("RAW/2025/0001");
        }

        [Fact]
        public async Task Handle_NoUnit_Throws()
        {
            _mockIp.Setup(i => i.GetUnitId()).Returns((int?)null);

            Func<Task> act = async () =>
                await CreateSut().Handle(new GetNextRawMaterialPONumberQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
