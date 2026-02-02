using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IParty;
using Contracts.Interfaces.External.IUser;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid
{
    public class GetSESCreateSourceQueryHandler : IRequestHandler<GetSESCreateSourceQuery, SesFromScheduleRawDto?>
    {
        
          private readonly  IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
          private readonly IMapper _mapper;
          private readonly IMediator _mediator;
          private readonly IUnitGrpcClient _unitGrpcClient;
          private readonly IPartyGrpcClient _partyGrpcClient;

          public GetSESCreateSourceQueryHandler( IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator, IUnitGrpcClient unitGrpcClient, IPartyGrpcClient partyGrpcClient)
          {
              _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
              _mapper = mapper;
              _mediator = mediator;
              _unitGrpcClient = unitGrpcClient;
              _partyGrpcClient = partyGrpcClient;
          }
           public async Task<SesFromScheduleRawDto?> Handle(
            GetSESCreateSourceQuery request,
            CancellationToken cancellationToken)
        {
            if (request.PurchaseOrderId <= 0) throw new ExceptionRules("Valid PurchaseOrderId is required.");
            if (request.ScheduleNo      <= 0) throw new ExceptionRules("Valid ScheduleNo is required.");
            if (request.ServiceItemId   <= 0) throw new ExceptionRules("Valid ServiceItemId is required.");

            // repo should return the RAW dto now
            var raw = await _servicePurchaseOrderQueryRepository.GetSesCreateSourceAsync(
                request.PurchaseOrderId, request.ScheduleNo, request.ServiceItemId, cancellationToken);

            if (raw is null)
                throw new ExceptionRules(
                    $"No schedule found for PO {request.PurchaseOrderId}, ScheduleNo {request.ScheduleNo}, ServiceItemId {request.ServiceItemId}.");

            return raw; // ✅ return SesFromScheduleRawDto directly
        }

                // public async Task<CreateServiceSheetDto?> Handle(      GetSESCreateSourceQuery request,         CancellationToken cancellationToken)
                // {
                //     if (request.PurchaseOrderId <= 0)
                //         throw new ExceptionRules("Valid PurchaseOrderId is required.");

                //     if (request.ScheduleNo <= 0)
                //         throw new ExceptionRules("Valid ScheduleNo is required.");

                //     if (request.ServiceItemId <= 0)
                //         throw new ExceptionRules("Valid ServiceItemId is required.");

                //     var raw = await _servicePurchaseOrderQueryRepository.GetSesCreateSourceAsync(
                //         request.PurchaseOrderId,
                //         request.ScheduleNo,
                //         request.ServiceItemId,
                //         cancellationToken);

                //     if (raw is null)
                //         throw new ExceptionRules(
                //             $"No schedule found for PO {request.PurchaseOrderId}, ScheduleNo {request.ScheduleNo}, ServiceItemId {request.ServiceItemId}.");

                //     // 🔁 AutoMapper takes over mapping raw → CreateServiceSheetDto
                //     var dto = _mapper.Map<CreateServiceSheetDto>(raw);
                //     return dto;
                // }
    }
}