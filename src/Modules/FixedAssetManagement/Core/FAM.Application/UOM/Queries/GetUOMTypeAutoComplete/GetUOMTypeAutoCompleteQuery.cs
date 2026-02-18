using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace FAM.Application.UOM.Queries.GetUOMTypeAutoComplete
{
    public class GetUOMTypeAutoCompleteQuery: IRequest<List<UOMTypeAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}