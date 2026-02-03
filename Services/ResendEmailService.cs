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
            Console.WriteLine($"📧 === START EMAIL SEND ===");
            
            var apiKey = _configuration["ResendApiKey"];
            Console.WriteLine($"API Key exists: {!string.IsNullOrEmpty(apiKey)}");
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("❌ ERROR: ResendApiKey is empty!");
                return;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            
            var baseUrl = _configuration["AppBaseUrl"] ?? "https://task4-production-5742.up.railway.app";
            Console.WriteLine($"BaseUrl: {baseUrl}");
            
            var encodedToken = Uri.EscapeDataString(verificationToken);
            var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={encodedToken}";
            Console.WriteLine($"Verification link: {verificationLink}");
            
            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "onboarding@resend.dev";
            Console.WriteLine($"SenderEmail: {senderEmail}");
            
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
            
            Console.WriteLine($"📧 Sending to: {email}");
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseBody}");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Resend API error: {response.StatusCode} - {responseBody}");
            }
            else
            {
                Console.WriteLine($"✅ Email sent successfully to {email}");
            }
            
            Console.WriteLine($"📧 === END EMAIL SEND ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EXCEPTION: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }
}
