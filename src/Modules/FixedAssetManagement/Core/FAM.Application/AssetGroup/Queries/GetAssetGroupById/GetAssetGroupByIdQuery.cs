using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroupById
{
    public class GetAssetGroupByIdQuery : IRequest<AssetGroupDto>
    {
        public int Id { get; set; }
    }
}