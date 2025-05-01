//using Microsoft.Extensions.Configuration;
//using System.Net;
//using System.Net.Mail;

//namespace BusinessLogicLayer.Helper
//{
//    public class EmailHelper
//    {
//        private readonly IConfiguration _config;

//        public EmailHelper(IConfiguration config)
//        {
//            _config = config;
//        }

//        public void SendEmail(string toEmail, string subject, string body)
//        {
//            var settings = _config.GetSection("SmtpSettings");

//            using var smtp = new SmtpClient(settings["Server"], int.Parse(settings["Port"]))
//            {
//                Credentials = new NetworkCredential(settings["SenderEmail"], settings["Password"]),
//                EnableSsl = true
//            };

//            var mail = new MailMessage
//            {
//                From = new MailAddress(settings["SenderEmail"], settings["SenderName"]),
//                Subject = subject,
//                Body = body,
//                IsBodyHtml = true
//            };

//            mail.To.Add(toEmail);
//            smtp.Send(mail);
//        }
//    }
//}