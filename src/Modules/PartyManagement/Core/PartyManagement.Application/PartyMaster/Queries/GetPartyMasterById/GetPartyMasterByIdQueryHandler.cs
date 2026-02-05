using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IUser;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Common;
using PartyManagement.Domain.Events;
using MediatR;
using static PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById.PartyMasterDto;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById
{
    public class GetPartyMasterByIdQueryHandler : IRequestHandler<GetPartyMasterByIdQuery, PartyMasterDto>
    {
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICityGrpcClient _cityGrpcClient;
        private readonly IStatesGrpcClient _stateGrpcClient;
        private readonly ICountryGrpcClient _countryGrpcClient;
        private readonly ICompanyGrpcClient _companyGrpcClient;
        private readonly IUnitGrpcClient _unitGrpcClient;

        public GetPartyMasterByIdQueryHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IMapper mapper, IMediator mediator, ICityGrpcClient cityGrpcClient, IStatesGrpcClient stateGrpcClient, ICountryGrpcClient countryGrpcClient, ICompanyGrpcClient companyGrpcClient, IUnitGrpcClient unitGrpcClient)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _cityGrpcClient = cityGrpcClient;
            _stateGrpcClient = stateGrpcClient;
            _countryGrpcClient = countryGrpcClient;
            _companyGrpcClient = companyGrpcClient;
            _unitGrpcClient = unitGrpcClient;
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
                                
              // Run gRPC calls concurrently
                var cityTask    = _cityGrpcClient.GetAllCityAsync();
                var stateTask   = _stateGrpcClient.GetAllStateAsync();
                var countryTask = _countryGrpcClient.GetAllCountryAsync();
               // var companyTask = _companyGrpcClient.GetAllCompanyAsync();
                var unitTask    = _unitGrpcClient.GetAllUnitAsync();

                await Task.WhenAll(cityTask, stateTask, countryTask, unitTask);

                var cityDict    = cityTask.Result.ToDictionary(x => x.CityId, x => x.CityName);
                var stateDict   = stateTask.Result.ToDictionary(x => x.StateId, x => x.StateName);
                var countryDict = countryTask.Result.ToDictionary(x => x.CountryId, x => x.CountryName);
               // var companyDict = companyTask.Result.ToDictionary(x => x.CompanyId, x => x.CompanyName);
                var unitDict    = unitTask.Result.ToDictionary(x => x.UnitId, x => x.UnitName);

                // Map address City/State/Country names
            if (dto.PartyAddresses != null && dto.PartyAddresses.Any())
            {
                foreach (var addr in dto.PartyAddresses)
                {
                    if (addr.CityId.HasValue && cityDict.TryGetValue(addr.CityId.Value, out var cityName))
                        addr.City = cityName;

                    if (addr.StateId.HasValue && stateDict.TryGetValue(addr.StateId.Value, out var stateName))
                        addr.State = stateName;

                    if (addr.CountryId.HasValue && countryDict.TryGetValue(addr.CountryId.Value, out var countryName))
                        addr.Country = countryName;

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