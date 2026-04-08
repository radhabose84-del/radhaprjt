using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;
using LogisticsManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete;
using LogisticsManagement.Domain.Events;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Application.FreightMaster.Queries
{
    public sealed class GetFreightMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFreightMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = new List<FreightMasterLookupDto> { FreightMasterBuilders.ValidLookupDto() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterLookupDto>>(lookups))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetFreightMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_UsesEmptyString()
        {
            var empty = new List<FreightMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(empty);
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterLookupDto>>(empty))
                .Returns(empty);

            var result = await CreateSut().Handle(
                new GetFreightMasterAutoCompleteQuery(null!, null), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = new List<FreightMasterLookupDto> { FreightMasterBuilders.ValidLookupDto() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterLookupDto>>(lookups))
                .Returns(lookups);

            await CreateSut().Handle(
                new GetFreightMasterAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetFreightMasterAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithModuleId_PassesModuleId()
        {
            var lookups = new List<FreightMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterLookupDto>>(lookups))
                .Returns(lookups);

            var result = await CreateSut().Handle(
                new GetFreightMasterAutoCompleteQuery("test", 5), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("test", 5, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
