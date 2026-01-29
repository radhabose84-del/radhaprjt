using MediatR;
using System.Data;
using Core.Application.Common.Interfaces.IEntity;
using AutoMapper;
using Core.Application.Entity.Queries.GetEntity;
using Core.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;

namespace Core.Application.Entity.Queries.GetEntityLastCode
{
    public class GetEntityLastCodeQueryHandler : IRequestHandler<GetEntityLastCodeQuery,ApiResponseDTO<string>>
    {
    private readonly IEntityQueryRepository _entityRepository;        
    private readonly IMapper _mapper;
    private readonly ILogger<GetEntityLastCodeQueryHandler> _logger;

    public GetEntityLastCodeQueryHandler(IEntityQueryRepository entityRepository,  IMapper mapper,ILogger<GetEntityLastCodeQueryHandler> logger)
    {
         _entityRepository = entityRepository;
         _mapper =mapper;
         _logger = logger?? throw new ArgumentNullException(nameof(logger));
    }

        public async Task<ApiResponseDTO<string>> Handle(GetEntityLastCodeQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching EntityCode Request started: {request}");
           var entityCode = await _entityRepository.GenerateEntityCodeAsync();
           _logger.LogInformation($"Fetching EntityCode Request Completed: {entityCode}");
           return new ApiResponseDTO<string> { IsSuccess = true, Message = "Success", Data = entityCode };
        }       

    }
}