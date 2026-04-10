using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IMiscMaster;
using LogisticsManagement.Application.MiscMaster.Dto;
using LogisticsManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using LogisticsManagement.Domain.Events;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = new List<MiscMasterLookupDto> { MiscMasterBuilders.ValidLookupDto() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<MiscMasterLookupDto>>(lookups))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_UsesEmptyString()
        {
            var empty = new List<MiscMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(empty);
            _mockMapper
                .Setup(m => m.Map<List<MiscMasterLookupDto>>(empty))
                .Returns(empty);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery(null!), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithMiscTypeCode_PassesMiscTypeCode()
        {
            var lookups = new List<MiscMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", "FREIGHT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<MiscMasterLookupDto>>(lookups))
                .Returns(lookups);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("test", "FREIGHT"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("test", "FREIGHT", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = new List<MiscMasterLookupDto> { MiscMasterBuilders.ValidLookupDto() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<MiscMasterLookupDto>>(lookups))
                .Returns(lookups);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetMiscMasterAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
