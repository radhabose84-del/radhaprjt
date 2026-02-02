using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete
{
    public class GetServiceAutocompleteQuery : IRequest< List<ServiceMasterAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}