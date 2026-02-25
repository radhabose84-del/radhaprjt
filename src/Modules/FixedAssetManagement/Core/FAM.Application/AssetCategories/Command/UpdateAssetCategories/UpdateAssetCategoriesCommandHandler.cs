#nullable disable
using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetCategories.Command.UpdateAssetCategories
{
    public class UpdateAssetCategoriesCommandHandler : IRequestHandler<UpdateAssetCategoriesCommand, int>
    {
        private readonly IAssetCategoriesCommandRepository _iAssetCategoriesCommandRepository;
        private readonly IAssetCategoriesQueryRepository _iAssetCategoriesQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator; 

        public UpdateAssetCategoriesCommandHandler( IAssetCategoriesCommandRepository iAssetCategoriesCommandRepository, IAssetCategoriesQueryRepository iAssetCategoriesQueryRepository, IMapper imapper, IMediator mediator)
        {
            _iAssetCategoriesCommandRepository = iAssetCategoriesCommandRepository;
            _Imapper = imapper;
            _mediator = mediator;
            _iAssetCategoriesQueryRepository = iAssetCategoriesQueryRepository;
        }

        public async Task<int> Handle(UpdateAssetCategoriesCommand request, CancellationToken cancellationToken)
        {
        // 🔹 First, check if the ID exists in the database
        var existingassetcategory = await _iAssetCategoriesQueryRepository.GetByIdAsync(request.Id);
        if (existingassetcategory is null)
        {
            throw new ValidationException("AssetCategory Id not found / AssetCategory is deleted .");
       
        }
            // Block inactivation if linked with SubCategories
            if (request.IsActive == 0) // 0 = Inactive
            {
                var isLinked = await _iAssetCategoriesQueryRepository.IsAssetCategoryLinkedAsync(request.Id);
                if (isLinked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

         // Check for duplicate GroupName or SortOrder
       var (isNameDuplicate, isSortOrderDuplicate) = await _iAssetCategoriesCommandRepository
                                .CheckForDuplicatesAsync(request.CategoryName, request.SortOrder, request.Id);

        if (isNameDuplicate || isSortOrderDuplicate)
        {
            string errorMessage = isNameDuplicate && isSortOrderDuplicate
            ? "Both Category Name and Sort Order already exist."
            : isNameDuplicate
            ? "AssetCategory with the same CategoryName already exists."
            : "AssetCategory with the same Sort Order already exists.";

            throw new ValidationException(errorMessage);
           
        }
        var assetCategories = _Imapper.Map<FAM.Domain.Entities.AssetCategories>(request);
        var result = await _iAssetCategoriesCommandRepository.UpdateAsync(request.Id, assetCategories);

        // AssetGroup not found
        {
        if (result <= 0) 
           throw new ValidationException("AssetGroup not found.");
        }

        //Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: assetCategories.Code,
            actionName: assetCategories.CategoryName,
            details: $"AssetCategory details was updated",
            module: "AssetCategory");
        await _mediator.Publish(domainEvent, cancellationToken);
     
        return result ;  
        }
    }      
    }
