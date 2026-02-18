using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Common.Interfaces;
using MediatR;
using AutoMapper;
using UserManagement.Application.Common;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Events;
using Contracts.Common;
using FluentValidation;

namespace UserManagement.Application.Country.Queries.GetCountryById
{
    public class GetCountryByIdQueryHandler : IRequestHandler<GetCountryByIdQuery, CountryDto>
    {
        private readonly ICountryQueryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCountryByIdQueryHandler(ICountryQueryRepository countryRepository, IMapper mapper, IMediator mediator)
        {
            _countryRepository = countryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CountryDto> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
        {           
            var country = await _countryRepository.GetByIdAsync(request.Id);
            if (country is null)
            {
                throw new ValidationException("Country not found");
             
            }            
            var countryDto = _mapper.Map<CountryDto>(country);
                
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: countryDto.CountryCode ?? string.Empty,        
                actionName: countryDto.CountryName ?? string.Empty,                
                details: $"Country '{countryDto.CountryName}' was created. CountryCode: {countryDto.CountryCode}",
                module:"Country"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  countryDto;           
        }
    }
}