using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQConsumer
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly IConfiguration _configuration;

        public RabbitMQConsumerService(ILogger<RabbitMQConsumerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // User Registration Queue
            channel.QueueDeclare(queue: "user_registration", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var registrationConsumer = new EventingBasicConsumer(channel);
            registrationConsumer.Received += (sender, ea) => HandleUserRegistration(ea);
            channel.BasicConsume(queue: "user_registration", autoAck: true, consumer: registrationConsumer);

            // Email Notifications Queue
            channel.QueueDeclare(queue: "email_notifications", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var emailConsumer = new EventingBasicConsumer(channel);
            emailConsumer.Received += (sender, ea) => HandleEmailNotification(ea);
            channel.BasicConsume(queue: "email_notifications", autoAck: true, consumer: emailConsumer);

            _logger.LogInformation("RabbitMQ Consumer started listening for messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void HandleUserRegistration(BasicDeliverEventArgs ea)
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var user = JsonConvert.DeserializeObject<UserRegistrationModel>(json);
                SendRegistrationEmail(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user registration message");
            }
        }

        private void HandleEmailNotification(BasicDeliverEventArgs ea)
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var email = JsonConvert.DeserializeObject<EmailModel>(json);
                SendGenericEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email notification message");
            }
        }

        private void SendRegistrationEmail(UserRegistrationModel user)
        {
            var email = new EmailModel
            {
                ToEmail = user.Email,
                Subject = "Registration Successful",
                Body = $"<b>Hello {user.FullName},</b><br/>You have been successfully registered as {user.Role}."
            };

            SendGenericEmail(email);
        }

        private void SendGenericEmail(EmailModel email)
        {
            try
            {
                var fromEmail = _configuration["SmtpSettings:SenderEmail"];
                var fromPassword = _configuration["SmtpSettings:Password"];

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(fromEmail, fromPassword),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Insurance Portal"),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = true
                };

                mail.To.Add(email.ToEmail);
                _logger.LogInformation("Preparing to send email to {Email}", email.ToEmail);
                smtp.Send(mail);
                _logger.LogInformation("Email sent successfully to {Email}", email.ToEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email.ToEmail);
            }
        }
    }
}