using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.Reports.AssetTransferReport
{
    public class AssetTransferQuery : IRequest<ApiResponseDTO<List<AssetTransferDetailsDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}