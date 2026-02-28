using MediatR;
using Microsoft.AspNetCore.Http;
using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.CustomerVisit.Commands.UploadCustomerVisitImage
{
    public class UploadCustomerVisitImageCommand : IRequest<CustomerVisitImageDto>
    {
        public IFormFile? File { get; set; }
    }
}
