using Contracts.Interfaces;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu;

namespace UserManagement.Presentation.Validation.UserFavoriteMenu
{
    public class RemoveUserFavoriteMenuCommandValidator : AbstractValidator<RemoveUserFavoriteMenuCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUserFavoriteMenuQueryRepository _queryRepo;
        private readonly IIPAddressService _ipAddressService;

        public RemoveUserFavoriteMenuCommandValidator(
            IUserFavoriteMenuQueryRepository queryRepo,
            IIPAddressService ipAddressService)
        {
            _queryRepo = queryRepo;
            _ipAddressService = ipAddressService;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.MenuId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(RemoveUserFavoriteMenuCommand.MenuId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.MenuId)
                            .MustAsync(async (menuId, ct) =>
                            {
                                var userId = _ipAddressService.GetUserId();
                                return await _queryRepo.FavoriteExistsAsync(userId, menuId);
                            })
                            .WithMessage("Menu is not in your favorites.")
                            .When(x => x.MenuId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
