using Contracts.Interfaces;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu;

namespace UserManagement.Presentation.Validation.UserFavoriteMenu
{
    public class AddUserFavoriteMenuCommandValidator : AbstractValidator<AddUserFavoriteMenuCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUserFavoriteMenuQueryRepository _queryRepo;
        private readonly IIPAddressService _ipAddressService;

        public AddUserFavoriteMenuCommandValidator(
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
                            .WithMessage($"{nameof(AddUserFavoriteMenuCommand.MenuId)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.MenuId)
                            .MustAsync(async (menuId, ct) => await _queryRepo.MenuExistsAndActiveAsync(menuId))
                            .WithMessage("Menu not found or inactive.")
                            .When(x => x.MenuId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MenuId)
                            .MustAsync(async (menuId, ct) =>
                            {
                                var userId = _ipAddressService.GetUserId();
                                return !await _queryRepo.AlreadyFavoritedAsync(userId, menuId);
                            })
                            .WithMessage("Menu is already in your favorites.")
                            .When(x => x.MenuId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
