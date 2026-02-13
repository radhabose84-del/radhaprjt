using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using FluentValidation;

namespace ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure
{
    public class DeleteProjectWorkBreakdownStructureCommandValidator   : AbstractValidator<DeleteProjectWorkBreakdownStructureCommand>
    {
        public DeleteProjectWorkBreakdownStructureCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than zero.");
        }
    }
}