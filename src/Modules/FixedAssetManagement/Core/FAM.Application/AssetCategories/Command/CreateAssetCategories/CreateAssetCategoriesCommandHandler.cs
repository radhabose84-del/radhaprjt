using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetCategories.Command.CreateAssetCategories
{
    public class CreateAssetCategoriesCommandHandler : IRequestHandler<CreateAssetCategoriesCommand, int>
    {
        private readonly IAssetCategoriesCommandRepository _iAssetCategoriesCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateAssetCategoriesCommandHandler(IAssetCategoriesCommandRepository iAssetCategoriesCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iAssetCategoriesCommandRepository = iAssetCategoriesCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreateAssetCategoriesCommand request, CancellationToken cancellationToken)
        {
           // Check if AssetGroup code already exists
            // var exists = await _iAssetCategoriesCommandRepository.ExistsByCodeAsync(request.Code);
            // if (exists)
            // {
            //     throw new ValidationException("AssetCategories Code already exists.");
            // }
            var assetCategories = _imapper.Map<FAM.Domain.Entities.AssetCategories>(request);
			var categorycode = await GenerateUniqueCodeAsync(request.CategoryName ?? string.Empty);
            assetCategories.Code = categorycode;
            
            var result = await _iAssetCategoriesCommandRepository.CreateAsync(assetCategories);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetCategories.Code,
                actionName: assetCategories.CategoryName,
                details: $"AssetCategories details was created",
                module: "AssetCategories");
            await _imediator.Publish(domainEvent, cancellationToken);
          
            
            if (result <= 0)
             {
              
               throw new Exception("AssetCategories Creation Failed");
             }
             return result;
            
        }
  private async Task<string> GenerateUniqueCodeAsync(string categoryName)
        {
            // Take first 4 alphanumeric uppercase characters from the group name
            var baseCode = new string(categoryName
                .Where(char.IsLetterOrDigit)             // Remove special chars
                .Take(4)                                  // Take first 4
                .Select(char.ToUpper)                    // Convert to uppercase
                .ToArray());

            if (string.IsNullOrWhiteSpace(baseCode))
                baseCode = "GRP"; // Fallback if name doesn't contain valid chars

            string code = baseCode;
            int counter = 1;

            // Loop to generate unique code like COMP, COMP1, COMP2, etc.
            while (await _iAssetCategoriesCommandRepository.ExistsByCodeAsync(code))
            {
                code = $"{baseCode}{counter}";
                counter++;
            }

            return code;
        }
    }
}