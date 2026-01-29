using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.DepreciationDetail.Commands.UpdateDepreciationDetail
{
    public class UpdateDepreciationDetailCommandHandler : IRequestHandler<UpdateDepreciationDetailCommand, DepreciationDto>
    {
        private readonly IDepreciationDetailCommandRepository _depreciationRepository;
        private readonly IDepreciationDetailQueryRepository _depreciationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
                
        public UpdateDepreciationDetailCommandHandler(IDepreciationDetailCommandRepository depreciationRepository, IMapper mapper,  IMediator mediator,IDepreciationDetailQueryRepository depreciationQueryRepository)
        {
            _depreciationRepository = depreciationRepository;
            _mapper = mapper;        
            _mediator = mediator;            
            _depreciationQueryRepository=depreciationQueryRepository;
        }

        public async Task<DepreciationDto> Handle(UpdateDepreciationDetailCommand request, CancellationToken cancellationToken)
        {             
            
            var depreciationLocked = await _depreciationQueryRepository.ExistDataLockedAsync( request.finYearId, request.depreciationType,request.depreciationPeriod);
            if (depreciationLocked==true)
            {
                throw new ValidationException("Already depreciation details Locked.");
           
            }
            var depreciationGroups = await _depreciationQueryRepository.ExistDataAsync( request.finYearId, request.depreciationType,request.depreciationPeriod);
            if (depreciationGroups==false)
            {
                throw new ValidationException("No details found for this period");
              
            }
            
            var depreciationUpdate = _mapper.Map<DepreciationDetails>(request);      
            var updateResult = await _depreciationRepository.UpdateAsync(request.finYearId, request.depreciationType,request.depreciationPeriod);
            if (updateResult > 0)
            {
                var depreciationGroupDto = _mapper.Map<DepreciationDto>(depreciationUpdate);  
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: depreciationUpdate.Finyear.ToString() ??string.Empty,
                    actionName: "Update",
                    details: $"Depreciation Details '{depreciationGroupDto.Company}' was Deleted. Code: {depreciationGroupDto.Unit}",
                    module:"DepreciationDetail"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);                 
                return  depreciationGroupDto;
            }
            throw new Exception("Depreciation Locked failed.");
                   
        }
    }
}