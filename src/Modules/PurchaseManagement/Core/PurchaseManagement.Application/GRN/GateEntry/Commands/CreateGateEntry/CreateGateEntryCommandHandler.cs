using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry
{
    public class CreateGateEntryCommandHandler : IRequestHandler<CreateGateEntryCommand, int>
    {
        private readonly IGateEntryCommandRepository _iGateEntryCommandRepository;
        private readonly IGateEntryQueryRepository _iGateEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateGateEntryCommandHandler(IGateEntryCommandRepository iGateEntryCommandRepository, IMapper mapper, IMediator mediator, IGateEntryQueryRepository iGateEntryQueryRepository)
        {
            _iGateEntryCommandRepository = iGateEntryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _iGateEntryQueryRepository = iGateEntryQueryRepository;
        }

        public async Task<int> Handle(CreateGateEntryCommand request, CancellationToken cancellationToken)
        {

            var gateEntryHeader = _mapper.Map<GateEntryHeader>(request.GateEntryDetails);

            // ✅ Auto-generate GateEntryNo if not set
            if (string.IsNullOrWhiteSpace(gateEntryHeader.GateEntryNo))
            {
                gateEntryHeader.GateEntryNo = await _iGateEntryCommandRepository
                    .GenerateNextCodeAsync();  // Custom method for unique number
                gateEntryHeader.GateEntryDate = DateTime.Now;
            }

            if (!string.IsNullOrWhiteSpace(gateEntryHeader.ImagePath))
            {
                string baseDirectory = await _iGateEntryQueryRepository.GetDocumentDirectoryAsync();
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
                EnsureDirectoryExists(uploadPath);

                string oldFilePath = Path.Combine(uploadPath, gateEntryHeader.ImagePath);
                if (File.Exists(oldFilePath))
                {
                    // New filename format → GateEntryNo.ext
                    string newFileName = $"{gateEntryHeader.GateEntryNo}{Path.GetExtension(oldFilePath)}";
                    string newFilePath = Path.Combine(uploadPath, newFileName);

                    try
                    {
                        File.Move(oldFilePath, newFilePath, overwrite: true);
                        gateEntryHeader.ImagePath = newFileName;


                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"File rename failed for '{gateEntryHeader.ImagePath}' → '{newFileName}': {ex.Message}", ex);
                    }
                }
            }

            var result = await _iGateEntryCommandRepository.CreateAsync(gateEntryHeader);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: gateEntryHeader.GateEntryNo ?? "NULL",
                actionName: gateEntryHeader.GateEntryDate.ToString() ?? "NULL",
                details: $"GateEntry details was created",
                module: "GateEntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("GateEntry Creation Failed.");
        }
         private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }  
    }
}