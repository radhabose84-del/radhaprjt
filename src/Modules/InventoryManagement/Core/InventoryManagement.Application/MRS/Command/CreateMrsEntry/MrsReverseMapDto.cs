using static InventoryManagement.Application.MRS.Command.CreateMrsEntry.CreateMrsEntryDto;

namespace InventoryManagement.Application.MRS.Command.CreateMrsEntry
{
    public class MrsReverseMapDto
    {
         // Represents the header information (one record)
        public CreateMrsEntryDto? Header { get; set; }

        // Represents the detail rows (many records)
        public ICollection<CreateMrsDetailDto>? Lines { get; set; } 
    }
}