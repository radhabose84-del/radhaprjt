using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Domain.Entities
{
    public class PartyDocument
    {
        public int Id { get; set; }   // PK
        public int PartyId { get; set; }     // FK to PartyMaster
        public PartyMaster PartyDocumentId { get; set; } = null!;
        public int DocumentId { get; set; }     // FK to DocumentMaster
        public MiscMaster DocumentTypeMisc { get; set; } = null!;
        public string? FileName { get; set; }
        public DateTimeOffset UploadedDate { get; set; }

    }
}