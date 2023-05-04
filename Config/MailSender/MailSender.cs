using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CT554_API.Config.MailSender
{
	public class MailSender : IEmailSender
	{
		public MailSettings MailSettings { get; }

		public MailSender(IOptions<MailSettings> mailSettings)
		{
			MailSettings = mailSettings.Value;
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			var myEmail = new MimeMessage();
			myEmail.Sender = MailboxAddress.Parse(MailSettings.Mail);
			myEmail.To.Add(MailboxAddress.Parse(email));
			myEmail.Subject = subject;
			var builder = new BodyBuilder();

			builder.HtmlBody = htmlMessage;
			myEmail.Body = builder.ToMessageBody();
			using var smtp = new SmtpClient();
			smtp.Connect(MailSettings.Host, MailSettings.Port, SecureSocketOptions.StartTls);
			smtp.Authenticate(MailSettings.Mail, MailSettings.Password);
			await smtp.SendAsync(myEmail);
			smtp.Disconnect(true);
		}
	}
}
