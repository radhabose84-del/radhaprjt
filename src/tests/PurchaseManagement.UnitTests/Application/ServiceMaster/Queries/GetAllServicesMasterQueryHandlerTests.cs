using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.UnitTests.TestData;
using MediatR;

namespace PurchaseManagement.UnitTests.Application.ServiceMaster.Queries
{
    public sealed class GetAllServicesMasterQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);

        private GetAllServicesMasterQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object, _mockUomLookup.Object, _mockHsnLookup.Object);

        private void SetupLookups()
        {
            _mockMapper
                .Setup(m => m.Map<List<GetServiceMasterDto>>(It.IsAny<object>()))
                .Returns<object>(src => src as List<GetServiceMasterDto> ?? new List<GetServiceMasterDto>());

            _mockUomLookup
                .Setup(u => u.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>
                {
                    new UOMLookupDto { Id = 1, Code = "NOS", UOMName = "Nos" }
                });

            _mockHsnLookup
                .Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HSNLookupDto>
                {
                    new HSNLookupDto { Id = 1, HSNCode = "SAC001", Description = "Test SAC" }
                });
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllServiceMasterAsync(1, 15, null))
                .ReturnsAsync((new List<GetServiceMasterDto>(), 0));

            SetupLookups();

            var result = await CreateSut().Handle(
                new GetAllServicesMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsCorrectCount()
        {
            var dtos = new List<GetServiceMasterDto>
            {
                ServiceMasterBuilders.ValidDto(1),
                ServiceMasterBuilders.ValidDto(2, "SRV002")
            };

            _mockQueryRepo
                .Setup(r => r.GetAllServiceMasterAsync(1, 15, null))
                .ReturnsAsync((dtos, 2));

            SetupLookups();

            var result = await CreateSut().Handle(
                new GetAllServicesMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data!.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllServiceMasterAsync(2, 10, "search"))
                .ReturnsAsync((new List<GetServiceMasterDto>(), 11));

            SetupLookups();

            var result = await CreateSut().Handle(
                new GetAllServicesMasterQuery { PageNumber = 2, PageSize = 10, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(11);
        }
    }
}
