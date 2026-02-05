using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Entities;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Command.DeletePartyMasterDocument
{
    public class DeletePartyMasterDocumentCommandHandler : IRequestHandler<DeletePartyMasterDocumentCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;   
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IPartyMasterCommandRepository _ipartyMasterCommandRepository;
        private readonly IPartyActivityLogCommandRepository _ipartyActivityLogCommandRepository;
        private readonly IIPAddressService _ipAddressService;


        public DeletePartyMasterDocumentCommandHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IFileUploadService fileUploadService, IPartyMasterCommandRepository partyMasterCommandRepository, IPartyActivityLogCommandRepository partyActivityLogCommandRepository, IIPAddressService ipAddressService)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _fileUploadService = fileUploadService;
            _ipartyMasterCommandRepository = partyMasterCommandRepository;
            _ipartyActivityLogCommandRepository = partyActivityLogCommandRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<bool> Handle(DeletePartyMasterDocumentCommand request, CancellationToken cancellationToken)
        {
            string baseDirectory = await _ipartyMasterQueryRepository.GetDocumentDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new Exception("Base directory not configured.");
            }

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
            string filePath = Path.Combine(uploadPath, request.partydocumentPath ?? string.Empty);

            // ✅ Check if file exists before attempting delete
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            // ✅ First delete file physically
            var result = await _fileUploadService.DeleteFileAsync(filePath);
            if (!result)
            {
                throw new Exception("File deletion failed.");
            }

            // ✅ Delete from DB only if Id and PartyId > 0
            if (request.Id > 0 && request.PartyId > 0)
            {
                var dbResult = await _ipartyMasterCommandRepository.DeleteFileDetailsDocumentAsync(
                    request.Id,
                    request.PartyId,
                    request.FileName ?? string.Empty
                );

                if (!dbResult)
                {
                    throw new Exception("Document entry not found or deletion failed in DB.");
                }
                // ✅ Log
                var log = new PartyActivityLog
                {
                PartyId = request.PartyId,
                TableName = "PartyDocuments",
                ColumnName = "FileName",
                OldValue = request.FileName ?? string.Empty,
                NewValue = "",
                ActionType = "Delete",
                ChangedBy = _ipAddressService.GetUserId(),
                ChangedByName=_ipAddressService.GetUserName(),
                ChangedIp=_ipAddressService.GetSystemIPAddress(),
                ChangedOn = DateTimeOffset.UtcNow
            };

                await _ipartyActivityLogCommandRepository.InsertAsync(log, cancellationToken); 
               
            }

            return true;
        }

    }
}