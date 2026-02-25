using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetApprovedPOList;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServiceScheduleByPoAndServiceId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetSESListToApprove;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
// using MassTransit.Futures.Contracts;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO
{
    public interface IServicePurchaseOrderQueryRepository
    {
        Task<PurchaseOrderHeader?> GetByIdAsync(int id, CancellationToken ct);   // for internal check alone

        // Task<CreateServicePurchaseOrderDto?> GetServicePOByIdAsync(int id, CancellationToken ct);  // main GetByIDasync
       Task<PurchaseOrderServiceDetailDto?> GetServicePOByIdAsync(int id, CancellationToken ct);  // main GetByIDasync

        Task<(List<GetServicePOPendingGroupDto> Rows, int Total)> GetServicePOPendingAsync(int? page, int? size, string? search, int? poId, CancellationToken ct);

        Task<ServicePOHeaderForSesDto?> GetServicePOHeaderForSesAsync(int poId, CancellationToken ct);

        Task<PoServiceHeaderByIdDto?> GetServicePoHeaderByIdAsync(int poId, CancellationToken ct);

        Task<List<ServiceScheduleDto>> GetByPoAndServiceIdAsync(int purchaseOrderId, int serviceId, CancellationToken ct);

        Task<List<PoIdNumberDto>> GetApprovedServicePoAsync();

        Task<IEnumerable<GetServicePOLinesDto>> GetLinesByPoIdAsync(int poId, CancellationToken ct);

        Task<SesFromScheduleRawDto?> GetSesCreateSourceAsync(int purchaseOrderId, int scheduleNo, int serviceItemId, CancellationToken ct = default);


        Task<GetServiceEntrySheetDto?> GetSESByIdAsync(int id, CancellationToken ct = default);

        Task<List<SesApprovalListDto>> GetServiceEntrySheetsForApprovalAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate, int? vendorId, CancellationToken ct = default);

        Task<IEnumerable<ServiceEntrySheetWithActivitiesDto>> GetByPurchaseOrderIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default);

        Task<(List<GetServiceEntrySheetListDto> Rows, int Total)> GetAllServiceEntrySheetsAsync(int pageNumber, int pageSize, string? searchTerm);
         


        Task<ServiceEntrySheetDetailDto.SesDto?> GetSesByIdAsync( int sesId, CancellationToken ct = default);

        Task<List<ServiceEntrySheetDetailDto.ActivityDto>> GetSesActivitiesAsync( int sesId,CancellationToken ct = default);

        Task<List<ServiceEntrySheetDetailDto.DocumentDto>> GetserviceEntrySheetDocumentDtosGetSesByIdAsync(    int sesId,    CancellationToken ct = default);

        Task<ServiceEntrySheetDetailDto.PurchaseOrderHeaderDto?> GetPoHeaderByIdAsync(int poId, CancellationToken ct = default);

        Task<List<ServiceEntrySheetDetailDto.PaymentTermDto>> GetPaymentTermsByPoIdAsync( int poId,  CancellationToken ct = default);

        Task<List<ServiceEntrySheetDetailDto.ServiceHeaderDto>> GetServiceHeadersByPoIdAsync(int poId, CancellationToken ct = default);

        Task<List<ServiceEntrySheetDetailDto.ServiceLineDto>> GetServiceLinesByPoAndServiceAsync( int poId, int serviceId,  CancellationToken ct = default);

        Task<List<ServiceEntrySheetDetailDto.ServiceScheduleDto>> GetServiceSchedulesByPoAndScheduleAsync(  int poId, int scheduleId, CancellationToken ct = default);

    }
}