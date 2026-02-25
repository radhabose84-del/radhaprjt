using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway
{
    public class CreateGRNPutawayCommand : IRequest<int>
    {
          public List<CreateGRNPutawayDto>? PutawayList { get; set; } 
    }
}