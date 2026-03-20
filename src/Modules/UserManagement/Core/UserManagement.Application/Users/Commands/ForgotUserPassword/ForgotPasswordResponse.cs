namespace UserManagement.Application.Users.Commands.ForgotUserPassword
{
    public class ForgotPasswordResponse
    {
    public string? Message { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public int PasswordResetCodeExpiryMinutes { get; set; }

    }
}