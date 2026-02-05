// using System.Text.Json;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using Contracts.Interfaces.External.IParty;
// using PartyManagement.Application.Common.Exceptions;
// using PartyManagement.Application.Common.Interfaces;
// using PartyManagement.Application.Common.Interfaces.IPartyMaster;
// using PartyManagement.Domain.Common;
// using PartyManagement.Domain.Entities;
// using PartyManagement.Domain.Events;
// using MediatR;

// namespace PartyManagement.Application.PartyMaster.Command.CreatePartyMaster
// {
//     public class CreatePartyMasterCommandHandler : IRequestHandler<CreatePartyMasterCommand, int>
//     {
//         private readonly IPartyMasterCommandRepository _partyMasterCommandRepository;
//         private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
//         private readonly IPartyActivityLogCommandRepository _ipartyActivityLogCommandRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator; 
//         private readonly ILocationGrpcClient _locationGrpcClient;  // ✅ add this
//          private readonly IEventPublisher _eventPublisher;

//         public CreatePartyMasterCommandHandler(IPartyMasterCommandRepository partyMasterCommandRepository, IMapper mapper, IMediator mediator, IPartyMasterQueryRepository ipartyMasterQueryRepository, IPartyActivityLogCommandRepository ipartyActivityLogCommandRepository, ILocationGrpcClient locationGrpcClient, IEventPublisher eventPublisher)
//         {
//             _partyMasterCommandRepository = partyMasterCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
//             _ipartyActivityLogCommandRepository = ipartyActivityLogCommandRepository;
//             _locationGrpcClient = locationGrpcClient;
//             _eventPublisher = eventPublisher;
//         }

//         // public async Task<int> Handle(CreatePartyMasterCommand request, CancellationToken cancellationToken)
//         // {
//         //     var dto = request.PartyMaster;

//         //     // ------------------- Clean Address placeholders -------------------
//         //     if (dto.PartyAddresses?.Any() == true)
//         //     {
//         //         dto.PartyAddresses = dto.PartyAddresses
//         //             .Where(c =>
//         //                 !(
//         //                     string.Equals(c.AddressType?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//         //                     string.Equals(c.AddressLine1?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//         //                     string.Equals(c.City?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//         //                     string.Equals(c.State?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//         //                     string.Equals(c.PostalCode?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//         //                     string.Equals(c.Country?.Trim(), "string", StringComparison.OrdinalIgnoreCase)
//         //                 )
//         //             )
//         //             .ToList();

//         //         if (!dto.PartyAddresses.Any())
//         //             dto.PartyAddresses = null;
//         //     }



//         //     // ------------------- Clean Contacts -------------------
//         //     if (dto.PartyContacts != null)
//         //     {
//         //         dto.PartyContacts = dto.PartyContacts
//         //             .Where(c =>
//         //                 !string.Equals(c.FirstName, "string", StringComparison.OrdinalIgnoreCase) &&
//         //                 !string.Equals(c.MobileNo, "string", StringComparison.OrdinalIgnoreCase) &&
//         //                 !string.Equals(c.ContactBy, "string", StringComparison.OrdinalIgnoreCase)
//         //             )
//         //             .ToList();

//         //         if (!dto.PartyContacts.Any())
//         //             dto.PartyContacts = null;
//         //     }

//         //     // ------------------- Clean Banks -------------------
//         //     if (dto.PartyBanks != null)
//         //     {
//         //         dto.PartyBanks = dto.PartyBanks
//         //             .Where(b =>
//         //                 !string.Equals(b.BankName, "string", StringComparison.OrdinalIgnoreCase) &&
//         //                 !string.Equals(b.BankAccountNumber, "string", StringComparison.OrdinalIgnoreCase) &&
//         //                 !string.Equals(b.BankBranch, "string", StringComparison.OrdinalIgnoreCase) &&
//         //                 !string.Equals(b.IFSCCode, "string", StringComparison.OrdinalIgnoreCase) &&
//         //                 b.AccountTypeId != 0
//         //             )
//         //             .ToList();

//         //         if (!dto.PartyBanks.Any())
//         //             dto.PartyBanks = null;
//         //     }

//         //     // ------------------- PartyCode Generation -------------------
//         //     var nextPartyCode = await _partyMasterCommandRepository.GetNextPartyCodeAsync();

