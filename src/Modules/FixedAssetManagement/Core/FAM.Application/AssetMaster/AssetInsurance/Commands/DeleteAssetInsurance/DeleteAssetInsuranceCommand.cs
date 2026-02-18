using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance
{
    public class DeleteAssetInsuranceCommand :  IRequest<bool>
    {
        public int Id { get; set; }  
    }
}