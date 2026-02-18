using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance
{
    public class UpdateAssetInsuranceCommand  :   IRequest<bool>
    {   
        public int Id { get; set; }
         public int  AssetId { get; set; }       
        public string? PolicyNo { get; set; }       
        public DateOnly StartDate { get; set; }
        public int Insuranceperiod { get; set; }  
        public DateOnly EndDate { get; set; }
        public decimal PolicyAmount { get; set; }
        public string? VendorCode { get; set; }
        public int RenewalStatus { get; set; }
        public DateOnly RenewedDate { get; set; }
        public byte IsActive { get; set; }
    }
}