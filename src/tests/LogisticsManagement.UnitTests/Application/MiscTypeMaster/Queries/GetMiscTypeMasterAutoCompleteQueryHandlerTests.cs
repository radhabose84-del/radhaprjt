using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Dto;
using LogisticsManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using LogisticsManagement.Domain.Events;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = new List<MiscTypeMasterLookupDto> { MiscTypeMasterBuilders.ValidLookupDto() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<MiscTypeMasterLookupDto>>(lookups))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_UsesEmptyString()
        {
            var empty = new List<MiscTypeMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>()))
                .ReturnsAsync(empty);
            _mockMapper
                .Setup(m => m.Map<List<MiscTypeMasterLookupDto>>(empty))
                .Returns(empty);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery(null!), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = new List<MiscTypeMasterLookupDto> { MiscTypeMasterBuilders.ValidLookupDto() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<MiscTypeMasterLookupDto>>(lookups))
                .Returns(lookups);

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetMiscTypeMasterAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var lookups = new List<MiscTypeMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<MiscTypeMasterLookupDto>>(lookups))
                .Returns(lookups);

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("test"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
