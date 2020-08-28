using System;
using System.Net.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace jtd_utilities
{
    public class mail
    {
        static public void SendEmailMessage(String msg)
        {
            MailMessage Message = new MailMessage();
            SmtpClient Smtp = new SmtpClient();
            System.Net.NetworkCredential SmtpUser = new System.Net.NetworkCredential();

            Message.From = new MailAddress("5138843612@tmomail.net", "5138843612");
            Message.IsBodyHtml = true;

            //MailAddress toAddress0 = new MailAddress("5138843612@tmomail.net");
            //MailAddress toAddress0 = new MailAddress("5133005031@txt.att.net");
            MailAddress toAddress1 = new MailAddress("bob@leroynet.com");
            MailAddress toAddress2 = new MailAddress("holly@jtdinc.com");
            Message.To.Add(toAddress1);
            Message.To.Add(toAddress2);
            //Message.To.Add(toAddress2);
            Message.Subject = "SOM Integration Error";
            Message.Body += "\r\n===========================================================";
            Message.Body += "\r\n" + msg;
            Message.Body += "\r\n===========================================================";

            //-- Define Authenticated User
            SmtpUser.UserName = Properties.sql.Default.MailUsername;
            SmtpUser.Password = Properties.sql.Default.MailPassword;

            //-- Send Message
            Smtp.UseDefaultCredentials = false;
            Smtp.Credentials = SmtpUser;
            Smtp.Host = "mail.leroynet.com";
            Smtp.Port = 26;
            Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                Smtp.Send(Message);
            }
            catch (Exception ex)
            {

            }
        }

        static public void SpiffEmailMessage(String msg)
        {
            MailMessage Message = new MailMessage();
            SmtpClient Smtp = new SmtpClient();
            System.Net.NetworkCredential SmtpUser = new System.Net.NetworkCredential();

            Message.From = new MailAddress("5138843612@tmomail.net", "5138843612");
            Message.IsBodyHtml = true;

            //MailAddress toAddress0 = new MailAddress("5138843612@tmomail.net");
            //MailAddress toAddress0 = new MailAddress("5133005031@txt.att.net");
            MailAddress toAddress1 = new MailAddress("bob@leroynet.com");
            //MailAddress toAddress2 = new MailAddress("holly@jtdinc.com");
            //MailAddress toAddress3 = new MailAddress("TisdelVIP@360Insights.com");
            Message.To.Add(toAddress1);
            //Message.To.Add(toAddress2);
            //Message.To.Add(toAddress3);

            Message.Subject = "Tisdel Spiff Data";
            Message.Body += "\r\n===========================================================";
            Message.Body += "\r\n" + msg;
            Message.Body += "\r\n===========================================================";
            Attachment csvAttachment = new Attachment(@"./TisdelSpiff.txt");
            Message.Attachments.Add(csvAttachment);

            //-- Define Authenticated User
            SmtpUser.UserName = Properties.sql.Default.MailUsername;
            SmtpUser.Password = Properties.sql.Default.MailPassword;

            //-- Send Message
            Smtp.UseDefaultCredentials = false;
            Smtp.Credentials = SmtpUser;
            Smtp.Host = "mail.leroynet.com";
            Smtp.Port = 26;
            Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            
            try
            {
                Smtp.Send(Message);
            }
            catch (Exception ex)
            {

            }
        }

        static public void SendTwilioMessage(String msg)
        {
            // Find your Account Sid and Token at twilio.com/console
            const string accountSid = "ACbb0cc2f2daeadc53ac42a84bc8f32096";
            const string authToken = "531d5cd000d007e3e9e71a41df1a5f6c";

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: msg,
                from: new Twilio.Types.PhoneNumber("+18593081287"),
                to: new Twilio.Types.PhoneNumber("+15138843612")
            );

            Console.WriteLine(message.Sid);
        }
    }
}