using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Queries
{
    public sealed class GetPurchaseIndentAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IPurchaseIndentQuery> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetPurchaseIndentAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            var headers = new List<IndentHeader>();
            _mockQueryRepo
                .Setup(r => r.GetPurchaseIndentAutoCompleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>()))
                .ReturnsAsync(headers);

            _mockMapper
                .Setup(m => m.Map<List<PurchaseIndentAutoCompleteQueryDto>>(It.IsAny<object>()))
                .Returns(new List<PurchaseIndentAutoCompleteQueryDto>());

            var result = await CreateSut().Handle(
                new GetPurchaseIndentAutoCompleteQuery { Status = "Approved" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeOfType<List<PurchaseIndentAutoCompleteQueryDto>>();
        }

        [Fact]
        public async Task Handle_CallsRepository_Once()
        {
            _mockQueryRepo
                .Setup(r => r.GetPurchaseIndentAutoCompleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<IndentHeader>());

            _mockMapper
                .Setup(m => m.Map<List<PurchaseIndentAutoCompleteQueryDto>>(It.IsAny<object>()))
                .Returns(new List<PurchaseIndentAutoCompleteQueryDto>());

            await CreateSut().Handle(
                new GetPurchaseIndentAutoCompleteQuery { Status = "Approved" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetPurchaseIndentAutoCompleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>()),
                Times.Once);
        }
    }
}
