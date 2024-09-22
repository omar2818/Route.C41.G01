﻿using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Route.C41.G01.DAL.Models;
using Route.C41.G01.PL.Services.Settings;

namespace Route.C41.G01.PL.Hepers
{
    public class EMailSettings : ImailSettings
    {
        private MailSettings _options;

        public EMailSettings(IOptions<MailSettings> options)
        {
            _options = options.Value;
        }
        public void SendMail(Email email)
        {
            var mail = new MimeMessage()
            {
                Sender = MailboxAddress.Parse(_options.Email),
                Subject = email.Subject,
            };

            mail.To.Add(MailboxAddress.Parse(email.To));
            mail.From.Add(new MailboxAddress(_options.DisplayName, _options.Email));

            var builder = new BodyBuilder();
            builder.TextBody = email.Body;
            
            mail.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_options.Email, _options.Password);
            smtp.Send(mail);

            smtp.Disconnect(true);
        }
    }
}
