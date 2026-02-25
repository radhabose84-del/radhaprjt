#nullable disable
using AutoMapper;
using MediatR;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Domain.Events;
using UserManagement.Application.Common.Interfaces;


namespace UserManagement.Application.Companies.Queries.GetCompanyAutoComplete
{
    public class GetCompanyAutoCompleteQueryHandler : IRequestHandler<GetCompanyAutoCompleteQuery,List<CompanyAutoCompleteDTO>>
    { 
        private readonly ICompanyQueryRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
         public GetCompanyAutoCompleteQueryHandler(ICompanyQueryRepository companyRepository, IMapper mapper, IMediator mediator,IIPAddressService ipAddressService)
         {
             _companyRepository = companyRepository;
             _mapper =mapper;
             _mediator = mediator;
             _ipAddressService = ipAddressService;
         }  
          public async Task<List<CompanyAutoCompleteDTO>> Handle(GetCompanyAutoCompleteQuery request, CancellationToken cancellationToken)
          {
             var groupcode = _ipAddressService.GetGroupcode();

            if(groupcode == "SUPER_ADMIN" || groupcode == "ADMIN")
                {
                    var Adminresult = await _companyRepository.GetCompany_SuperAdmin(request.SearchPattern);
                    var Admincompany = _mapper.Map<List<CompanyAutoCompleteDTO>>(Adminresult);

                    return Admincompany; 
                }
               
            var userId = _ipAddressService.GetUserId();
              var result = await _companyRepository.GetCompany(userId,request.SearchPattern);
              var company = _mapper.Map<List<CompanyAutoCompleteDTO>>(result);
              //Domain Event
                 var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "GetCompanyAutoComplete",
                     actionCode: "",
                     actionName: "",
                     details: $"Company details was fetched.",
                     module:"Company"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
            return company;            

         } 
    }
}