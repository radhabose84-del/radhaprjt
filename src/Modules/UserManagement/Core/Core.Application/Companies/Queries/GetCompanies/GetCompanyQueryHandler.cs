using AutoMapper;
using MediatR;
using Core.Application.Common.Interfaces.ICompany;
using Core.Application.Common.HttpResponse;
using Core.Domain.Events;
using Core.Application.Common.Interfaces;

namespace Core.Application.Companies.Queries.GetCompanies
{
    public class GetCompanyQueryHandler : IRequestHandler<GetCompanyQuery,ApiResponseDTO<List<GetCompanyDTO>>>
    {  
        private readonly ICompanyQueryRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetCompanyQueryHandler(ICompanyQueryRepository companyRepository, IMapper mapper, IMediator mediator)
        {
             _companyRepository = companyRepository;
             _mapper =mapper;
             _mediator = mediator;
        } 
        public async Task<ApiResponseDTO<List<GetCompanyDTO>>> Handle(GetCompanyQuery requst, CancellationToken cancellationToken)
        {
            
            var (companies, totalCount) = await _companyRepository.GetAllCompaniesAsync(requst.PageNumber, requst.PageSize, requst.SearchTerm);             
            
            var companylist = _mapper.Map<List<GetCompanyDTO>>(companies);
            
             //Domain Event
                 var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "GetAll",
                     actionCode: "",
                     actionName: "",
                     details: $"Company details was fetched.",
                     module:"Company"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
            var response =companylist.ToList();
            return new ApiResponseDTO<List<GetCompanyDTO>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = response,
                TotalCount = totalCount,
                PageNumber = requst.PageNumber,
                PageSize = requst.PageSize
            };
        }
    }
}