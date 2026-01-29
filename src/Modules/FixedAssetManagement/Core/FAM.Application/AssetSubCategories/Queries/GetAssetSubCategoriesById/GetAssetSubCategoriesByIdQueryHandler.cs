using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesById
{
    public class GetAssetSubCategoriesByIdQueryHandler: IRequestHandler<GetAssetSubCategoriesByIdQuery,AssetSubCategoriesDto>
    {
        private readonly IAssetSubCategoriesQueryRepository _iAssetSubCategoriesQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSubCategoriesByIdQueryHandler(IAssetSubCategoriesQueryRepository iAssetSubCategoriesQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubCategoriesQueryRepository = iAssetSubCategoriesQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }
         public async Task<AssetSubCategoriesDto> Handle(GetAssetSubCategoriesByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetSubCategoriesQueryRepository.GetByIdAsync(request.Id);
            // Check if the entity exists
            if (result is null)
            {
                throw new ValidationException($"AssetSubCategories ID {request.Id} not found.");
                
            }
            // Map a single entity
            var assetSubCategories = _mapper.Map<AssetSubCategoriesDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetSubCategories details {assetSubCategories.Id} was fetched.",
                    module:"AssetSubCategories"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return  assetSubCategories;
        }

    }
}