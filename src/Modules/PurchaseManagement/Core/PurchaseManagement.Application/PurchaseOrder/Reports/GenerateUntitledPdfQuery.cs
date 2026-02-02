using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Reports;
public sealed record GenerateUntitledPdfQuery(int UnitId,int PoId) : IRequest<byte[]>;
