using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Application.Port.Queries.GetById;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PortMaster.Queries
{
    public sealed class GetPortByIdQueryHandlerTests
    {
        private readonly Mock<IPortMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _mockCountryLookup = new(MockBehavior.Loose);

        private GetPortByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCountryLookup.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = PortMasterBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockCountryLookup
                .Setup(c => c.GetAllCountriesAsync(default))
                .ReturnsAsync(new List<CountryLookupDto>());

            var result = await CreateSut().Handle(new GetPortByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.PortCode.Should().Be("PORT001");
        }

        [Fact]
        public async Task Handle_NotFoundId_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PortMasterDto?)null);

            var result = await CreateSut().Handle(new GetPortByIdQuery(999), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsGetByIdAsyncOnce()
        {
            var dto = PortMasterBuilders.ValidDto();
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockCountryLookup
                .Setup(c => c.GetAllCountriesAsync(default))
                .ReturnsAsync(new List<CountryLookupDto>());

            await CreateSut().Handle(new GetPortByIdQuery(1), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
