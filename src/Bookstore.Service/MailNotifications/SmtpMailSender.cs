using Rhetos.Logging;
using System.Collections.Generic;
using System.Net.Mail;

namespace Bookstore
{
    /// <summary>
    /// This class demonstrates usage of Dependency Injection in Rhetos apps:
    ///   (1) System automatically provides other components in the constructor of this class.
    ///   (2) Classes <see cref="SmtpMailSender"/> and <see cref="MailOptions"/> are registered
    ///       to DI container in <see cref="Rhetos.RhetosRuntime"/> class.
    ///   (3) Book entity repository uses member property 'IMailSender _mailSender',
    ///       resolved from DI with 'RepositoryUses' concept, in source file MailNotifications.rhe.
    /// </summary>
    /// 
    /// <remarks>
    /// DISCLAIMER:
    ///     This class is not intended to be used in production for sending e-mails,
    ///     use a background service, improve error handling and add other features
    ///     to make it more robust and safe.
    /// </remarks>
    public class SmtpMailSender : IMailSender
    {
        private readonly ILogger _logger;
        private readonly MailOptions _options;

        public SmtpMailSender(ILogProvider logProvider, MailOptions options)
        {
            _logger = logProvider.GetLogger(GetType().Name);
            _options = options;
        }

        public void SendMail(string message, List<string> emailAddresses)
        {
            if (string.IsNullOrEmpty(_options.SmtpHost))
                return;

            foreach (var emailAddress in emailAddresses)
            {
                _logger.Info($"Sending e-mail to {emailAddress}: {message}.");

                var smtpClient = new SmtpClient(_options.SmtpHost);

                var mail = new MailMessage(
                    from: _options.FromMailAddress,
                    to: emailAddress,
                    subject: "New book",
                    body: message);
                
                smtpClient.Send(mail);
            }
        }
    }
}
