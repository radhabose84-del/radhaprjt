using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Queries.GetNextOcrNumber;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Queries
{
    public sealed class GetNextOcrNumberQueryHandlerTests
    {
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOCREntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private GetNextOcrNumberQueryHandler CreateSut() =>
            new(_mockDocSeq.Object, _mockIp.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(string next = "OCR/2025/0004", string? last = "OCR/2025/0003")
        {
            _mockIp.Setup(i => i.GetUnitId()).Returns(37);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(5);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(5))
                .ReturnsAsync(new List<string> { next });
            _mockQueryRepo.Setup(r => r.GetLastOcrNumberAsync()).ReturnsAsync(last);
        }

        [Fact]
        public async Task Handle_ReturnsLastAndNext()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(new GetNextOcrNumberQuery(), CancellationToken.None);

            result.NextNumber.Should().Be("OCR/2025/0004");
            result.LastNumber.Should().Be("OCR/2025/0003");
        }

        [Fact]
        public async Task Handle_NoExistingDocs_LastIsNull()
        {
            SetupHappyPath(next: "OCR/2025/0001", last: null);

            var result = await CreateSut().Handle(new GetNextOcrNumberQuery(), CancellationToken.None);

            result.LastNumber.Should().BeNull();
            result.NextNumber.Should().Be("OCR/2025/0001");
        }

        [Fact]
        public async Task Handle_NoUnit_Throws()
        {
            _mockIp.Setup(i => i.GetUnitId()).Returns((int?)null);

            Func<Task> act = async () =>
                await CreateSut().Handle(new GetNextOcrNumberQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
