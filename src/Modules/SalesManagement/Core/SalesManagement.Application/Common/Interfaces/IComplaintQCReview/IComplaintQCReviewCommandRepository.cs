namespace SalesManagement.Application.Common.Interfaces.IComplaintQCReview
{
    public interface IComplaintQCReviewCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ComplaintQCReview entity);
        Task<int> UpdateAsync(Domain.Entities.ComplaintQCReview entity, List<Domain.Entities.ComplaintQCReviewAssignment> assignments);
    }
}
