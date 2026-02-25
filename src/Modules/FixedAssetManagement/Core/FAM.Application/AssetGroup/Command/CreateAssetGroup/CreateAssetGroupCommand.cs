using MediatR;

namespace FAM.Application.AssetGroup.Command.CreateAssetGroup
{
    public class CreateAssetGroupCommand : IRequest<int>
    {
        public string? Code { get; set; }
        public string? GroupName { get; set; }
        public decimal? GroupPercentage { get; set; }
        
       
    }
}
