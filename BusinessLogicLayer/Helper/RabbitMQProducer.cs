using ModelLayer.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace BusinessLogicLayer.Helper
{
    public static class RabbitMQProducer
    {
        public static void EnqueueUserRegistration(UserRegistrationModel user)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "user_registration",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user));
            channel.BasicPublish(
                exchange: "",
                routingKey: "user_registration",
                basicProperties: null,
                body: body);
        }

        public static void EnqueueEmail(EmailModel email)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "email_notifications",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(email));
            channel.BasicPublish(
                exchange: "",
                routingKey: "email_notifications",
                basicProperties: null,
                body: body);
        }
    }
}