//         //     // ------------------- Rename Uploaded Docs -------------------
//         //     if (dto.PartyDocuments != null && dto.PartyDocuments.Any())
//         //     {
//         //         dto.PartyDocuments = dto.PartyDocuments
//         //             .Where(d => d.DocumentId != 0 &&
//         //                         !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//         //             .ToList();

//         //         if (dto.PartyDocuments.Any())
//         //         {
//         //             string baseDirectory = await _ipartyMasterQueryRepository.GetBaseDirectoryAsync();
//         //             string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
//         //             EnsureDirectoryExists(uploadPath);

//         //             foreach (var doc in dto.PartyDocuments)
//         //             {
//         //                 if (string.IsNullOrWhiteSpace(doc.FileName))
//         //                     continue;

//         //                 string oldFilePath = Path.Combine(uploadPath, doc.FileName);
//         //                 if (File.Exists(oldFilePath))
//         //                 {
//         //                     string newFileName = $"{nextPartyCode}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
//         //                     string newFilePath = Path.Combine(uploadPath, newFileName);

//         //                     try
//         //                     {
//         //                         File.Move(oldFilePath, newFilePath, overwrite: true);
//         //                         doc.FileName = newFileName;
//         //                         doc.UploadedDate = DateTimeOffset.UtcNow;
//         //                     }
//         //                     catch (Exception ex)
//         //                     {
//         //                         throw new InvalidOperationException(
//         //                             $"File upload failed while renaming '{doc.FileName}' to '{newFileName}': {ex.Message}", ex);
//         //                     }
//         //                 }
//         //             }
//         //         }
//         //         else
//         //         {
//         //             dto.PartyDocuments = null;
//         //         }
//         //     }

//         //     // ------------------- Map to Entity -------------------
//         //     var partyMasterEntity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);

//         //     // Assign generated PartyCode
//         //     partyMasterEntity.PartyCode = nextPartyCode;


//         //      // ------------------- Call gRPC Location Service -------------------
//         //   if (partyMasterEntity.PartyAddressTypes != null && partyMasterEntity.PartyAddressTypes.Any())
//         //     {
//         //         // Assuming same order: dto.PartyAddresses[i] → entity.PartyAddressTypes[i]
//         //         for (int i = 0; i < dto.PartyAddresses.Count; i++)
//         //         {
//         //             var addressDto = dto.PartyAddresses[i];
//         //             var addressEntity = partyMasterEntity.PartyAddressTypes.ElementAt(i);

//         //             if (!string.IsNullOrWhiteSpace(addressDto.City) &&
//         //                 !string.IsNullOrWhiteSpace(addressDto.State) &&
//         //                 !string.IsNullOrWhiteSpace(addressDto.Country))
//         //             {
//         //                 var location = await _locationGrpcClient.GetOrCreateLocationAsync(
//         //                     addressDto.City,
//         //                     addressDto.State,
//         //                     addressDto.Country
//         //                 );

//         //                 if (location == null)
//         //                     throw new Exception("Location could not be resolved via gRPC.");

//         //                 // Assign IDs directly to the matching entity address
//         //                 addressEntity.CityId = location.CityId;
//         //                 addressEntity.StateId = location.StateId;
//         //                 addressEntity.CountryId = location.CountryId;
//         //             }
//         //         }
//         //     }


//         //     // ------------------- Save to DB -------------------
//         //     var result = await _partyMasterCommandRepository.CreateAsync(partyMasterEntity);
//         //     if (result > 0)
//         //     {
//         //         var entity = await _ipartyMasterQueryRepository.GetByIdPartyMasterAsync(result);
//         //         var reverseMap = _mapper.Map<CreatePartyMasterReverseDto>(entity);
//         //         string serializedPayload = JsonSerializer.Serialize(reverseMap);


//         //         // ------------------- Audit Logs -------------------
//         //         var domainEvent = new AuditLogsDomainEvent(
//         //         actionDetail: "Create",
//         //         actionCode: partyMasterEntity.PartyCode ?? "NULL",
//         //         actionName: partyMasterEntity.PartyName ?? "NULL",
//         //         details: "PartyMaster details created",
//         //         module: "PartyMaster"
//         //     );

//         //         await _mediator.Publish(domainEvent, cancellationToken);
//         //     }

