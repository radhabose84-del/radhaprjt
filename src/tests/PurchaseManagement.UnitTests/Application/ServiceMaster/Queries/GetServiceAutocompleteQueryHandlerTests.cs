using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.ServiceMaster.Queries
{
    public sealed class GetServiceAutocompleteQueryHandlerTests
    {
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);

        private GetServiceAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUomLookup.Object, _mockHsnLookup.Object);

        private void SetupHappyPath(string term = "SRV")
        {
            var autocompleteItems = new List<ServiceMasterAutoCompleteDto>
            {
                new ServiceMasterAutoCompleteDto
                {
                    Id = 1, ServiceCode = "SRV001", ServiceDescription = "Test Service",
                    SacId = 1, UomId = 1
                }
            };

            _mockQueryRepo
                .Setup(r => r.ServiceMasterAuotoComplete(term))
                .ReturnsAsync(autocompleteItems);

            _mockMapper
                .Setup(m => m.Map<List<ServiceMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns<object>(src => src as List<ServiceMasterAutoCompleteDto> ?? new List<ServiceMasterAutoCompleteDto>());

            _mockUomLookup
                .Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());

            _mockHsnLookup
                .Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HSNLookupDto>());
        }

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            SetupHappyPath("SRV");

            var result = await CreateSut().Handle(
                new GetServiceAutocompleteQuery { SearchPattern = "SRV" },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            SetupHappyPath("SRV");

            await CreateSut().Handle(
                new GetServiceAutocompleteQuery { SearchPattern = "SRV" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.ServiceMasterAuotoComplete("SRV"), Times.Once);
        }
    }
}
