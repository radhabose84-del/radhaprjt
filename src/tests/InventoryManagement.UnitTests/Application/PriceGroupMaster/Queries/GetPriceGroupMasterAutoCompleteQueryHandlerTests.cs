using Contracts.Dtos.Lookups.Inventory;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterAutoComplete;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PriceGroupMaster.Queries
{
    public sealed class GetPriceGroupMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPriceGroupMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingLookups()
        {
            IReadOnlyList<PriceGroupMasterLookupDto> list = new List<PriceGroupMasterLookupDto>
            {
                new() { Id = 1, PriceGroupCode = "STD", PriceGroupName = "Standard" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("STD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetPriceGroupMasterAutoCompleteQuery("STD"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].PriceGroupCode.Should().Be("STD");
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepository()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PriceGroupMasterLookupDto>)new List<PriceGroupMasterLookupDto>());

            await CreateSut().Handle(new GetPriceGroupMasterAutoCompleteQuery(null!), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PriceGroupMasterLookupDto>)new List<PriceGroupMasterLookupDto>());

            await CreateSut().Handle(new GetPriceGroupMasterAutoCompleteQuery("x"), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PRICEGROUP_AUTOCOMPLETE"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
