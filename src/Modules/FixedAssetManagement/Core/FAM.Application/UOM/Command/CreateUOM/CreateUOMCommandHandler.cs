using AutoMapper;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.UOM.Command.CreateUOM
{
    public class CreateUOMCommandHandler : IRequestHandler<CreateUOMCommand, UOMDto>
    {
         private readonly IUOMCommandRepository _uomCommandRepository;
        private readonly IUOMQueryRepository _uomQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public CreateUOMCommandHandler(IUOMCommandRepository uomCommandRepository,IUOMQueryRepository uomQueryRepository,IMapper mapper,IMediator mediator)
        {
            _uomCommandRepository = uomCommandRepository;
            _uomQueryRepository = uomQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<UOMDto> Handle(CreateUOMCommand request, CancellationToken cancellationToken)
        {
            var existingUOM = await _uomQueryRepository.GetByUOMNameAsync(request.UOMName ?? string.Empty);

               if (existingUOM != null)
               {
                throw new ValidationException("UOM already exists");
                   
               }
           
                 var uom  = _mapper.Map<FAM.Domain.Entities.UOM>(request);

                var uomresult = await _uomCommandRepository.CreateAsync(uom);
                
                var locationMap = _mapper.Map<UOMDto>(uomresult);
                if (uomresult.Id > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: uomresult.Code,
                     actionName: uomresult.UOMName,
                     details: $"UOM '{uomresult.Code}' was created. UOMName: {uomresult.UOMName}",
                     module:"UOM"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return locationMap;
                }
               throw new Exception("UOM not created");
                    
        }
    }
}