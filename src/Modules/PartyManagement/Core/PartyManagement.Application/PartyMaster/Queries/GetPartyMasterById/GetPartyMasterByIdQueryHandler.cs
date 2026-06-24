using AutoMapper;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Common;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById
{
    public class GetPartyMasterByIdQueryHandler : IRequestHandler<GetPartyMasterByIdQuery, PartyMasterDto>
    {
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICountryLookup _countryLookup;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IBankAccountLookup _bankAccountLookup;
        private readonly ILocationMasterLookup _locationMasterLookup;
        private readonly IStationLookup _stationLookup;

        public GetPartyMasterByIdQueryHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IMapper mapper, IMediator mediator, ICityLookup cityLookup, IStateLookup stateLookup, ICountryLookup countryLookup, ICompanyLookup companyLookup, IUnitLookup unitLookup, IBankAccountLookup bankAccountLookup, ILocationMasterLookup locationMasterLookup, IStationLookup stationLookup)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _countryLookup = countryLookup;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _bankAccountLookup = bankAccountLookup;
            _locationMasterLookup = locationMasterLookup;
            _stationLookup = stationLookup;
        }
        public async Task<PartyMasterDto> Handle(GetPartyMasterByIdQuery request, CancellationToken cancellationToken)
        {
           var dto = await _ipartyMasterQueryRepository.GetByIdPartyMasterAsync(request.PartyId);
           

           // 1️⃣ Fetch MiscType descriptions for document paths
            var miscDirectories = await _ipartyMasterQueryRepository.GetDocumentDirectoryPath();
            var baseDir = miscDirectories.GetValueOrDefault(MiscEnumEntity.PartyDocumentImage.GETPARTYIMAGE, string.Empty);
            var documentFolder = miscDirectories.GetValueOrDefault(MiscEnumEntity.PartyDocumentImage.MiscCode, string.Empty);

            // 2️⃣ Map PartyDocuments to include full file path
           // Map PartyDocuments to include full file path
            if (dto.PartyDocuments != null && dto.PartyDocuments.Any())
            {
                foreach (var doc in dto.PartyDocuments)
                {
                    if (!string.IsNullOrEmpty(doc.FileName))
                    {
                        doc.FilePath = $"{baseDir}/{documentFolder}/{doc.FileName}";
                    }
                    else
                    {
                        doc.FilePath = string.Empty;
                    }
                }
            }
                                
              // Run lookup calls concurrently
                var cityTask    = _cityLookup.GetAllCityAsync(cancellationToken);
                var stateTask   = _stateLookup.GetAllStatesAsync(cancellationToken);
                var countryTask = _countryLookup.GetAllCountriesAsync(cancellationToken);
               // var companyTask = _companyLookup.GetAllCompanyAsync();
                var unitTask    = _unitLookup.GetAllUnitAsync();

                await Task.WhenAll(cityTask, stateTask, countryTask, unitTask);

                var cityDict    = cityTask.Result.ToDictionary(x => x.CityId, x => x.CityName);
                var stateDict   = stateTask.Result.ToDictionary(x => x.StateId, x => x.StateName);
                var countryDict = countryTask.Result.ToDictionary(x => x.CountryId, x => x.CountryName);
               // var companyDict = companyTask.Result.ToDictionary(x => x.CompanyId, x => x.CompanyName);
                var unitDict    = unitTask.Result.ToDictionary(x => x.UnitId, x => x.UnitName);

                // Map address City/State/Country names
            if (dto.PartyAddresses != null && dto.PartyAddresses.Any())
            {
                // Resolve Location/Station names (cross-module masters) for the addresses that carry them.
                var locationIds = dto.PartyAddresses.Where(a => a.LocationId.HasValue && a.LocationId > 0).Select(a => a.LocationId!.Value).Distinct().ToList();
                var stationIds = dto.PartyAddresses.Where(a => a.StationId.HasValue && a.StationId > 0).Select(a => a.StationId!.Value).Distinct().ToList();

                var locationDict = locationIds.Count > 0
                    ? (await _locationMasterLookup.GetByIdsAsync(locationIds, cancellationToken)).ToDictionary(l => l.Id, l => l.LocationName)
                    : new Dictionary<int, string>();
                var stationDict = stationIds.Count > 0
                    ? (await _stationLookup.GetByIdsAsync(stationIds, cancellationToken)).ToDictionary(s => s.Id, s => s.StationName)
                    : new Dictionary<int, string>();

                foreach (var addr in dto.PartyAddresses)
                {
                    if (addr.CityId.HasValue && cityDict.TryGetValue(addr.CityId.Value, out var cityName))
                        addr.City = cityName;

                    if (addr.StateId.HasValue && stateDict.TryGetValue(addr.StateId.Value, out var stateName))
                        addr.State = stateName;

                    if (addr.CountryId.HasValue && countryDict.TryGetValue(addr.CountryId.Value, out var countryName))
                        addr.Country = countryName;

                    if (addr.LocationId.HasValue && locationDict.TryGetValue(addr.LocationId.Value, out var locationName))
                        addr.LocationName = locationName;

                    if (addr.StationId.HasValue && stationDict.TryGetValue(addr.StationId.Value, out var stationName))
                        addr.StationName = stationName;
                }
            }

                 // Map Company/Unit names
            if (dto.PartyUnitCompanyMappings != null && dto.PartyUnitCompanyMappings.Any())
            {
                foreach (var addr in dto.PartyUnitCompanyMappings)
                {
                    // if (addr.CompanyId.HasValue && companyDict.TryGetValue(addr.CompanyId.Value, out var companyName))
                    //     addr.CompanyName = companyName;
                    
                    if (addr.UnitId.HasValue && unitDict.TryGetValue(addr.UnitId.Value, out var unitName))
                        addr.UnitName = unitName;
                        
                }
            }
                        

            if (dto == null)
                throw new KeyNotFoundException("PartyId not found");

            // Populate the party's designated bank account display fields via cross-schema lookup
            if (dto.BankAccountId is > 0)
            {
                var bankAccount = await _bankAccountLookup.GetByIdAsync(dto.BankAccountId.Value, cancellationToken);
                if (bankAccount is not null)
                {
                    dto.BankAccountNumber = bankAccount.AccountNumber;
                    dto.BankName = bankAccount.BankName;
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPartyMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Party details {dto.Id} fetched.",
                module: "PartyMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
