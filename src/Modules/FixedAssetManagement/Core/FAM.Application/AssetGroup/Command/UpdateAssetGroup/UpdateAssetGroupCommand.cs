using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetGroup.Command.UpdateAssetGroup
{
    public class UpdateAssetGroupCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public int SortOrder { get; set; }
        public byte IsActive { get; set; }
        public decimal? GroupPercentage { get; set; }
    }
}