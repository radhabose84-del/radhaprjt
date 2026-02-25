#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILanguage;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Language.Commands.UpdateLanguage
{
    public class UpdateLanguageCommandHandler : IRequestHandler<UpdateLanguageCommand, bool>
    {
        private readonly ILanguageCommand _languageCommand;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly ILanguageQuery _languageQuery;

        public UpdateLanguageCommandHandler(IMapper imapper, ILanguageCommand languageCommand, IMediator mediator, ILanguageQuery languageQuery)
        {
            _imapper = imapper;
            _languageCommand = languageCommand;
            _mediator = mediator;
            _languageQuery = languageQuery;
        }

        public async Task<bool> Handle(UpdateLanguageCommand request, CancellationToken cancellationToken)
        {
            var existingLanguage = await _languageQuery.GetByLanguagenameAsync(request.Name, request.Id);

            if (existingLanguage != null)
            {
                throw new ValidationException("Language already exists");
                
            }
            var language  = _imapper.Map<UserManagement.Domain.Entities.Language>(request);
         
                var languageresult = await _languageCommand.UpdateAsync(language);

                
                   
              
                if(languageresult)
                {
                     var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: language.Code,
                        actionName: language.Name,
                        details: $"Language '{language.Id}' was updated.",
                        module:"Language"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 

                    return languageresult;
                }
            throw new Exception("Language not updated.");
        }
    }
}