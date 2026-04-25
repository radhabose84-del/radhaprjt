using Contracts.Commands.Finance;
using Contracts.Common;
using Contracts.Dtos.Finance;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Sales;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceFromSales
{
    public class CreateEInvoiceFromSalesCommandHandler
        : IRequestHandler<CreateEInvoiceFromSalesCommand, ApiResponseDTO<EInvoiceCreationResultDto>>
    {
        private readonly ISalesInvoiceLookup _salesInvoiceLookup;
        private readonly IEInvoiceHeaderCommandRepository _eInvoiceCommandRepo;
        private readonly IEInvoiceHeaderQueryRepository _eInvoiceQueryRepo;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateEInvoiceFromSalesCommandHandler> _logger;

        public CreateEInvoiceFromSalesCommandHandler(
            ISalesInvoiceLookup salesInvoiceLookup,
            IEInvoiceHeaderCommandRepository eInvoiceCommandRepo,
            IEInvoiceHeaderQueryRepository eInvoiceQueryRepo,
            IMediator mediator,
            ILogger<CreateEInvoiceFromSalesCommandHandler> logger)
        {
            _salesInvoiceLookup = salesInvoiceLookup;
            _eInvoiceCommandRepo = eInvoiceCommandRepo;
            _eInvoiceQueryRepo = eInvoiceQueryRepo;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<EInvoiceCreationResultDto>> Handle(
            CreateEInvoiceFromSalesCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Fetch Sales Invoice data via shared lookup (by ID or by InvoiceNumber+UnitId)
            SalesInvoiceForEInvoiceDto? invoiceData;

            if (request.InvoiceId.HasValue && request.InvoiceId.Value > 0)
            {
                invoiceData = await _salesInvoiceLookup.GetInvoiceForEInvoiceByIdAsync(request.InvoiceId.Value);
            }
            else
            {
                invoiceData = await _salesInvoiceLookup.GetInvoiceForEInvoiceAsync(
                    request.InvoiceNumber!, request.UnitId);
            }

            if (invoiceData == null)
            {
                return new ApiResponseDTO<EInvoiceCreationResultDto>
                {
                    IsSuccess = false,
                    Message = request.InvoiceId.HasValue
                        ? $"Sales Invoice with Id {request.InvoiceId} not found."
                        : $"Sales Invoice '{request.InvoiceNumber}' not found for UnitId {request.UnitId}."
                };
            }

            // 1b. Idempotent check
            if (!string.IsNullOrEmpty(invoiceData.InvoiceNo))
            {
                var alreadyExists = await _eInvoiceQueryRepo.ExistsByInvoiceNoAsync(
                    invoiceData.InvoiceNo, cancellationToken);

                if (alreadyExists)
                {
                    // EInvoice with IRN already exists — try EWaybill only if requested
                    _logger.LogInformation(
                        "EInvoice with IRN already exists for Invoice '{InvoiceNo}', skipping creation",
                        invoiceData.InvoiceNo);

                    if (!request.IsEwaybillCreate)
                    {
                        return new ApiResponseDTO<EInvoiceCreationResultDto>
                        {
                            IsSuccess = true,
                            Message = $"EInvoice already exists for Invoice '{invoiceData.InvoiceNo}'."
                        };
                    }

                    // Try to generate EWaybill for the existing EInvoice
                    var existingId = await _eInvoiceQueryRepo.GetIdByInvoiceNoAsync(
                        invoiceData.InvoiceNo, cancellationToken);

                    if (existingId == null)
                    {
                        return new ApiResponseDTO<EInvoiceCreationResultDto>
                        {
                            IsSuccess = false,
                            Message = $"EInvoice found for Invoice '{invoiceData.InvoiceNo}' but could not retrieve its Id."
                        };
                    }

                    var ewbRetryCommand = new GenerateEwbCommand
                    {
                        EInvoiceHeaderId = existingId.Value,
                        TransporterId = invoiceData.TransporterGstin,
                        TransporterName = invoiceData.TransporterName,
                        TransMode = invoiceData.TransportModeCode,
                        VehicleNo = invoiceData.VehicleNo,
                        VehicleType = "R",
                        Distance = 1
                    };

                    var ewbRetryResult = await _mediator.Send(ewbRetryCommand, cancellationToken);

                    if (!ewbRetryResult.IsSuccess)
                    {
                        _logger.LogWarning(
                            "EWaybill generation failed for existing EInvoice {EInvoiceId}: {Error}",
                            existingId.Value, ewbRetryResult.Message);

                        return new ApiResponseDTO<EInvoiceCreationResultDto>
                        {
                            IsSuccess = false,
                            Message = $"EInvoice already exists with IRN but EWaybill not generated: {ewbRetryResult.Message}",
                            Data = new EInvoiceCreationResultDto
                            {
                                EInvoiceHeaderId = existingId.Value,
                                ErrorMessage = ewbRetryResult.Message
                            }
                        };
                    }

                    return new ApiResponseDTO<EInvoiceCreationResultDto>
                    {
                        IsSuccess = true,
                        Message = "EWaybill generated successfully for existing EInvoice.",
                        Data = new EInvoiceCreationResultDto
                        {
                            EInvoiceHeaderId = existingId.Value,
                            EwbNo = ewbRetryResult.Data?.EwbNo,
                            EwbDate = ewbRetryResult.Data?.EwbDate
                        }
                    };
                }

                // No EInvoice with IRN — clean up any incomplete orphan (no IRN) before creating fresh
                await _eInvoiceCommandRepo.HardDeleteIncompleteByInvoiceNoAsync(
                    invoiceData.InvoiceNo, cancellationToken);
            }

            // 2. Create EInvoice Header via existing command
            var createCommand = MapToCreateEInvoiceCommand(invoiceData);
            var createResult = await _mediator.Send(createCommand, cancellationToken);

            if (!createResult.IsSuccess || createResult.Data <= 0)
            {
                return new ApiResponseDTO<EInvoiceCreationResultDto>
                {
                    IsSuccess = false,
                    Message = $"EInvoice creation failed: {createResult.Message}"
                };
            }

            var eInvoiceHeaderId = createResult.Data;

            _logger.LogInformation(
                "EInvoice Header {EInvoiceId} created from Sales Invoice {InvoiceNo}",
                eInvoiceHeaderId, invoiceData.InvoiceNo);

            // 3. Generate IRN (with optional transport details for Case 1: IRN + EWB together)
            var irnCommand = new GenerateIrnCommand
            {
                EInvoiceHeaderId = eInvoiceHeaderId,
                TransId = invoiceData.TransporterGstin,
                TransName = invoiceData.TransporterName,
                TransMode = invoiceData.TransportModeCode,
                VehNo = invoiceData.VehicleNo
            };

            var irnResult = await _mediator.Send(irnCommand, cancellationToken);

            if (!irnResult.IsSuccess)
            {
                // Clean up orphaned EInvoice header + details (no IRN was generated, no external reference exists)
                try
                {
                    await _eInvoiceCommandRepo.HardDeleteWithDetailsAsync(eInvoiceHeaderId, cancellationToken);
                    _logger.LogInformation(
                        "Cleaned up EInvoice Header {EInvoiceId} and details after IRN failure",
                        eInvoiceHeaderId);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx,
                        "Failed to clean up EInvoice Header {EInvoiceId} after IRN failure",
                        eInvoiceHeaderId);
                }

                return new ApiResponseDTO<EInvoiceCreationResultDto>
                {
                    IsSuccess = false,
                    Message = $"IRN generation failed: {irnResult.Message}",
                    Data = new EInvoiceCreationResultDto
                    {
                        EInvoiceHeaderId = eInvoiceHeaderId,
                        ErrorCode = irnResult.Data?.ErrorCode,
                        ErrorMessage = irnResult.Data?.ErrorMessage
                    }
                };
            }

            _logger.LogInformation(
                "IRN generated for EInvoice {EInvoiceId}, IRN: {Irn}",
                eInvoiceHeaderId, irnResult.Data?.Irn);

            var resultDto = new EInvoiceCreationResultDto
            {
                EInvoiceHeaderId = eInvoiceHeaderId,
                Irn = irnResult.Data?.Irn,
                AckNo = irnResult.Data?.AckNo,
                AckDate = irnResult.Data?.AckDate,
                EwbNo = irnResult.Data?.EwbNo,
                EwbDate = irnResult.Data?.EwbDate
            };

            // 4. Generate E-Waybill separately if requested and not already done via Case 1
            bool ewbAlreadyGenerated = irnResult.Data?.EwbNo.HasValue == true;

            if (request.IsEwaybillCreate && !ewbAlreadyGenerated)
            {
                var ewbCommand = new GenerateEwbCommand
                {
                    EInvoiceHeaderId = eInvoiceHeaderId,
                    TransporterId = invoiceData.TransporterGstin,
                    TransporterName = invoiceData.TransporterName,
                    TransMode = invoiceData.TransportModeCode,
                    VehicleNo = invoiceData.VehicleNo,
                    VehicleType = "R",
                    Distance = 1
                };

                var ewbResult = await _mediator.Send(ewbCommand, cancellationToken);

                if (ewbResult.IsSuccess)
                {
                    resultDto.EwbNo = ewbResult.Data?.EwbNo;
                    resultDto.EwbDate = ewbResult.Data?.EwbDate;

                    _logger.LogInformation(
                        "E-Waybill generated for EInvoice {EInvoiceId}, EwbNo: {EwbNo}",
                        eInvoiceHeaderId, ewbResult.Data?.EwbNo);
                }
                else
                {
                    _logger.LogWarning(
                        "E-Waybill generation failed for EInvoice {EInvoiceId}: {Error}",
                        eInvoiceHeaderId, ewbResult.Message);

                    // IRN was generated — return failure so caller reverts statuses to Pending.
                    // Next approval attempt will detect existing IRN and retry EWaybill only.
                    resultDto.ErrorMessage = ewbResult.Message;
                    return new ApiResponseDTO<EInvoiceCreationResultDto>
                    {
                        IsSuccess = false,
                        Message = $"EInvoice generated with IRN '{resultDto.Irn}' but EWaybill not generated: {ewbResult.Message}",
                        Data = resultDto
                    };
                }
            }

            // 5. Audit log
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "CreateEInvoiceFromSales",
                actionCode: "EINVOICE_FROM_SALES",
                actionName: invoiceData.InvoiceNo ?? string.Empty,
                details: $"EInvoice created from Sales Invoice '{invoiceData.InvoiceNo}', EInvoiceHeaderId={eInvoiceHeaderId}, IRN={resultDto.Irn}",
                module: "EInvoiceHeader");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<EInvoiceCreationResultDto>
            {
                IsSuccess = true,
                Message = "EInvoice created and IRN generated successfully from Sales Invoice.",
                Data = resultDto
            };
        }

        private static CreateEInvoiceHeaderCommand MapToCreateEInvoiceCommand(SalesInvoiceForEInvoiceDto invoice)
        {
            return new CreateEInvoiceHeaderCommand
            {
                UnitId = invoice.UnitId,
                DocType = "INV",
                SupplyType = "B2B",
                InvoiceNo = invoice.InvoiceNo,
                InvoiceDate = invoice.InvoiceDate,
                PlaceOfSupply = invoice.PlaceOfSupply,
                PartyId = invoice.PartyId,
                GstNo = invoice.GstNo,
                ReverseCharge = invoice.ReverseCharge,
                CGST = invoice.CGST,
                SGST = invoice.SGST,
                IGST = invoice.IGST,
                TCS = invoice.TCS,
                Discount = invoice.TotalDiscount,
                OtherCharges = invoice.OtherCharges,
                RoundOff = invoice.RoundOff,
                InvoiceAmount = invoice.InvoiceAmount,
                Remarks = invoice.Remarks,
                IrnStatus = "Pending",
                Details = invoice.Details.Select(d =>
                {
                    var grossAmount = d.TaxableAmount + d.DiscountValue;
                    var unitPrice = d.NetWeight != 0 ? Math.Round(grossAmount / d.NetWeight, 6) : 0;
                    return new CreateEInvoiceDetailDto
                    {
                        ItemSno = d.ItemSno,
                        ItemId = d.ItemId,
                        ItemName = d.ItemName,
                        HsnNo = d.HsnCode,
                        NoOfBags = d.NoOfBags,
                        Qty = d.NetWeight,
                        UnitPrice = unitPrice,
                        Rate = d.RatePerKg,
                        Discount = d.DiscountValue,
                        GrossAmount = grossAmount,
                        TaxableAmount = d.TaxableAmount,
                        GstPercentage = d.GstPercentage,
                        CGST = d.CGST,
                        SGST = d.SGST,
                        IGST = d.IGST,
                        TotalAmount = d.TotalAmount,
                        PackTypeId = d.PackTypeId,
                        UOM = d.UOMName
                    };
                }).ToList()
            };
        }
    }
}
