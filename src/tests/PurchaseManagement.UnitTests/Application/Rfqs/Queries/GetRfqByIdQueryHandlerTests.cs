using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqById;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Application.Rfqs.Queries
{
    public sealed class GetRfqByIdQueryHandlerTests
    {
        private readonly Mock<IRfqQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);

        private PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockItemLookup.Object, _mockUomLookup.Object, _mockHsnLookup.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var aggregate = new RfqMaster
            {
                Id = 1,
                Items = new List<RfqItem>(),
                Suppliers = new List<RfqSupplier>()
            };

            _mockRepo
                .Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(aggregate);

            _mockMapper
                .Setup(m => m.Map<RfqDto>(It.IsAny<object>()))
                .Returns(new RfqDto(1, null, "RFQ001", 1, "Approved", 1, "Manual", null, DateOnly.FromDateTime(DateTime.Today), Array.Empty<RfqItemDto>(), Array.Empty<RfqSupplierDto>()));

            var result = await CreateSut().Handle(
                new GetRfqByIdQuery(1),
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsExceptionRules()
        {
            _mockRepo
                .Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RfqMaster?)null);

            Func<Task> act = async () =>
                await CreateSut().Handle(new GetRfqByIdQuery(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