//         //     if (result > 0)
//         //         {
//         //             var log = new PartyActivityLog
//         //             {
//         //                 PartyId = partyMasterEntity.Id,
//         //                 TableName = "PartyMaster",
//         //                 ColumnName = "PartyName",
//         //                 OldValue = "",
//         //                 NewValue = partyMasterEntity.PartyName ?? string.Empty,
//         //                 ActionType = "Insert",
//         //                 ChangedBy = partyMasterEntity.CreatedBy,
//         //                 ChangedByName = partyMasterEntity.CreatedByName ?? string.Empty,
//         //                 ChangedIp = partyMasterEntity.CreatedIP ?? string.Empty,
//         //                 ChangedOn = DateTimeOffset.UtcNow
//         //             };

//         //             await _ipartyActivityLogCommandRepository.InsertAsync(log, cancellationToken);
//         //         }

//         //     if (result > 0)
//         //     {
//         //         var correlationId = Guid.NewGuid();
//         //         var @event = new TransactionCreatedEvent
//         //         {
//         //             CorrelationId = correlationId,
//         //             ModuleTypeName = MiscEnumEntity.PartyDocumentImage.PartyMaster.ToString(),
//         //             ModuleTransactionId = result,
//         //             Payload = serializedPayload
//         //         };

//         //         await _eventPublisher.SaveEventAsync(@event);
//         //         await _eventPublisher.PublishPendingEventsAsync();
//         //     }
//         //     else
//         //     {
//         //         throw new ExceptionRules("PartyMaster creation failed.");
//         //     }

//         //     return result;
//         // }
//         public async Task<int> Handle(CreatePartyMasterCommand request, CancellationToken cancellationToken)
//         {
//             var dto = request.PartyMaster;

//             // ------------------- Clean Addresses -------------------
//             if (dto.PartyAddresses?.Any() == true)
//             {
//                 dto.PartyAddresses = dto.PartyAddresses
//                     .Where(c =>
//                         !(string.Equals(c.AddressType?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//                         string.Equals(c.AddressLine1?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//                         string.Equals(c.City?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//                         string.Equals(c.State?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//                         string.Equals(c.PostalCode?.Trim(), "string", StringComparison.OrdinalIgnoreCase) &&
//                         string.Equals(c.Country?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
//                     )
//                     .ToList();

//                 if (!dto.PartyAddresses.Any())
//                     dto.PartyAddresses = null;
//             }

//             // ------------------- Clean Contacts -------------------
//             if (dto.PartyContacts != null)
//             {
//                 dto.PartyContacts = dto.PartyContacts
//                     .Where(c =>
//                         !string.Equals(c.FirstName, "string", StringComparison.OrdinalIgnoreCase) &&
//                         !string.Equals(c.MobileNo, "string", StringComparison.OrdinalIgnoreCase) &&
//                         !string.Equals(c.ContactBy, "string", StringComparison.OrdinalIgnoreCase))
//                     .ToList();

//                 if (!dto.PartyContacts.Any())
//                     dto.PartyContacts = null;
//             }

//             // ------------------- Clean Banks -------------------
//             if (dto.PartyBanks != null)
//             {
//                 dto.PartyBanks = dto.PartyBanks
//                     .Where(b =>
//                         !string.Equals(b.BankName, "string", StringComparison.OrdinalIgnoreCase) &&
//                         !string.Equals(b.BankAccountNumber, "string", StringComparison.OrdinalIgnoreCase) &&
//                         !string.Equals(b.BankBranch, "string", StringComparison.OrdinalIgnoreCase) &&
//                         !string.Equals(b.IFSCCode, "string", StringComparison.OrdinalIgnoreCase) &&
//                         b.AccountTypeId != 0)
//                     .ToList();

//                 if (!dto.PartyBanks.Any())
//                     dto.PartyBanks = null;
//             }

//             // ------------------- Generate PartyCode -------------------
//             var nextPartyCode = await _partyMasterCommandRepository.GetNextPartyCodeAsync();

//             // ------------------- Clean & Rename Docs -------------------
//             if (dto.PartyDocuments != null && dto.PartyDocuments.Any())
//             {
//                 dto.PartyDocuments = dto.PartyDocuments
//                     .Where(d => d.DocumentId != 0 &&
//                                 !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//                     .ToList();

//                 if (dto.PartyDocuments.Any())
//                 {
//                     string baseDirectory = await _ipartyMasterQueryRepository.GetBaseDirectoryAsync();
//                     string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
//                     EnsureDirectoryExists(uploadPath);

//                     foreach (var doc in dto.PartyDocuments)
//                     {
//                         if (string.IsNullOrWhiteSpace(doc.FileName))
//                             continue;

