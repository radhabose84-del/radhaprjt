using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ICompanySettings;
using Core.Application.CompanySettings.Queries.GetCompanySettings;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.CompanySettings.Queries.GetCompanySettingsById
{
    public class GetCompanySettingByIdQueryHandler : IRequestHandler<GetCompanySettingByIdQuery,ApiResponseDTO<CompanySettingsDTO>>
    {
        private readonly ICompanyQuerySettings _ICompanyQuerySettings;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetCompanySettingByIdQueryHandler(ICompanyQuerySettings companyQuerySettings, IMapper mapper, IMediator mediator)
        {
            _ICompanyQuerySettings=companyQuerySettings;
            _mapper=mapper;
            _mediator=mediator;
        }


        public async Task<ApiResponseDTO<CompanySettingsDTO>> Handle(GetCompanySettingByIdQuery request, CancellationToken cancellationToken)
        {
             var result = await _ICompanyQuerySettings.GetAsync();
             if (result == null)
            {
                return new ApiResponseDTO<CompanySettingsDTO> { IsSuccess = false, Message = "Company Setting not found" };
            }
        var companySetting = _mapper.Map<CompanySettingsDTO>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"Company Settings details {companySetting.Id} was fetched.",
                    module:"Company Setting"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return new ApiResponseDTO<CompanySettingsDTO> { IsSuccess = true, Message = "Success", Data = companySetting };
        }
    }
}