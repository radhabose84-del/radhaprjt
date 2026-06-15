using System.Text.Json;
using MediatR;

namespace UserManagement.Application.IconMaster.Commands.CreateIconMaster
{
    public class CreateIconMasterCommand : IRequest<int>
    {
        public string? Keyword { get; set; }
        public string? IconName { get; set; }
        public string? IconLibrary { get; set; }
        public int Size { get; set; } = 18;
        public JsonElement? Style { get; set; }
    }
}
