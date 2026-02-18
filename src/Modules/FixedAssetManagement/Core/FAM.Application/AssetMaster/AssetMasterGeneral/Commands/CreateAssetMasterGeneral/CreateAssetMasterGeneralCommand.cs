using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral
{
    public class CreateAssetMasterGeneralCommand : IRequest<AssetMasterDto>
    {
       public AssetMasterDto? AssetMaster { get; set; }       
    }
}