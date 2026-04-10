using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetById;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.BillEntry.Queries
{
    public sealed class GetPurchaseBillEntryByIdQueryHandlerTests
    {
        private readonly Mock<IPurchaseBillEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetPurchaseBillEntryByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseBillEntryHeader?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetPurchaseBillEntryByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_Found_ReturnsDto()
        {
            var entity = new PurchaseBillEntryHeader { Id = 1 };
            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<PurchaseBillEntryHeaderDto>(It.IsAny<object>()))
                .Returns(new PurchaseBillEntryHeaderDto { Id = 1 });

            var result = await CreateSut().Handle(
                new GetPurchaseBillEntryByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }
    }
}
