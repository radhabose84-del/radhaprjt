using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetById;

namespace PurchaseManagement.UnitTests.Application.PriceMaster.Queries
{
    public sealed class GetPriceMasterByIdQueryHandlerTests
    {
        private readonly Mock<IPriceMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);

        private GetPriceMasterByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockItemLookup.Object, _mockUomLookup.Object,
                _mockPartyLookup.Object, _mockCurrencyLookup.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PriceMasterGetAllDto
                {
                    Id = id,
                    ItemId = 1,
                    ItemCode = "ITEM001",
                    VendorId = 1,
                    UomId = 1,
                    Details = new List<PriceMasterDetailUpsertDto>()
                });

            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>());

            _mockUomLookup
                .Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());

            _mockPartyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>());

            _mockCurrencyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            SetupHappyPath(1);

            var result = await CreateSut().Handle(
                new GetPriceMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsQueryRepoOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                new GetPriceMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsAllLookups()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                new GetPriceMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockItemLookup.Verify(
                l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUomLookup.Verify(
                l => l.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PriceMasterGetAllDto?)null);

            var result = await CreateSut().Handle(
                new GetPriceMasterByIdQuery { Id = 99 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotCallLookups()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PriceMasterGetAllDto?)null);

            await CreateSut().Handle(
                new GetPriceMasterByIdQuery { Id = 99 },
                CancellationToken.None);

            _mockItemLookup.Verify(
                l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
