using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class TnCTemplateMasterProfile  : Profile
    {
        public TnCTemplateMasterProfile()
        {


            CreateMap<PurchaseManagement.Domain.Entities.TnCTemplateMaster, TncTemplateMasterDto>();

            CreateMap<CreateTnCTemplateMasterCommand, PurchaseManagement.Domain.Entities.TnCTemplateMaster>()
               .ForMember(d => d.TemplateCode, opt => opt.Ignore())
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
               .ForMember(d => d.Applicabilities, opt => opt.MapFrom(s => s.Applicabilities ?? new List<TncApplicabilityDto>()));
               

                // Update – only safe fields
            CreateMap<UpdateTnCTemplateMasterCommand, PurchaseManagement.Domain.Entities.TnCTemplateMaster>()
                .ForMember(d => d.TemplateCode,    opt => opt.Ignore())
                .ForMember(d => d.IsDeleted,       opt => opt.Ignore())
                .ForMember(d => d.Applicabilities, opt => opt.Ignore()) // handled in repo
                .ForMember(d => d.IsActive,        opt => opt.MapFrom(s =>
                    s.IsActive == 1 ? Status.Active : Status.Inactive));

            //    CreateMap<UpdateTnCTemplateMasterCommand, Core.Domain.Entities.TnCTemplateMaster>()
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));  

            CreateMap<TncApplicabilityDto, PurchaseManagement.Domain.Entities.TnCTemplateApplicability>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.TnCTemplateMasterId, opt => opt.Ignore());
            
        }
    }
}