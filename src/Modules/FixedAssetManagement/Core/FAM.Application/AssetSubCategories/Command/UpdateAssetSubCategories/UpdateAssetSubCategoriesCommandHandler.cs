#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories
{
    public class UpdateAssetSubCategoriesCommandHandler : IRequestHandler<UpdateAssetSubCategoriesCommand, int>
    {
         private readonly IAssetSubCategoriesCommandRepository _iAssetSubCategoriesCommandRepository;
        private readonly IAssetSubCategoriesQueryRepository _iAssetSubCategoriesQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator; 

        public UpdateAssetSubCategoriesCommandHandler(IAssetSubCategoriesCommandRepository iAssetCategoriesCommandRepository, IAssetSubCategoriesQueryRepository iAssetCategoriesQueryRepository, IMapper imapper, IMediator mediator)
        {
            _iAssetSubCategoriesCommandRepository = iAssetCategoriesCommandRepository;
            _Imapper = imapper;
            _mediator = mediator;
            _iAssetSubCategoriesQueryRepository = iAssetCategoriesQueryRepository;
        }

        public async Task<int> Handle(UpdateAssetSubCategoriesCommand request, CancellationToken cancellationToken)
        {
             // 🔹 First, check if the ID exists in the database
        var existingassetsubcategory = await _iAssetSubCategoriesQueryRepository.GetByIdAsync(request.Id);
        if (existingassetsubcategory is null)
        {
      throw new ValidationException("AssetCategory Id not found / AssetCategory is deleted .");
     
        }

            if (request.IsActive == 0)
            {
                var linked = await _iAssetSubCategoriesQueryRepository.IsAssetSubCategoryLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

         // Check for duplicate GroupName or SortOrder
            var (isNameDuplicate, isSortOrderDuplicate) = await _iAssetSubCategoriesCommandRepository
                                .CheckForDuplicatesAsync(request.SubCategoryName, request.SortOrder, request.Id);

        if (isNameDuplicate || isSortOrderDuplicate)
        {
            string errorMessage = isNameDuplicate && isSortOrderDuplicate
            ? "Both SubCategory Name and Sort Order already exist."
            : isNameDuplicate
            ? "AssetSubCategory with the same SubCategoryName already exists."
            : "AssetSubCategory with the same Sort Order already exists.";
            throw new ValidationException(errorMessage);
         
        }
        var assetsubCategories = _Imapper.Map<FAM.Domain.Entities.AssetSubCategories>(request);
        var result = await _iAssetSubCategoriesCommandRepository.UpdateAsync(request.Id, assetsubCategories);

        // AssetSubCategory not found
        {
        if (result <= 0) 
           throw new ValidationException("AssetSubCategory id not found.");
            
        }

        //Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: assetsubCategories.Code,
            actionName: assetsubCategories.SubCategoryName,
            details: $"AssetSubCategory details was updated",
            module: "AssetSubCategory");
        await _mediator.Publish(domainEvent, cancellationToken);
     
        return result;  
        }
    }
}