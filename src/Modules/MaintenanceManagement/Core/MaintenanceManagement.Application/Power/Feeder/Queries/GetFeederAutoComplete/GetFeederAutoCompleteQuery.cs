using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete
{
    public class GetFeederAutoCompleteQuery : IRequest<List<GetFeederAutoCompleteDto>>  
    {
            public string? SearchPattern { get; set; }
    }
}