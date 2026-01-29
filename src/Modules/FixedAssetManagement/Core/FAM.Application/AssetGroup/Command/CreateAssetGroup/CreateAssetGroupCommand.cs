using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
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
