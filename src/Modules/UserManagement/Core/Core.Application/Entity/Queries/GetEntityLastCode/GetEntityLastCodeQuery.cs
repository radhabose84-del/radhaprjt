using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Entity.Queries.GetEntityLastCode
{
    public class GetEntityLastCodeQuery : IRequest<ApiResponseDTO<string>>
    {
        
    }
}