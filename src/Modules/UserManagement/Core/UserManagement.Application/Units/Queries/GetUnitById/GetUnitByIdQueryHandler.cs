#nullable disable
using AutoMapper;
using Contracts.Interfaces.Lookups.Party;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Units.Queries.GetUnitById
{
    //public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery,UnitDto>
    public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery,GetUnitsByIdDto>
    {
         private readonly IUnitQueryRepository _unitRepository;
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly IBankAccountLookup _bankAccountLookup;

         private readonly ILogger<GetUnitByIdQueryHandler> _logger;

        public GetUnitByIdQueryHandler(IUnitQueryRepository unitRepository, IMapper mapper, IMediator mediator, IBankAccountLookup bankAccountLookup, ILogger<GetUnitByIdQueryHandler> logger)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
            _mediator = mediator;
            _bankAccountLookup = bankAccountLookup;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

         public async Task<GetUnitsByIdDto> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching Unit Request started: {request.Id}");
            var units = await _unitRepository.GetByIdAsync(request.Id);

              if (units is null)
                {
                    // Not found is a normal read outcome → return null so the controller responds
                    // 200 with data:null, matching the other GetById handlers (not a 400).
                    _logger.LogWarning($"No Unit Record {request.Id} not found in DB.");
                    return null;
                }

            var unitList = _mapper.Map<GetUnitsByIdDto>(units);

            // Populate bank account display fields via cross-module lookup (rule #3 — no JOIN to Party schema)
            if (unitList.BankAccountId is > 0)
            {
                var bankAccount = await _bankAccountLookup.GetByIdAsync(unitList.BankAccountId.Value, cancellationToken);
                if (bankAccount is not null)
                {
                    unitList.BankAccountNumber = bankAccount.AccountNumber;
                    unitList.BankName = bankAccount.BankName;
                    unitList.BankAccountDetails = bankAccount;
                }
            }
            //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetUnitByIdQuery",
                    actionCode: unitList.Id.ToString(),        
                    actionName: unitList.UnitName,
                    details: $"Unit '{unitList.UnitName}' was Fetched. UnitId: {unitList.Id}",
                    module:"Unit"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"Fetching Unit Request Completed: {request.Id}");
            return unitList;
     
          
        }
    }
}   