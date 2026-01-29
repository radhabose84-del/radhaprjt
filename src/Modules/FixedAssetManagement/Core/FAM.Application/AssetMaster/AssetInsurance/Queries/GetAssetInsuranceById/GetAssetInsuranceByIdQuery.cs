using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsuranceById
{
    public class GetAssetInsuranceByIdQuery  : IRequest<GetAssetInsuranceDto>
    {
          public int Id { get; set; }
    }
}