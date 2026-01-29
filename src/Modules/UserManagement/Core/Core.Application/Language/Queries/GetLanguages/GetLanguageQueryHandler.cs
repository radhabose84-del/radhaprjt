using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ILanguage;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.Language.Queries.GetLanguages
{
    public class GetLanguageQueryHandler : IRequestHandler<GetLanguageQuery,ApiResponseDTO<List<LanguageDTO>>>
    {
        private readonly ILanguageCommand _languageCommand;
        private readonly ILanguageQuery _languageQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetLanguageQueryHandler(IMapper mapper, ILanguageCommand languageCommand, IMediator mediator, ILanguageQuery languageQuery)
        {
            _mapper = mapper;
            _languageCommand = languageCommand;
            _mediator = mediator;
            _languageQuery = languageQuery;
        }


        public async Task<ApiResponseDTO<List<LanguageDTO>>> Handle(GetLanguageQuery request, CancellationToken cancellationToken)
        {
            var (languages, totalCount) = await _languageQuery.GetAllLanguageAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var languagesList = _mapper.Map<List<LanguageDTO>>(languages);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetLanguage",
                    actionCode: "",        
                    actionName: "",
                    details: $"Language details was fetched.",
                    module:"Language"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<LanguageDTO>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = languagesList ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}