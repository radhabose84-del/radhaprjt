using FluentValidation;

namespace UserManagement.UnitTests.Common.Helpers;

public static class ValidationHelper
{
    public static void ShouldHaveErrorFor<T>(this IValidator<T> validator, T instance, string propertyName)
    {
        var result = validator.Validate(instance);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == propertyName);
    }

    public static void ShouldBeValid<T>(this IValidator<T> validator, T instance)
        => validator.Validate(instance).IsValid.Should().BeTrue();
}
