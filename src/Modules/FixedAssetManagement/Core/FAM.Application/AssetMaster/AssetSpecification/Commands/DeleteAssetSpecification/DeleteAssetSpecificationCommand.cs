using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification
{ 
    public class DeleteAssetSpecificationCommand :  IRequest<AssetSpecificationDTO>
    {
         public int Id { get; set; }    
    }
}