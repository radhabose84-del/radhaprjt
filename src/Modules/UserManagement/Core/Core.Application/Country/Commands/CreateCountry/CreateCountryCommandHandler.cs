using AutoMapper;
using Core.Application.Country.Commands.CreateCountry;
using Core.Application.Country.Queries.GetCountries;
using Core.Application.Common.Interfaces.ICountry;
using Core.Domain.Entities;
using Core.Domain.Events;
using MediatR;

public class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommand, CountryDto>
{
    private readonly IMapper _mapper;
    private readonly ICountryCommandRepository _countryRepository;
    private readonly IMediator _mediator;

    public CreateCountryCommandHandler(
        IMapper mapper,
        ICountryCommandRepository countryRepository,
        IMediator mediator)
    {
        _mapper = mapper;
        _countryRepository = countryRepository;
        _mediator = mediator;
    }

    public async Task<CountryDto> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        // Map to entity
        var entity = _mapper.Map<Countries>(request);


        // Persist
        var created = await _countryRepository.CreateAsync(entity);
        if (created is null || created.Id <= 0)
            throw new InvalidOperationException("Country not created.");

        // Domain event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: created.CountryCode ?? string.Empty,
            actionName: created.CountryName ?? string.Empty,
            details: $"Country '{created.CountryName}' was created. CountryCode: {created.CountryCode}",
            module: "Country"
        );
        await _mediator.Publish(domainEvent, cancellationToken);

        // Map to DTO and return
        return _mapper.Map<CountryDto>(created);
    }
}
