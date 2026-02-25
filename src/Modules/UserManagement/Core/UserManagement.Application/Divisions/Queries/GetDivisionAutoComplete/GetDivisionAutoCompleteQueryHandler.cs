#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces;
using MediatR;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Events;



namespace UserManagement.Application.Divisions.Queries.GetDivisionAutoComplete
{
    public class GetDivisionAutoCompleteQueryHandler : IRequestHandler<GetDivisionAutoCompleteQuery,List<DivisionAutoCompleteDTO>>
    {
        private readonly IDivisionQueryRepository _divisionRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
         public GetDivisionAutoCompleteQueryHandler(IDivisionQueryRepository divisionRepository, IMapper mapper, IMediator mediator,IIPAddressService ipAddressService)
         {
            _divisionRepository =divisionRepository;
            _mapper =mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
         }  
          public async Task<List<DivisionAutoCompleteDTO>> Handle(GetDivisionAutoCompleteQuery request, CancellationToken cancellationToken)
          {
             var groupcode = _ipAddressService.GetGroupcode();

            if(groupcode == "SUPER_ADMIN" || groupcode == "ADMIN")
                {
                    var Adminresult = await _divisionRepository.GetDivision_SuperAdmin(request.SearchPattern);
                    var Admindivision = _mapper.Map<List<DivisionAutoCompleteDTO>>(Adminresult);

                    return Admindivision; 
                }

            //     var companies = JsonSerializer.Deserialize<List<UserCompanyDTO>>(request.Companies) ?? new List<UserCompanyDTO>();
            //  var companylist =   _mapper.Map<List<UserCompany>>(companies);
             
            var result = await _divisionRepository.GetDivision(request.SearchPattern);
            var division = _mapper.Map<List<DivisionAutoCompleteDTO>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"Division details was fetched.",
                    module:"Division"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return division;            
         } 
    }
}