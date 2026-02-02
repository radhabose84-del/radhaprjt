using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete
{
    public class AutoCompleteDto
    {

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        
    }
}