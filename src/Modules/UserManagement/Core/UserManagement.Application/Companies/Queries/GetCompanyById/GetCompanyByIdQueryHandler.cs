using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.Interfaces;
using MediatR;
using System.Text;
using UserManagement.Application.Companies.Queries.GetCompanies;
using System.Data;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Domain.Events;

namespace UserManagement.Application.Companies.Queries.GetCompanyById
{
    public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery,GetByIdDTO>
    {
          private readonly ICompanyQueryRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
         public GetCompanyByIdQueryHandler(ICompanyQueryRepository companyRepository, IMapper mapper, IMediator mediator)
        {
              _companyRepository = companyRepository;
             _mapper =mapper;
             _mediator = mediator;
        } 
        public async Task<GetByIdDTO> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
           
            var result = await _companyRepository.GetByIdAsync(request.CompanyId);
            string logoBase64 = null;
             if (!string.IsNullOrEmpty(result.Logo) && File.Exists(result.Logo))
             {
                 byte[] imageBytes = await File.ReadAllBytesAsync(result.Logo);
                 logoBase64 = Convert.ToBase64String(imageBytes);
             }
            var company = _mapper.Map<GetByIdDTO>(result);
            company.LogoBase64 = logoBase64;
             //Domain Event
                 var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "GetCompanyById",
                     actionCode: "",
                     actionName: "",
                     details: $"Company details {company.Id} was fetched.",
                     module:"Company"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
            return company;
        }
    }
}