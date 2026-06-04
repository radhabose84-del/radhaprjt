using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetPartyMasterByIdQueryHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCityLookup = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockStateLookup = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _mockCountryLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IBankAccountLookup> _mockBankLookup = new(MockBehavior.Loose);

        private GetPartyMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockCityLookup.Object, _mockStateLookup.Object, _mockCountryLookup.Object,
                _mockCompanyLookup.Object, _mockUnitLookup.Object, _mockBankLookup.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = new PartyMasterDto
            {
                Id = id,
                PartyCode = "PAR001",
                PartyName = "Test Party",
                PartyDocuments = null,
                PartyAddresses = null,
                PartyUnitCompanyMappings = null
            };

            _mockQueryRepo
                .Setup(r => r.GetByIdPartyMasterAsync(id))
                .ReturnsAsync(dto);

            _mockQueryRepo
                .Setup(r => r.GetDocumentDirectoryPath())
                .ReturnsAsync(new Dictionary<string, string>());

            _mockCityLookup
                .Setup(l => l.GetAllCityAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityLookupDto>());

            _mockStateLookup
                .Setup(l => l.GetAllStatesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateLookupDto>());

            _mockCountryLookup
                .Setup(l => l.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryLookupDto>());

            _mockUnitLookup
                .Setup(l => l.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>());
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            SetupHappyPath(1);

            var result = await CreateSut().Handle(
                new GetPartyMasterByIdQuery { PartyId = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsQueryRepoOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                new GetPartyMasterByIdQuery { PartyId = 1 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdPartyMasterAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                new GetPartyMasterByIdQuery { PartyId = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<PartyManagement.Domain.Events.AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
