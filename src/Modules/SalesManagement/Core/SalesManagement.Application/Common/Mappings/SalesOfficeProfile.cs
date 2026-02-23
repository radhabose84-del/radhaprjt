#nullable disable
using AutoMapper;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesOfficeProfile : Profile
    {
        public SalesOfficeProfile()
        {
            CreateMap<CreateSalesOfficeCommand, Domain.Entities.SalesOffice>();
            CreateMap<UpdateSalesOfficeCommand, Domain.Entities.SalesOffice>();
        }
    }
}
