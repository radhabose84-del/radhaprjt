using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById
{
    public class GetServiceEntrySheetByIdQuery   : IRequest<ApiResponseDTO<ServiceEntrySheetDetailDto?>> 
    {
        public int SesId  { get; init; }
        
    }
}