using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupAutoComplete
{
    public class GetFeederGroupAutoCompleteQuery : IRequest<List<GetFeederGroupAutoCompleteDto>>

    {
         public string? SearchPattern { get; set; }
        
    }
}