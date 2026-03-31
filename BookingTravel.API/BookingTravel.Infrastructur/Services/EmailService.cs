using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BookingTravel.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BookingTravel.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username!, "Booking Travel App"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false // Chuyển thành true nếu body gửi kiểu HTML
            };
            
            mailMessage.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (Có thể log ra file hoặc console)
                Console.WriteLine($"\n[ERROR EMAIL]: Lỗi gửi email đến {toEmail} - {ex.Message}\n");
                throw new Exception("Không thể gởi email OTP. Vui lòng kiểm tra lại cấu hình SMTP.");
            }
        }
    }
}
