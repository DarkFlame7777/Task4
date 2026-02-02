namespace Task4.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string name, string verificationToken);
    }
}