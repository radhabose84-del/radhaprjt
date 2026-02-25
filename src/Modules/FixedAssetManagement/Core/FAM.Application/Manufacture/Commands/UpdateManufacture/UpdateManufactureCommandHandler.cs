
using AutoMapper;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Manufacture.Commands.UpdateManufacture
{
    public class UpdateManufactureCommandHandler : IRequestHandler<UpdateManufactureCommand, bool>
    {
        private readonly IManufactureCommandRepository _manufactureRepository;
        private readonly IManufactureQueryRepository _manufactureQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public UpdateManufactureCommandHandler(IManufactureCommandRepository manufactureRepository, IMapper mapper,IManufactureQueryRepository manufactureQueryRepository, IMediator mediator)
        {
            _manufactureRepository = manufactureRepository;
            _mapper = mapper;
            _manufactureQueryRepository = 
            manufactureQueryRepository;
            _mediator = mediator;
        }
        public async Task<bool> Handle(UpdateManufactureCommand request, CancellationToken cancellationToken)
        {
            var manufactures = await _manufactureQueryRepository.GetByIdAsync(request.Id);
            if (manufactures is null)
            throw new ValidationException("Invalid ManufactureId. The specified Name does not exist.");
            
            if (request.IsActive == 0)
            {
                var linked = await _manufactureQueryRepository.IsManufactureLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }           
      
            var oldManufactureName = manufactures.ManufactureName;
            manufactures.ManufactureName = request.ManufactureName;

            if (manufactures is null || manufactures.IsDeleted is BaseEntity.IsDelete.Deleted )
            {
                throw new ValidationException("Invalid ManufactureID. The specified ManufactureName does not exist or is deleted.");

            }
            var manufactureExists = await _manufactureRepository.ExistsByCodeAsync(request.Code ?? string.Empty, request.Id);

            var nameExists = await _manufactureRepository.ExistsByNameAsync(request.ManufactureName ?? string.Empty, request.Id);

            //if both are duplicate, show combined message
            if (manufactureExists && nameExists)
            {
                throw new ValidationException("Manufacture Name and Manufacture Code already exists.");
            }

            if (manufactureExists)
            {
                throw new ValidationException("Manufacture Code already exists.");
                                       
            }

            if (nameExists)
            {
                throw new ValidationException("Manufacture Name already exists. Please use a different name.");
            }
            var updatedManufactures = _mapper.Map<Manufactures>(request);                   
            var updateResult = await _manufactureRepository.UpdateAsync(updatedManufactures);            
            
                         
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: request.Code ?? string.Empty,
                    actionName: request.ManufactureName ?? string.Empty,                            
                    details: $"Manufacture '{oldManufactureName}' was updated to '{request.ManufactureName}'.  Code: {request.Code}",
                    module:"Manufacture"
                );            
                await _mediator.Publish(domainEvent, cancellationToken);
                if (updateResult)
                { 
                    return updateResult;
                }
            throw new Exception("Manufacture not updated.");
                             
            }
    }
}