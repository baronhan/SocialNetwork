using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Neo4j.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSenderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string resetpwUrl)
        {
            try
            {
                using (var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"]))
                {
                    smtpClient.Port = int.Parse(_configuration["EmailSettings:Port"]);
                    smtpClient.Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);
                    smtpClient.EnableSsl = true;

                    var messageBody = $@"
                        <p>Dear,</p>
                        <p>We received a request to reset your password. Please follow the steps below to complete the process:</p>
                        <ol>
                            <li>Click the link below to reset your password:</li>
                            <p><a href=""{resetpwUrl}"">Reset Password Link</a></p>
                            <li>If the link does not work, copy and paste the URL into your browser's address bar.</li>
                            <li>Once on the password reset page, create a new password and confirm it.</li>
                        </ol>
                        <p>For security reasons, this link will expire in 24 hours.</p>
                        <p>Thank you,<br>SocialV<br></p>";

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_configuration["EmailSettings:Username"], "SocialV"),
                        Subject = subject,
                        Body = messageBody,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (SmtpException ex)
            {
                // Ghi log lỗi hoặc thông báo tới người dùng nếu cần
                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw; // Có thể ném lại lỗi hoặc xử lý tại đây
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khác
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }
    }
}