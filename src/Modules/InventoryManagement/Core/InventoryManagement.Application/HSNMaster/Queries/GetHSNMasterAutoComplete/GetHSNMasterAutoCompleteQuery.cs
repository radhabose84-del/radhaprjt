using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete
{
    public class GetHSNMasterAutoCompleteQuery : IRequest<List<GetHSNMasterAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }     
        public string? TypeCode { get; set; }   
        
    }
}