using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById
{
    public class ServiceEntrySheetDocumentDto
    {

        // public int DocumentId { get; set; }
        // public string? FileName { get; set; }
        // public DateTimeOffset UploadedDate { get; set; }
        // public string? UploadedPath { get; set; }
        // public string? DocumentName { get; set; }  
            

            public int Id { get; set; }
            public int ServiceEntrySheetId { get; set; }
            public int DocumentId { get; set; }
            public string FileName { get; set; } = null!;
            public DateTimeOffset UploadedDate { get; set; }
            public string? UploadedPath { get; set; }
            public string? DocumentName { get; set; }    
     
      
    }
}