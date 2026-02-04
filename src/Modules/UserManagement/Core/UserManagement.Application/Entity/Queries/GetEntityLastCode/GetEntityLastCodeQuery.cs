using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Entity.Queries.GetEntityLastCode
{
    public class GetEntityLastCodeQuery : IRequest<ApiResponseDTO<string>>
    {
        
    }
}