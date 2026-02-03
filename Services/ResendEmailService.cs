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

                var baseUrl = _configuration["AppBaseUrl"];
                
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
                    {
                        baseUrl = "https://" + baseUrl;
                    }
                }
                else
                {
                    baseUrl = "https://task4-production-5742.up.railway.app";
                }


                baseUrl = baseUrl.TrimEnd('/');
                

                var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={verificationToken}";
                
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "onboarding@resend.dev";

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

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
                await _httpClient.PostAsync("https://api.resend.com/emails", content);
            }
            catch
            {

            }
        }
    }
}
