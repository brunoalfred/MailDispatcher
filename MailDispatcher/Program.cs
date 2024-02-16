using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;

namespace MailDispatcher;

abstract class Program
{
    static void Main()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        Console.Write("📫Please enter the path to CSV file with emails: ");
        string? path = Console.ReadLine();

        string[] emailAddresses;
        string message;

        if (path != null && File.Exists(path))
        {
            emailAddresses = File.ReadAllLines(path);
        }
        else
        {
            throw new FileNotFoundException("File not found. Invalid path.");
        }

        Console.Write("📩Please enter path to file with email content: ");

        path = Console.ReadLine();

        if (path != null && File.Exists(path))
        {
            message = File.ReadAllText(path);
        }
        else
        {
            throw new FileNotFoundException("File not found. Invalid path.");
        }

        Console.Write("🔖Please enter the email address to send from: ");
        string? mailFrom = Console.ReadLine();

        if (mailFrom == null) throw new ArgumentNullException(nameof(mailFrom));
            

        SendEmail(config, emailAddresses, message, mailFrom);
    }

    static void SendEmail(IConfigurationRoot config, string[] emailAddresses, string message, string mailFrom)
    {
        

        MailMessage mail = new MailMessage
        {
            Body = message,
            BodyEncoding = null,
            BodyTransferEncoding = TransferEncoding.QuotedPrintable,
            DeliveryNotificationOptions = DeliveryNotificationOptions.None,
            From = new MailAddress(mailFrom),
            HeadersEncoding = null,
            IsBodyHtml = false,
            Priority = MailPriority.Normal,
            Sender = new MailAddress(mailFrom),
            Subject = "Email from MailDispatcher",
            SubjectEncoding = null
        };

        foreach (string emailAddress in emailAddresses)
        {
            mail.To.Add(emailAddress);
        }

        SmtpClient client = new SmtpClient(config["MAIL:HOST"], 587)
        {
            Credentials = new NetworkCredential
            {
                Password = config["MAIL:PASSWORD"],
                UserName = config["MAIL:USERNAME"]
            },
            EnableSsl = true,
        };

        try
        {
            client.Send(mail);
        }
        catch (SmtpException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            client.Dispose();
            mail.Dispose();
        }

        Console.WriteLine("Email sent");
    }
}