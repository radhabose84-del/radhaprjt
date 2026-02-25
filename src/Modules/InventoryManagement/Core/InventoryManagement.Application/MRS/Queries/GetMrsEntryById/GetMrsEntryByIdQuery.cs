using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetMrsEntryById
{
    public class GetMrsEntryByIdQuery : IRequest<GetMrsEntryByIdDto>
    {
         public int Id { get; set; }
    }
}