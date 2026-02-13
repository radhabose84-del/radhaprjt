using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using FluentValidation;


namespace PurchaseManagement.Presentation.Validation.TnCTemplateMaster
{
    public class CreateTnCTemplateMasterValidator : AbstractValidator<CreateTnCTemplateMasterCommand>
    {

        private readonly ITnCTemplateMasterQueryRepository _tnCTemplateMasterQueryRepository;

        public CreateTnCTemplateMasterValidator(ITnCTemplateMasterQueryRepository tnCTemplateMasterQueryRepository)
        {
            _tnCTemplateMasterQueryRepository = tnCTemplateMasterQueryRepository;

            ClassLevelCascadeMode = CascadeMode.Stop;

             RuleFor(x => x.TemplateName)
                .NotEmpty().WithMessage("Template Name is required.")
                .MaximumLength(200)
                .DependentRules(() =>
                {
                    RuleFor(x => x.TemplateName)
                        .Must(s => !string.IsNullOrWhiteSpace(s?.Trim()))
                        .WithMessage("Template Name is required.");
                });

            // Uniqueness: (TemplateTypeId, TemplateName)
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await _tnCTemplateMasterQueryRepository.ExistsByTypeAndNameAsync(
                        cmd.TemplateTypeId,
                        cmd.TemplateName?.Trim() ?? string.Empty,
                        null,
                        ct))
                .WithMessage("A template with this name already exists for the selected Template Type.");

            // Template Type
            RuleFor(x => x.TemplateTypeId)
                .GreaterThan(0).WithMessage("Template Type is required.");

            // Terms HTML (not blank / not only whitespace or empty tags)
            RuleFor(x => x.TermsHtml)
                .NotEmpty().WithMessage("Terms & Conditions content is required.")
                .Must(HasMeaningfulHtml)
                .WithMessage("Terms & Conditions cannot be empty markup.");

            // Applicabilities: require at least one, no duplicates
            RuleFor(x => x.Applicabilities)
                .NotNull().WithMessage("At least one Applicability is required.")
                .Must(list => list != null && list.Count > 0)
                    .WithMessage("At least one Applicability is required.")
                .Must(list => list == null || list.Select(a => a.ApplicabilityId).Distinct().Count() == list.Count)
                    .WithMessage("Duplicate Applicability values are not allowed.");

            
        }

        // Simple “non-empty HTML” check (strips tags and &nbsp;)
        private static bool HasMeaningfulHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return false;
            var noTags = Regex.Replace(html, "<.*?>", string.Empty, RegexOptions.Singleline);
            noTags = Regex.Replace(noTags, "&nbsp;", " ", RegexOptions.IgnoreCase).Trim();
            return !string.IsNullOrWhiteSpace(noTags);
        }
        


    }
}
