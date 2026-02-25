#nullable disable
using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories
{
    public class CreateAssetSubCategoriesCommandHandler: IRequestHandler<CreateAssetSubCategoriesCommand, int>
    {
        private readonly IAssetSubCategoriesCommandRepository _iAssetSubCategoriesCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateAssetSubCategoriesCommandHandler(IAssetSubCategoriesCommandRepository iAssetSubCategoriesCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iAssetSubCategoriesCommandRepository = iAssetSubCategoriesCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreateAssetSubCategoriesCommand request, CancellationToken cancellationToken)
        {
             // Check if AssetGroup code already exists
            // var exists = await _iAssetSubCategoriesCommandRepository.ExistsByCodeAsync(request.Code);
            // if (exists)
            // {
            //     throw new ValidationException("AssetSubCategories Code already exists.");
              
            // }
            var assetSubCategories = _imapper.Map<FAM.Domain.Entities.AssetSubCategories>(request);
			var subcategorycode = await GenerateUniqueCodeAsync(request.SubCategoryName);
            assetSubCategories.Code = subcategorycode;
            
            var result = await _iAssetSubCategoriesCommandRepository.CreateAsync(assetSubCategories);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetSubCategories.Code,
                actionName: assetSubCategories.SubCategoryName,
                details: $"AssetSubCategories details was created",
                module: "AssetSubCategories");
            await _imediator.Publish(domainEvent, cancellationToken);
          
            
            if (result > 0)
                  {
                   
                        return  result;
                 }
                 throw new Exception("AssetSubCategories Creation Failed");
            
        }
  private async Task<string> GenerateUniqueCodeAsync(string subcategoryName)
        {
            // Take first 4 alphanumeric uppercase characters from the group name
            var baseCode = new string(subcategoryName
                .Where(char.IsLetterOrDigit)             // Remove special chars
                .Take(4)                                  // Take first 4
                .Select(char.ToUpper)                    // Convert to uppercase
                .ToArray());

            if (string.IsNullOrWhiteSpace(baseCode))
                baseCode = "GRP"; // Fallback if name doesn't contain valid chars

            string code = baseCode;
            int counter = 1;

            // Loop to generate unique code like COMP, COMP1, COMP2, etc.
            while (await _iAssetSubCategoriesCommandRepository.ExistsByCodeAsync(code))
            {
                code = $"{baseCode}{counter}";
                counter++;
            }

            return code;
        }
    }
}