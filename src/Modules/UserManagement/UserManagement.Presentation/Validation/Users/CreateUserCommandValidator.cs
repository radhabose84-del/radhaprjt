#nullable disable
using FluentValidation;
using UserManagement.Domain.Entities;
using UserManagement.Application.Users.Commands.CreateUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Common.Interfaces;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Users
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly ICompanyQueryRepository _companyQueryRepository;
        private readonly IDivisionQueryRepository _divisionQueryRepository;
        private readonly IDepartmentQueryRepository _departmentQueryRepository;
        private readonly IUserRoleQueryRepository _userRoleQueryRepository;
        private readonly IUnitQueryRepository _unitQueryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateUserCommandValidator(MaxLengthProvider maxLengthProvider, IUserQueryRepository userRepository, ICompanyQueryRepository companyQueryRepository, IDivisionQueryRepository divisionQueryRepository, IDepartmentQueryRepository departmentQueryRepository, IUserRoleQueryRepository userRoleQueryRepository, IUnitQueryRepository unitQueryRepository, IIPAddressService ipAddressService)
        {
            var MaxLen = maxLengthProvider.GetMaxLength<User>("FirstName") ?? 25;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _userQueryRepository = userRepository;
            _companyQueryRepository = companyQueryRepository;
            _divisionQueryRepository = divisionQueryRepository;
            _departmentQueryRepository = departmentQueryRepository;
            _userRoleQueryRepository = userRoleQueryRepository;
            _unitQueryRepository = unitQueryRepository;
            _ipAddressService = ipAddressService;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.FirstName)
                             .NotNull()
                             .WithMessage($"{nameof(CreateUserCommand.FirstName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUserCommand.FirstName)} {rule.Error}");

                        RuleFor(x => x.LastName)
                             .MaximumLength(MaxLen)
                             .When(x => !string.IsNullOrWhiteSpace(x.LastName))
                             .WithMessage($"{nameof(CreateUserCommand.LastName)} {rule.Error} {MaxLen}");

                        RuleFor(x => x.UserName)
                        .NotNull()
                             .WithMessage($"{nameof(CreateUserCommand.UserName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUserCommand.UserName)} {rule.Error}");

                        RuleFor(x => x.UserGroupId)
                             .NotNull()
                             .WithMessage($"{nameof(CreateUserCommand.UserGroupId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUserCommand.UserGroupId)} {rule.Error}");

                        RuleFor(x => x.UserCompanies)
                        .Cascade(CascadeMode.Stop)
                        .NotNull()
                        .WithMessage($"{rule.Error}")
                        .Must(x => x.Count > 0)
                        .WithMessage($"{rule.Error}")
                        .When(x => _ipAddressService.GetGroupcode() == "USER");

                        RuleFor(x => x.userUnits)
                        .Cascade(CascadeMode.Stop)
                        .NotNull()
                        .WithMessage($"{rule.Error}")
                        .Must(x => x.Count > 0)
                        .WithMessage($"{rule.Error}")
                        .When(x => _ipAddressService.GetGroupcode() == "USER");

                        RuleFor(x => x.userDivisions)
                        .Cascade(CascadeMode.Stop)
                        .NotNull()
                        .WithMessage($"{rule.Error}")
                        .Must(x => x.Count > 0)
                        .WithMessage($"{rule.Error}")
                        .When(x => _ipAddressService.GetGroupcode() == "USER");

                        RuleFor(x => x.userDepartments)
                        .Cascade(CascadeMode.Stop)
                        .NotNull()
                       .WithMessage($"{rule.Error}")
                       .Must(x => x.Count > 0)
                       .WithMessage($"{rule.Error}")
                       .When(x => _ipAddressService.GetGroupcode() == "USER");

                        RuleFor(x => x.userRoleAllocations)
                         .NotNull()
                        .WithMessage($"{rule.Error}")
                        .Must(x => x.Count > 0)
                        .WithMessage($"{rule.Error}");
                        break;

                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.FirstName)
                            .MaximumLength(MaxLen) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUserCommand.FirstName)} {rule.Error} {MaxLen}");

                        RuleFor(x => x.LastName)
                       .MaximumLength(MaxLen) // Dynamic value from MaxLengthProvider
                       .WithMessage($"{nameof(CreateUserCommand.LastName)} {rule.Error} {MaxLen}");

                        RuleFor(x => x.UserName)
                       .MaximumLength(MaxLen) // Dynamic value from MaxLengthProvider
                       .WithMessage($"{nameof(CreateUserCommand.UserName)} {rule.Error} {MaxLen}");

                        break;

                    case "Email":
                        RuleFor(x => x.EmailId)
                            .EmailAddress()
                            .WithMessage($"{nameof(CreateUserCommand.EmailId)} {rule.Error}");
                        break;

                    case "MobileNumber":
                        RuleFor(x => x.Mobile)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateUserCommand.Mobile)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.UserName)
                        .MustAsync(async (UserName, cancellation) => !await _userQueryRepository.AlreadyExistsAsync(UserName))
                        .WithName("User Name")
                         .WithMessage($"{rule.Error}");
                        break;
                    case "PasswordMaxLength":
                        RuleFor(x => x.Password)
                        .Length(8, 10)
                        .WithMessage($"{nameof(CreateUserCommand.Password)} {rule.Error}");
                        break;
                    case "UpperCase":
                        RuleFor(x => x.Password)
                        .Matches(@"[A-Z]+")
                        .WithMessage($"{nameof(CreateUserCommand.Password)} {rule.Error}");
                        break;
                    case "LowerCase":
                        RuleFor(x => x.Password)
                        .Matches(@"[a-z]+")
                        .WithMessage($"{nameof(CreateUserCommand.Password)} {rule.Error}");
                        break;
                    case "Numeric":
                        RuleFor(x => x.Password)
                        .Matches(@"[0-9]+")
                        .WithMessage($"{nameof(CreateUserCommand.Password)} {rule.Error}");
                        break;
                    case "SpecialCharacter":
                        RuleFor(x => x.Password)
                        .Matches(@"[!@#$%^&*(),.?""{}|<>]")
                        .WithMessage($"{nameof(CreateUserCommand.Password)} {rule.Error}");
                        break;
                    case "FKColumnDelete":
                        RuleFor(x => x.UserCompanies)
                             .ForEach(companyRule =>
                             {
                                 companyRule.MustAsync(async (company, cancellation) =>
                                     await _companyQueryRepository.FKColumnExistValidation(company.CompanyId))
                                     .WithMessage($"{rule.Error}");
                             })
                             .When(x => _ipAddressService.GetGroupcode() == "USER");

                        RuleFor(x => x.userDivisions)
                                .ForEach(divisionRule =>
                                {
                                    divisionRule.MustAsync(async (division, cancellation) =>
                                        await _divisionQueryRepository.FKColumnExistValidation(division.DivisionId))
                                        .WithMessage($"{rule.Error}");
                                })
                                .When(x => _ipAddressService.GetGroupcode() == "USER");
                        RuleFor(x => x.userUnits)
                             .ForEach(unitRule =>
                             {
                                 unitRule.MustAsync(async (unit, cancellation) =>
                                     await _unitQueryRepository.FKColumnExistValidation(unit.UnitId))
                                     .WithMessage($"{rule.Error}");
                             })
                             .When(x => _ipAddressService.GetGroupcode() == "USER");
                        RuleFor(x => x.userRoleAllocations)
                             .ForEach(RoleRule =>
                             {
                                 RoleRule.MustAsync(async (role, cancellation) =>
                                     await _userRoleQueryRepository.FKColumnExistValidation(role.UserRoleId))
                                     .WithMessage($"{rule.Error}");
                             });

                        RuleFor(x => x.userDepartments)
                             .ForEach(departmentRule =>
                             {
                                 departmentRule.MustAsync(async (department, cancellation) =>
                                     await _departmentQueryRepository.FKColumnExistValidation(department.DepartmentId))
                                     .WithMessage($"{rule.Error}");
                             })
                             .When(x => _ipAddressService.GetGroupcode() == "USER");

                        break;

                    default:
                        break;
                }
            }
            // Add explicit SQL Injection detection for UserName for Security Testing
            RuleFor(x => x.UserName)
                .Must(userName => !ContainsSqlInjectionPatterns(userName))
                .WithMessage($"{nameof(CreateUserCommand.UserName)} contains invalid characters or patterns.");
        }
        private bool ContainsSqlInjectionPatterns(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var pattern = @"(--|;|'|\b(SELECT|UPDATE|DELETE|INSERT|DROP|ALTER|CREATE|EXEC|UNION)\b)";
            return System.Text.RegularExpressions.Regex.IsMatch(input, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}