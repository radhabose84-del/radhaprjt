namespace UserManagement.Application.Users.Commands.CreateFirstTimeUserPassword
{
    public class PasswordLogDTO
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Message { get; set; }
    }
}