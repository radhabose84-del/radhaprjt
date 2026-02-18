using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposalById
{
    public class GetAssetDisposalByIdQuery : IRequest<AssetDisposalDto>
    {
        public int Id { get; set; }
    }
}