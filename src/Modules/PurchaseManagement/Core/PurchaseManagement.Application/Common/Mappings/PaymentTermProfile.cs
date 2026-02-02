using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class PaymentTermProfile : Profile
    {

        public PaymentTermProfile()
        {
            CreateMap<PurchaseManagement.Domain.Entities.PaymentTermMaster, PaymentTermMasterDto>();



            CreateMap<CreatePaymentTermMasterCommand, PurchaseManagement.Domain.Entities.PaymentTermMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
               .ForMember(d => d.Installments, opt => opt.MapFrom(s => s.Installments ?? new List<PaymentTermInstallmentDto>()));

            CreateMap<PaymentTermInstallmentDto, PurchaseManagement.Domain.Entities.PaymentTermInstallment>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.PaymentTermId, opt => opt.Ignore());


            CreateMap<UpdatePaymentTermMasterCommand, PurchaseManagement.Domain.Entities.PaymentTermMaster>()
              .ForMember(dest => dest.IsActive,
                   opt => opt.MapFrom(src => src.IsActive ? Status.Active : Status.Inactive))
                .ForMember(d => d.BalancePercent, opt => opt.Ignore())
                .ForMember(d => d.BaselineType,   opt => opt.Ignore())
                .ForMember(d => d.Installments,   opt => opt.Ignore());   

            // (Optional) Entity -> DTO if you ever return via EF instead of Dapper
            CreateMap<PurchaseManagement.Domain.Entities.PaymentTermMaster, PaymentTermMasterDto>();
            CreateMap<PaymentTermInstallment, PaymentTermInstallmentDto>();

        }
    }
}