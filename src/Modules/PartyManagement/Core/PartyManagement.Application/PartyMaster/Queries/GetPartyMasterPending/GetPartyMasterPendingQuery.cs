using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyMasterPending
{
    public class GetPartyMasterPendingQuery : IRequest<ApiResponseDTO<List<PartyMasterPendingDto>>>
    {
        public string? SearchTerm { get; set; }
    }
}