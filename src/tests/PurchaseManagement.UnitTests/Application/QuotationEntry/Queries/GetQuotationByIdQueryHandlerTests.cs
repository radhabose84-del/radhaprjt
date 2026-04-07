using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationById;

namespace PurchaseManagement.UnitTests.Application.QuotationEntry.Queries
{
    public sealed class GetQuotationByIdQueryHandlerTests
    {
        private readonly Mock<IQuotationQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetQuotationByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = new GetQuotationHeaderDto { Id = 1 };
            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<GetQuotationHeaderDto>(It.IsAny<object>()))
                .Returns(dto);

            var result = await CreateSut().Handle(
                new GetQuotationByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetQuotationHeaderDto?)null);

            Func<Task> act = async () =>
                await CreateSut().Handle(new GetQuotationByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