//                         string oldFilePath = Path.Combine(uploadPath, doc.FileName);
//                         if (File.Exists(oldFilePath))
//                         {
//                             string newFileName = $"{nextPartyCode}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
//                             string newFilePath = Path.Combine(uploadPath, newFileName);

//                             try
//                             {
//                                 File.Move(oldFilePath, newFilePath, overwrite: true);
//                                 doc.FileName = newFileName;
//                                 doc.UploadedDate = DateTimeOffset.UtcNow;
//                             }
//                             catch (Exception ex)
//                             {
//                                 throw new InvalidOperationException(
//                                     $"File upload failed while renaming '{doc.FileName}' to '{newFileName}': {ex.Message}", ex);
//                             }
//                         }
//                     }
//                 }
//                 else
//                 {
//                     dto.PartyDocuments = null;
//                 }
//             }

//             // ------------------- Map to Entity -------------------
//             var partyMasterEntity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);
//             partyMasterEntity.PartyCode = nextPartyCode;

//             // ------------------- gRPC Location Service -------------------
//             if (partyMasterEntity.PartyAddressTypes != null && partyMasterEntity.PartyAddressTypes.Any())
//             {
//                 for (int i = 0; i < dto.PartyAddresses.Count; i++)
//                 {
//                     var addressDto = dto.PartyAddresses[i];
//                     var addressEntity = partyMasterEntity.PartyAddressTypes.ElementAt(i);

//                     if (!string.IsNullOrWhiteSpace(addressDto.City) &&
//                         !string.IsNullOrWhiteSpace(addressDto.State) &&
//                         !string.IsNullOrWhiteSpace(addressDto.Country))
//                     {
//                         var location = await _locationGrpcClient.GetOrCreateLocationAsync(
//                             addressDto.City,
//                             addressDto.State,
//                             addressDto.Country
//                         );

//                         if (location == null)
//                             throw new Exception("Location could not be resolved via gRPC.");

//                         addressEntity.CityId = location.CityId;
//                         addressEntity.StateId = location.StateId;
//                         addressEntity.CountryId = location.CountryId;
//                     }
//                 }
//             }

//             // ------------------- Save to DB -------------------
//             var result = await _partyMasterCommandRepository.CreateAsync(partyMasterEntity);
//             if (result <= 0)
//                 throw new ExceptionRules("PartyMaster creation failed.");

//             // ------------------- Fetch entity again for mapping -------------------
//             var entity = await _partyMasterCommandRepository.GetByIdPartyMasterWorkFlowAsync(result);
//             var reverseMap = _mapper.Map<CreatePartyMasterReverseDto>(entity);
//             string serializedPayload = JsonSerializer.Serialize(reverseMap);

//             // ------------------- Audit Logs -------------------
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode: partyMasterEntity.PartyCode ?? "NULL",
//                 actionName: partyMasterEntity.PartyName ?? "NULL",
//                 details: "PartyMaster details created",
//                 module: "PartyMaster"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);

//             var log = new PartyActivityLog
//             {
//                 PartyId = partyMasterEntity.Id,
//                 TableName = "PartyMaster",
//                 ColumnName = "PartyName",
//                 OldValue = "",
//                 NewValue = partyMasterEntity.PartyName ?? string.Empty,
//                 ActionType = "Insert",
//                 ChangedBy = partyMasterEntity.CreatedBy,
//                 ChangedByName = partyMasterEntity.CreatedByName ?? string.Empty,
//                 ChangedIp = partyMasterEntity.CreatedIP ?? string.Empty,
//                 ChangedOn = DateTimeOffset.UtcNow
//             };

//             await _ipartyActivityLogCommandRepository.InsertAsync(log, cancellationToken);

//             // ------------------- Publish Outbox Event -------------------
//             var correlationId = Guid.NewGuid();
//             var @event = new TransactionCreatedEvent
//             {
//                 CorrelationId = correlationId,
//                 ModuleTypeName = MiscEnumEntity.PartyDocumentImage.PartyMaster.ToString(),
//                 ModuleTransactionId = result,
//                 Payload = serializedPayload
//             };

//             await _eventPublisher.SaveEventAsync(@event);
//             await _eventPublisher.PublishPendingEventsAsync();

//             return result;
//         }

//          private void EnsureDirectoryExists(string path)
//         {
//             if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
//             {
//                 Directory.CreateDirectory(path);
//             }
//         }  

       
       
//     }

// }