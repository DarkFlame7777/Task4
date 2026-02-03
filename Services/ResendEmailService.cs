using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Task4.Services;

namespace Task4.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ResendEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task SendVerificationEmailAsync(string email, string name, string verificationToken)
        {
            try
            {
                var apiKey = _configuration["ResendApiKey"];
                if (string.IsNullOrEmpty(apiKey)) return;
        
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        
                var baseUrl = _configuration["AppBaseUrl"] ?? "https://task4-production-5742.up.railway.app";
                var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={verificationToken}";
                
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "onboarding@resend.dev";
        
                Console.WriteLine($"DEBUG: Generated link: {verificationLink}");
        
                var payload = new
                {
                    from = senderEmail,
                    to = email,
                    subject = "Verify Your Email",
                    html = $@"
                        <h3>Email Verification</h3>
                        <p>Hello {name},</p>
                        <p>Click this link to verify your email:</p>
                        <p><a href='{verificationLink}'>{verificationLink}</a></p>
                        <p>If you didn't register, please ignore this email.</p>"
                };
        
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"DEBUG: Resend response: {responseContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR sending email: {ex.Message}");
            }
        }
    }
}


