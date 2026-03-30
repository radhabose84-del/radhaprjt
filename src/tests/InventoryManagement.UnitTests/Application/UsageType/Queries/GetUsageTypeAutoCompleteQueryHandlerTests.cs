using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Dto;
using InventoryManagement.Application.UsageType.Queries.GetUsageTypeAutoComplete;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UsageType.Queries
{
    public sealed class GetUsageTypeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IUsageTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUsageTypeAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsItems()
        {
            var lookups = new List<UsageTypeLookupDto> { UsageTypeBuilders.ValidLookupDto() }.AsReadOnly();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<UsageTypeLookupDto>>(It.IsAny<object>()))
                .Returns(new List<UsageTypeLookupDto> { UsageTypeBuilders.ValidLookupDto() });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUsageTypeAutoCompleteQuery("UTY"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UsageTypeLookupDto>().AsReadOnly());
            _mockMapper
                .Setup(m => m.Map<List<UsageTypeLookupDto>>(It.IsAny<object>()))
                .Returns(new List<UsageTypeLookupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUsageTypeAutoCompleteQuery("ZZZ"), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
