using MediatR;

namespace UserManagement.Application.Language.Commands.DeleteLanguage
{
    public class DeleteLanguageCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}