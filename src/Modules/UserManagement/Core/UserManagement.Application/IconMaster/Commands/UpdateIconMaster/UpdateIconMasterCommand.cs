using System.Text.Json;
using MediatR;

namespace UserManagement.Application.IconMaster.Commands.UpdateIconMaster
{
    public class UpdateIconMasterCommand : IRequest<int>
    {
        public int Id { get; set; }
        // Keyword is immutable — excluded from update
        public string? IconName { get; set; }
        public string? IconLibrary { get; set; }
        public int Size { get; set; } = 18;
        public JsonElement? Style { get; set; }
        public byte IsActive { get; set; }
    }
}
