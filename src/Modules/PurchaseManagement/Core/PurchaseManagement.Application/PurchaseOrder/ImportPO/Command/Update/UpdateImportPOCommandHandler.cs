    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
    using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
    using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
    using PurchaseManagement.Domain.Common;
    using PurchaseManagement.Domain.Entities.PurchaseOrder;
    using MediatR;

    namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Update;

    public class UpdateImportPOCommandHandler : IRequestHandler<UpdateImportPOCommand, bool>
    {
        private readonly IImportPOCommandRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPODocumentQueryRepository _poDocs;

        public UpdateImportPOCommandHandler(
            IImportPOCommandRepository repo,
            IMapper mapper,
            IPODocumentQueryRepository poDocs)
        {
            _repo = repo;
            _mapper = mapper;
            _poDocs = poDocs;
        }

        public async Task<bool> Handle(UpdateImportPOCommand request, CancellationToken ct)
        {
            var dto = request.Data ?? throw new ValidationException("Body required.");
            if (dto.Id <= 0) throw new ValidationException("Purchase Order id is required.");

            // Map header + children (repo will delete & rebuild children)
            var incoming = _mapper.Map<PurchaseOrderHeader>(dto);
            incoming.Id = dto.Id;            

            // 🔧 Normalize ALL documents sent by the client (single list)
        if (dto.Documents != null && dto.Documents.Count > 0)
        {
            var baseDir = MiscEnumEntity.DocumentPath;

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var doc in dto.Documents.Where(d => !string.IsNullOrWhiteSpace(d.FileName)))
            {
                var oldPath = Path.Combine(uploadDir, doc.FileName!);
                if (File.Exists(oldPath))
                {
                    var finalName = $"{dto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
                    var newPath = Path.Combine(uploadDir, finalName);

                    if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(oldPath, newPath, overwrite: true);
                        doc.FileName = finalName;
                    }
                }

                if (doc.UploadedDate == default)
                    doc.UploadedDate = DateTimeOffset.UtcNow;
            }
        }
           
            var updatedId = await _repo.UpdateAsync(incoming, dto, ct);
            return updatedId > 0;
        }
    }
