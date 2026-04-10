using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueAutoComplete;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationValue.Queries
{
    public sealed class GetItemSpecificationValueAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IItemSpecificationValueQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemSpecificationValueAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidTerm_ReturnsLookupList()
        {
            var lookups = new List<ItemSpecificationValueLookupDto>
            {
                ItemSpecificationValueBuilders.ValidLookupDto(1, specificationMasterId: 1, value: "Red")
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Red", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueAutoCompleteQuery("Red", null), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsList()
        {
            var lookups = new List<ItemSpecificationValueLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueAutoCompleteQuery(string.Empty, null), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithSpecificationMasterIdFilter_ReturnsOnlyMatching()
        {
            var lookups = new List<ItemSpecificationValueLookupDto>
            {
                ItemSpecificationValueBuilders.ValidLookupDto(1, specificationMasterId: 1, value: "Red"),
                ItemSpecificationValueBuilders.ValidLookupDto(2, specificationMasterId: 2, value: "Small"),
                ItemSpecificationValueBuilders.ValidLookupDto(3, specificationMasterId: 1, value: "Blue")
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueAutoCompleteQuery(string.Empty, SpecificationMasterId: 1),
                CancellationToken.None);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.SpecificationMasterId == 1);
        }

        [Fact]
        public async Task Handle_WithSpecificationMasterIdZero_ReturnsAllResults()
        {
            var lookups = new List<ItemSpecificationValueLookupDto>
            {
                ItemSpecificationValueBuilders.ValidLookupDto(1, specificationMasterId: 1, value: "Red"),
                ItemSpecificationValueBuilders.ValidLookupDto(2, specificationMasterId: 2, value: "Small")
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueAutoCompleteQuery(string.Empty, SpecificationMasterId: 0),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_CallsAutocompleteOnce()
        {
            var lookups = new List<ItemSpecificationValueLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Blue", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);

            await CreateSut().Handle(
                new GetItemSpecificationValueAutoCompleteQuery("Blue", null), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("Blue", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
