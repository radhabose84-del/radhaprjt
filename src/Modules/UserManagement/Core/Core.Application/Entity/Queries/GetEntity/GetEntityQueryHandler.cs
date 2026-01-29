using System.ComponentModel;
using AutoMapper;
using Core.Application.Common;

using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IEntity;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;


namespace Core.Application.Entity.Queries.GetEntity
{
    public class GetEntityQueryHandler : IRequestHandler<GetEntityQuery, ApiResponseDTO<List<GetEntityDTO>>>
    {
         private readonly IEntityQueryRepository _entityRepository;        
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly ILogger<GetEntityQueryHandler> _logger;

        public GetEntityQueryHandler(IEntityQueryRepository entityRepository,  IMapper mapper, IMediator mediator,ILogger<GetEntityQueryHandler> logger)
        {
           _entityRepository = entityRepository;
            _mapper =mapper;
            _mediator = mediator;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<ApiResponseDTO<List<GetEntityDTO>>> Handle(GetEntityQuery request, CancellationToken cancellationToken)
        {
          
               _logger.LogInformation($"Fetching Entity Request started: {request}");
                var (newentity, totalCount) = await _entityRepository.GetAllEntityAsync(request.PageNumber, request.PageSize, request.SearchTerm);
                if (newentity is null || !newentity.Any() || newentity.Count == 0)
                {
                 _logger.LogWarning($"No Entity Record {newentity.Count} not found in DB.");
                     return new ApiResponseDTO<List<GetEntityDTO>>
                     {
                         IsSuccess = false,
                         Message = "No entity found"
                     };
                }
                var entitylist = _mapper.Map<List<GetEntityDTO>>(newentity);
                _logger.LogInformation($"Fetching Entity Request Completed: {entitylist.Count}");
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetEntityQuery",
                    actionCode: "Get Entity",        
                    actionName: entitylist.Count.ToString(),
                    details: $"Entity details was fetched.",
                    module:"Entity"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                _logger.LogInformation($"Entity {entitylist.Count} Listed successfully.");
                return new ApiResponseDTO<List<GetEntityDTO>>
                { 
                    IsSuccess = true, 
                    Message = "Success", 
                    Data = entitylist,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                 };
                
       

        }
    }
}