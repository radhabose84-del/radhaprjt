using MediatR;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryById
{
    public class GetOCREntryByIdQuery : IRequest<OCREntryDto?>
    {
        public int Id { get; set; }
    }
}
