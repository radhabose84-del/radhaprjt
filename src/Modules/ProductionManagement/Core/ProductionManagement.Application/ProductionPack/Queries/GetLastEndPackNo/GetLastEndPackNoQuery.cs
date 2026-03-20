using MediatR;

namespace ProductionManagement.Application.ProductionPack.Queries.GetLastEndPackNo
{
    public class GetLastEndPackNoQuery : IRequest<int>
    {
        public int ProductionYear { get; set; }
    }
}
