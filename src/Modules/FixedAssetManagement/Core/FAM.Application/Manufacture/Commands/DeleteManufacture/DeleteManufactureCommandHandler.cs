

using AutoMapper;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Manufacture.Commands.DeleteManufacture
{
    public class DeleteManufactureCommandHandler   : IRequestHandler<DeleteManufactureCommand, ManufactureDTO>
    {
         private readonly IManufactureCommandRepository _manufactureRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly IManufactureQueryRepository _manufactureQueryRepository;

        public DeleteManufactureCommandHandler(IManufactureCommandRepository manufactureRepository, IMapper mapper,  IMediator mediator,IManufactureQueryRepository manufactureQueryRepository)
        {
            _manufactureRepository = manufactureRepository;
             _mapper = mapper;        
            _mediator = mediator;
            _manufactureQueryRepository=manufactureQueryRepository;
        }

        public async Task<ManufactureDTO> Handle(DeleteManufactureCommand request, CancellationToken cancellationToken)
        {
              var manufactures = await _manufactureQueryRepository.GetByIdAsync(request.Id);
            if (manufactures is null )
            {
                throw new ValidationException("Invalid ManufactureID.");
               
            }
            //Check if linked with any MachineGroup
              var linked = await _manufactureQueryRepository.IsManufactureLinkedAsync(request.Id);
            if (linked)
            {
                throw new ValidationException("This master is linked with other records and cannot be deleted.");
            }
          
            var manufacturesDelete = _mapper.Map<Manufactures>(request);      
            var updateResult = await _manufactureRepository.DeleteAsync(request.Id, manufacturesDelete);
            if (updateResult > 0)
            {
                var manufactureDto = _mapper.Map<ManufactureDTO>(manufacturesDelete);  
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: manufacturesDelete.Code ?? string.Empty,
                    actionName: manufacturesDelete.ManufactureName ?? string.Empty,
                    details: $"Manufacture '{manufactureDto.ManufactureName}' was created. Code: {manufactureDto.Code}",
                    module:"Manufacture"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);                 
                return  manufactureDto;
            }
            throw new Exception("Manufacture deletion failed.");
        
        }
    }
}