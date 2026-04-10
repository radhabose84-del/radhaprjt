using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterAutoComplete;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationMaster.Queries
{
    public sealed class GetItemSpecificationMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IItemSpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemSpecificationMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidTerm_ReturnsLookupList()
        {
            var lookups = new List<ItemSpecificationMasterLookupDto>
            {
                ItemSpecificationMasterBuilders.ValidLookupDto(1)
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Color", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationMasterLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemSpecificationMasterAutoCompleteQuery("Color"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsList()
        {
            var lookups = new List<ItemSpecificationMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationMasterLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetItemSpecificationMasterAutoCompleteQuery(string.Empty), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsAutocompleteOnce()
        {
            var lookups = new List<ItemSpecificationMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Size", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationMasterLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);

            await CreateSut().Handle(
                new GetItemSpecificationMasterAutoCompleteQuery("Size"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("Size", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
