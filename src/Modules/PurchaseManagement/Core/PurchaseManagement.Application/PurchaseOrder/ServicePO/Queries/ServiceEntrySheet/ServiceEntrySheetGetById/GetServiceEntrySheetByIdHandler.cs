// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using PurchaseManagement.Application.Common.Exceptions;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;
// using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById
// {
//     public class GetServiceEntrySheetByIdHandler : IRequestHandler<GetServiceEntrySheetByIdQuery, ApiResponseDTO<ServiceEntrySheetDetailDto?>>
//     {


//         private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;

//         public GetServiceEntrySheetByIdHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator)
//         {
//             _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;

//         }
//         public async Task<ApiResponseDTO<ServiceEntrySheetDetailDto?>> Handle(GetServiceEntrySheetByIdQuery request, CancellationToken cancellationToken)
//         {
//             // 1️⃣ First get SES (we need PurchaseOrderId, ServiceId, ScheduleId)
//             var ses = await _servicePurchaseOrderQueryRepository.GetSesByIdAsync(request.SesId, cancellationToken);

//             if (ses == null)
//             {
//                 return new ApiResponseDTO<ServiceEntrySheetDetailDto?>
//                 {
//                     IsSuccess = false,
//                     StatusCode = 404,
//                     Message = "Service Entry Sheet not found.",
//                     Data = null
//                 };
//             }
            
//               // 2️⃣ Run repo calls SEQUENTIALLY (no Task.WhenAll, avoids closed connection issue)
//             var activities       = await _servicePurchaseOrderQueryRepository
//                 .GetSesActivitiesAsync(ses.Id, cancellationToken);

//             var documents = await _servicePurchaseOrderQueryRepository
//                  .GetserviceEntrySheetDocumentDtosGetSesByIdAsync(ses.Id, cancellationToken);   

//             var poHeader         = await _servicePurchaseOrderQueryRepository
//                 .GetPoHeaderByIdAsync(ses.PurchaseOrderId, cancellationToken);

//             var paymentTerms     = await _servicePurchaseOrderQueryRepository
//                 .GetPaymentTermsByPoIdAsync(ses.PurchaseOrderId, cancellationToken);

//             var serviceHeaders   = await _servicePurchaseOrderQueryRepository
//                 .GetServiceHeadersByPoIdAsync(ses.PurchaseOrderId, cancellationToken);

//             var serviceLines     = await _servicePurchaseOrderQueryRepository
//                 .GetServiceLinesByPoAndServiceAsync(ses.PurchaseOrderId, ses.ServiceId, cancellationToken);

//             var serviceSchedules = await _servicePurchaseOrderQueryRepository
//                 .GetServiceSchedulesByPoAndScheduleAsync(ses.PurchaseOrderId, ses.ScheduleId, cancellationToken);

//             // 3️⃣ Build combined DTO
//             var detail = new ServiceEntrySheetDetailDto
//             {
//                 Ses                = ses,
//                 Activities         = activities,
//                 Documents          = documents,
//                 PurchaseOrderHeader = poHeader,
//                 PaymentTerms       = paymentTerms,
//                 ServiceHeaders     = serviceHeaders,
//                 ServiceLines       = serviceLines,
//                 ServiceSchedules   = serviceSchedules
//             };

//             return new ApiResponseDTO<ServiceEntrySheetDetailDto?>
//             {
//                 IsSuccess  = true,
//                 StatusCode = 200,
//                 Message    = "Service Entry Sheet full details retrieved successfully.",
//                 Data       = detail
//             };

//             // // 2️⃣ Fire all related queries in parallel
//             // var activitiesTask       = _servicePurchaseOrderQueryRepository.GetSesActivitiesAsync(ses.Id, cancellationToken);
//             // var poHeaderTask         = _servicePurchaseOrderQueryRepository.GetPoHeaderByIdAsync(ses.PurchaseOrderId, cancellationToken);
//             // var paymentTermsTask     = _servicePurchaseOrderQueryRepository.GetPaymentTermsByPoIdAsync(ses.PurchaseOrderId, cancellationToken);
//             // var serviceHeadersTask   = _servicePurchaseOrderQueryRepository.GetServiceHeadersByPoIdAsync(ses.PurchaseOrderId, cancellationToken);
//             // var serviceLinesTask     = _servicePurchaseOrderQueryRepository.GetServiceLinesByPoAndServiceAsync(ses.PurchaseOrderId, ses.ServiceId, cancellationToken);
//             // var serviceSchedulesTask = _servicePurchaseOrderQueryRepository.GetServiceSchedulesByPoAndScheduleAsync(ses.PurchaseOrderId, ses.ScheduleId, cancellationToken);

//             // await Task.WhenAll(
//             //     activitiesTask,
//             //     poHeaderTask,
//             //     paymentTermsTask,
//             //     serviceHeadersTask,
//             //     serviceLinesTask,
//             //     serviceSchedulesTask
//             // );

//             // // 3️⃣ Collect results
//             // var detail = new ServiceEntrySheetDetailDto
//             // {
//             //     Ses                = ses,
//             //     Activities         = await activitiesTask,
//             //     PurchaseOrderHeader= await poHeaderTask,
//             //     PaymentTerms       = await paymentTermsTask,
//             //     ServiceHeaders     = await serviceHeadersTask,
//             //     ServiceLines       = await serviceLinesTask,
//             //     ServiceSchedules   = await serviceSchedulesTask
//             // };

//             // return new ApiResponseDTO<ServiceEntrySheetDetailDto?>
//             // {
//             //     IsSuccess  = true,
//             //     StatusCode = 200,
//             //     Message    = "Service Entry Sheet full details retrieved successfully.",
//             //     Data       = detail
//             // };
//         }
        
//     }
// }