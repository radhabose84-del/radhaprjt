using MediatR;
using PurchaseManagement.Application.VendorRatingGrade.Dto;

namespace PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeById
{
    public class GetVendorRatingGradeByIdQuery : IRequest<VendorRatingGradeDto?>
    {
        public int Id { get; set; }
    }
}
