using MediatR;
using AutoMapper;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Events;
using UserManagement.Domain.Enums.Common; // Enums.Status, Enums.IsDelete
using FluentValidation;

namespace UserManagement.Application.Country.Commands.UpdateCountry
{
    public class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryCommand, CountryDto>
    {
        private readonly ICountryCommandRepository _countryCommandRepo;
        private readonly ICountryQueryRepository _countryQueryRepo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateCountryCommandHandler(
            ICountryCommandRepository countryCommandRepo,
            IMapper mapper,
            ICountryQueryRepository countryQueryRepo,
            IMediator mediator)
        {
            _countryCommandRepo = countryCommandRepo;
            _mapper = mapper;
            _countryQueryRepo = countryQueryRepo;
            _mediator = mediator;
        }

        public async Task<CountryDto> Handle(UpdateCountryCommand request, CancellationToken ct)
        {
            // Entity existence check (business flow concern; validation is in validator)
            var country = await _countryQueryRepo.GetByIdAsync(request.Id);
            if (country is null || country.IsDeleted == Enums.IsDelete.Deleted)
                throw new KeyNotFoundException("Country not found or is deleted.");

            if (request.IsActive == 0) 
            {
                var linked = await _countryQueryRepo.IsLinkedWithStatesAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with Maintenance System. You cannot inactivate this record.");
            }

            // Keep old values for audit
            var oldName = country.CountryName;
            var oldCode = country.CountryCode;
            var oldStatus = country.IsActive;

            // Apply updates (validator already ensured correctness/uniqueness)
            country.CountryName = request.CountryName!.Trim();
            country.CountryCode = request.CountryCode!.Trim();
            country.IsActive = (Enums.Status)request.IsActive;

            var rows = await _countryCommandRepo.UpdateAsync(country.Id, country);
            if (rows <= 0)
                throw new InvalidOperationException("Country update failed.");

            // Reload & map
            var updated = await _countryQueryRepo.GetByIdAsync(country.Id)
                          ?? throw new InvalidOperationException("Country update persisted but reload failed.");
            var dto = _mapper.Map<CountryDto>(updated);

            // Domain Event (fixed module/name)
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: dto.CountryCode ?? string.Empty,
                actionName: dto.CountryName ?? string.Empty,
                details: $"Country '{oldName}' ({oldCode}) updated to '{dto.CountryName}' ({dto.CountryCode}). Status: {oldStatus} → {dto.IsActive}.",
                module: "Country"
            );
            await _mediator.Publish(ev, ct);

            return dto;
        }
    }
}
