using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetAssetSpecificationById
{
    public class GetAssetSpecificationByIdQuery : IRequest<ApiResponseDTO<List<AssetSpecificationByAssetIdDto>>>
    {
        public int AssetId { get; set; }
    }
}