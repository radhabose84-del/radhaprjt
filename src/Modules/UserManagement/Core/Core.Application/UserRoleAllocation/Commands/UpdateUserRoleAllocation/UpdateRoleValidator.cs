using  FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.UserRole.Commands.UpdateRole;

namespace Core.Application.UserRole.Commands.UpdateRole
{
    public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
    {
           public UpdateRoleValidator()
        {
  RuleFor(v => v.RoleName)
                .NotEmpty().WithMessage("ShortName is required.")
                .MaximumLength(100).WithMessage("ShortName must not exceed 6 characters.");

            RuleFor(v => v.CompanyId)
                .NotEmpty().WithMessage("CompanyId is required.")
                .Must(x => x >= 1).WithMessage("CompanyId must be a positive integer.");

            RuleFor(v => v.IsActive)
                .NotEmpty().WithMessage("IsActive is required.")
                .InclusiveBetween((byte)0, (byte)1).WithMessage("IsActive must be either 0 or 1.");
        }
        
    }
}