using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.HttpResponse;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES
{
    public class GetServiceEntrySheetsWithActivitiesByPoIdQuery  : IRequest<ApiResponseDTO<List<ServiceEntrySheetWithActivitiesDto>>>
    {
        public int PurchaseOrderId { get; }

        public GetServiceEntrySheetsWithActivitiesByPoIdQuery(int purchaseOrderId)
        {
            PurchaseOrderId = purchaseOrderId;
        }
        
    }
}