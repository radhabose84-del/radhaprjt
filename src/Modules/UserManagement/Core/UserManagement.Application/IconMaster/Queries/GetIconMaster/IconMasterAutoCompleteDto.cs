using System.Text.Json;

namespace UserManagement.Application.IconMaster.Queries.GetIconMaster
{
    // Autocomplete returns the FULL icon descriptor so the FE can bind directly
    public class IconMasterAutoCompleteDto
    {
        public int Id { get; set; }
        public string? Keyword { get; set; }
        public string? IconName { get; set; }
        public string? IconLibrary { get; set; }
        public int Size { get; set; }
        public JsonElement? Style { get; set; }
    }
}
