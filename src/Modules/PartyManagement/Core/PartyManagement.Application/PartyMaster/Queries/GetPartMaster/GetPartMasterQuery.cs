using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Application.Common.HttpResponse;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMaster
{
    public class GetPartMasterQuery : IRequest<ApiResponseDTO<List<GetPartyMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}