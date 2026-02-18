using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.Location.Queries.GetSubLocations;
using MediatR;

namespace FAM.Application.SubLocation.Queries.GetSubLocationAutoComplete
{
    public class GetSubLocationAutoCompleteQuery :  IRequest<List<SubLocationAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
        
    }
}