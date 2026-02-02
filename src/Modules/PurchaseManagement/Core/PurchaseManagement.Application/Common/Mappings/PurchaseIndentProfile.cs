using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Commands.Purchase;
using Contracts.Commands.Workflow;
using Contracts.Dtos.Purchase;
using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class PurchaseIndentProfile : Profile
    {
    public PurchaseIndentProfile()
    {
      CreateMap<CreatePurchaseIndentCommand, IndentHeader>()
       .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
           .ForMember(dest => dest.IndentDetails, opt => opt.MapFrom(src => src.IndentDetails));

      CreateMap<PurchaseIndents.Command.CreatePurchaseIndent.IndentDetailDto, IndentDetail>()
      .ForMember(dest => dest.IsRFQDone, opt => opt.MapFrom(src => false));

      CreateMap<DeletePurchaseIndentCommand, IndentHeader>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

      CreateMap<UpdatePurchaseIndentCommand, IndentHeader>()
        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
        .ForMember(dest => dest.IndentDetails, opt => opt.MapFrom(src => src.IndentDetails));

      CreateMap<IndentDetailUpdateDto, IndentDetail>();

      // CreateMap<IndentHeader, PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>()
      // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0))
      // .ForMember(dest => dest.IndentType, opt => opt.MapFrom(src => src.IndentType.Code))
      // .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Code));

      CreateMap<IndentHeader, IndentByIdDto>()
      .ForMember(dest => dest.IndentTypeName, opt => opt.MapFrom(src => src.IndentType.Code));
      CreateMap<IndentDetail, IndentDetailByIdDto>()
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Code));

      CreateMap<IndentHeader, UpdatePurchaseIndentCommand>()
        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0))
        .ForMember(dest => dest.IndentDetails, opt => opt.MapFrom(src => src.IndentDetails));

      CreateMap<IndentDetail, IndentDetailUpdateDto>();

      CreateMap<IndentHeader, IndentReverseMapDto>()
           .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src))
           .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.IndentDetails));

      CreateMap<IndentHeader, CreateIndentHeaderDto>();
      CreateMap<IndentDetail, CreateIndentDetailDto>();

      CreateMap<IndentHeader, PendingIndentDto>()
      .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0))
      .ForMember(dest => dest.IndentType, opt => opt.MapFrom(src => src.IndentType.Code));

      CreateMap<IndentHeader, PendingIndentByIdDto>()
      .ForMember(dest => dest.IndentTypeName, opt => opt.MapFrom(src => src.IndentType.Code));
      CreateMap<IndentDetail, PendingIndentDetailByIdDto>()
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Code));

      CreateMap<UpdateLineStatusDto, IndentDetail>()
      // .ForMember(dest => dest.ApprovedQuantity, opt => opt.MapFrom(src => src.ApprovedQuantity))
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ModuleLineId))
      .ForMember(dest => dest.Status, opt => opt.Ignore());

      CreateMap<UpdateApprovedRejectedPurchaseCommand, IndentHeader>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ModuleTransactionId))
      .ForMember(dest => dest.IndentDetails, opt => opt.MapFrom(src => src.LineStatus))
      .ForMember(dest => dest.Status, opt => opt.Ignore());

      CreateMap<IndentHeader, PurchaseIndentAutoCompleteQueryDto>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.IndentNumber, opt => opt.MapFrom(src => src.IndentNumber));

       CreateMap<IndentHeader, IndentForPODto>()
      .ForMember(dest => dest.IndentTypeName, opt => opt.MapFrom(src => src.IndentType.Code));
      CreateMap<IndentDetail, IndentDetailsForPODto>();
        }
        
    }
}