using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation.TnCTemplateMaster
{
    public class UpdateTnCTemplateMasterValidator : AbstractValidator<UpdateTnCTemplateMasterCommand>
    {


        public UpdateTnCTemplateMasterValidator(ITnCTemplateMasterQueryRepository repo)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.TemplateName)
                .NotEmpty().WithMessage("Template Name is required.")
                .MaximumLength(200);

            // Uniqueness within type, excluding the row being updated
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await repo.ExistsByTypeAndNameAsync(
                        cmd.TemplateTypeId,
                        cmd.TemplateName?.Trim() ?? string.Empty,
                        cmd.Id,
                        ct))
                .WithMessage("A template with this name already exists for the selected Template Type.");

            RuleFor(x => x.TemplateTypeId)
                .GreaterThan(0);

            RuleFor(x => x.TermsHtml)
                .NotEmpty().WithMessage("Terms & Conditions content is required.")
                .Must(HasMeaningfulHtml)
                .WithMessage("Terms & Conditions cannot be empty markup.");

            RuleFor(x => x.Applicabilities)
                .NotNull().WithMessage("At least one Applicability is required.")
                .Must(list => list != null && list.Count > 0)
                    .WithMessage("At least one Applicability is required.")
                .Must(list => list == null || list.Select(a => a.ApplicabilityId).Distinct().Count() == list.Count)
                    .WithMessage("Duplicate Applicability values are not allowed.");
        }

        private static bool HasMeaningfulHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return false;
            var noTags = Regex.Replace(html, "<.*?>", string.Empty, RegexOptions.Singleline);
            noTags = Regex.Replace(noTags, "&nbsp;", " ", RegexOptions.IgnoreCase).Trim();
            return !string.IsNullOrWhiteSpace(noTags);
        }

    }

}