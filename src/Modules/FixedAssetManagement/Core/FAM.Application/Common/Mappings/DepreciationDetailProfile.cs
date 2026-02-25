using AutoMapper;
using FAM.Application.DepreciationDetail.Commands.CreateDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.DeleteDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.UpdateDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Entities;

namespace FAM.Application.Common.Mappings
{
    public class DepreciationDetailProfile : Profile
    {
        public DepreciationDetailProfile()
        { 
            CreateMap<DeleteDepreciationDetailCommand, DepreciationDetails>();                                  
            CreateMap<CreateDepreciationDetailCommand, DepreciationDetails>(); 
            CreateMap<UpdateDepreciationDetailCommand,DepreciationDetails>();               
            CreateMap<DepreciationDetails, DepreciationDto>();     
            CreateMap<DepreciationDetails, DepreciationCalculationDto>();   
            CreateMap<DepreciationDetails, DepreciationAbstractDto>();                      
        }
             
    }
}