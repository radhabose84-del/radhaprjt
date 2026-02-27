using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories
{
    public class DeleteAssetSubCategoriesCommandHandler : IRequestHandler<DeleteAssetSubCategoriesCommand, int>
    {
        private readonly IAssetSubCategoriesCommandRepository _iAssetSubCategoryCommandRepository;
        private readonly IAssetSubCategoriesQueryRepository _iAssetSubCategoryQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator;

        public DeleteAssetSubCategoriesCommandHandler(IAssetSubCategoriesCommandRepository iAssetSubCategoryCommandRepository,IAssetSubCategoriesQueryRepository iAssetSubCategoryQueryRepository,IMapper mapper,IMediator mediator )
        {
            _Imapper=mapper;
            _mediator=mediator;
            _iAssetSubCategoryCommandRepository=iAssetSubCategoryCommandRepository;
            _iAssetSubCategoryQueryRepository=iAssetSubCategoryQueryRepository;

        }
          public async Task<int> Handle(DeleteAssetSubCategoriesCommand request, CancellationToken cancellationToken)
        {
            // 🔹 First, check if the ID exists in the database
            var existingAssetCategory = await _iAssetSubCategoryQueryRepository.GetByIdAsync(request.Id);
            if (existingAssetCategory is null)
            {
             throw new ValidationException("AssetSubCategory Id not found / AssetSubCategory is deleted .");  
               
            }

            var linked = await _iAssetSubCategoryQueryRepository.IsAssetSubCategoryLinkedAsync(request.Id);
            if (linked)
            {
                throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }

            var assetsubCategories = _Imapper.Map<FAM.Domain.Entities.AssetSubCategories>(request);
            var result = await _iAssetSubCategoryCommandRepository.DeleteAsync(request.Id,assetsubCategories);
            if (result == -1) 
            {
            throw new ValidationException("AssetCategoryId not found.");
             
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: assetsubCategories.Code,
                actionName: assetsubCategories.SubCategoryName,
                details: $"AssetSubCategory details was deleted",
                module: "AssetSubCategory");
            await _mediator.Publish(domainEvent);

            return  result;
        }
    }
}