using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Application.Port.Queries.GetAllPorts;
using PurchaseManagement.Application.Purchase.PortMaster.Handlers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PortMaster.Queries
{
    public sealed class GetAllPortsQueryHandlerTests
    {
        private readonly Mock<IPortMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _mockCountryLookup = new(MockBehavior.Loose);

        private GetAllPortsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockCountryLookup.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyItems()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<PortMasterDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<PortMasterDto>>(It.IsAny<IReadOnlyList<PortMasterDto>>()))
                .Returns(new List<PortMasterDto>());

            var result = await CreateSut().Handle(
                new GetAllPortsQuery(1, 10),
                CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.Total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsCorrectCount()
        {
            var dtoList = new List<PortMasterDto>
            {
                PortMasterBuilders.ValidDto(1),
                PortMasterBuilders.ValidDto(2)
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((dtoList, 2));

            _mockMapper
                .Setup(m => m.Map<List<PortMasterDto>>(It.IsAny<IReadOnlyList<PortMasterDto>>()))
                .Returns(dtoList);

            _mockCountryLookup
                .Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryLookupDto>());

            var result = await CreateSut().Handle(
                new GetAllPortsQuery(1, 10),
                CancellationToken.None);

            result.Total.Should().Be(2);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsyncOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, "PORT", null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<PortMasterDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<PortMasterDto>>(It.IsAny<IReadOnlyList<PortMasterDto>>()))
                .Returns(new List<PortMasterDto>());

            await CreateSut().Handle(
                new GetAllPortsQuery(1, 10, "PORT"),
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllAsync(1, 10, "PORT", null, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
