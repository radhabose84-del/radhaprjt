using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetCategories.Command.DeleteAssetCategories
{
    public class DeleteAssetCategoriesCommandHandler : IRequestHandler<DeleteAssetCategoriesCommand, int>
    {
        private readonly IAssetCategoriesCommandRepository _iAssetCategoryCommandRepository;
        private readonly IAssetCategoriesQueryRepository _iAssetCategoryQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator;

        public DeleteAssetCategoriesCommandHandler(IAssetCategoriesCommandRepository iAssetCategoryCommandRepository,IAssetCategoriesQueryRepository iAssetCategoryQueryRepository,IMapper imapper,IMediator imediator )
        {
            _iAssetCategoryCommandRepository=iAssetCategoryCommandRepository;
            _iAssetCategoryQueryRepository=iAssetCategoryQueryRepository;
            _Imapper=imapper;
            _mediator=imediator;    
        }

        public async Task<int> Handle(DeleteAssetCategoriesCommand request, CancellationToken cancellationToken)
        {
            
            var existingAssetCategory = await _iAssetCategoryQueryRepository.GetByIdAsync(request.Id);
            if (existingAssetCategory is null)
            {
               throw new ValidationException("AssetCategory Id not found / AssetCategory is deleted .");
              
            }
            
            var isLinked = await _iAssetCategoryQueryRepository.IsAssetCategoryLinkedAsync(request.Id);
            if (isLinked)
            {
                throw new ValidationException("This master is linked with other records and cannot be deleted.");
            }

            var assetCategories = _Imapper.Map<FAM.Domain.Entities.AssetCategories>(request);
            var result = await _iAssetCategoryCommandRepository.DeleteAsync(request.Id,assetCategories);
            if (result == -1) 
            {
            throw new ValidationException("AssetCategoryId not found.");
             
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: assetCategories.Code,
                actionName: assetCategories.CategoryName,
                details: $"AssetCategory details was deleted",
                module: "AssetCategory");
            await _mediator.Publish(domainEvent);

            return result;
              
        }
    }
}