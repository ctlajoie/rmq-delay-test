using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            const string MESSAGE = @"The message body";
            const string DELAY_EXCH = "Delay_TestMessages";
            const string REGULAR_EXCH = "NoDelay_TestMessages";
            const string DEST_QUEUE = "TestMessages";

            var connFactory = new ConnectionFactory() { Uri = "amqp://localhost" };
            using (var conn = connFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {
                channel.ExchangeDeclare(DELAY_EXCH, "x-delayed-message",
                                        durable: true,
                                        autoDelete: false,
                                        arguments: new Dictionary<string, object>() {
                                                    ["x-delayed-type"] = "fanout" });

                channel.ExchangeDeclare(REGULAR_EXCH, "fanout",
                                        durable: true,
                                        autoDelete: false);

                channel.QueueDeclare(DEST_QUEUE,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false);

                channel.QueueBind(DEST_QUEUE, DELAY_EXCH, "");
                channel.QueueBind(DEST_QUEUE, REGULAR_EXCH, "");

                var props = channel.CreateBasicProperties();
                props.Headers = new Dictionary<string, object>() {
                    ["x-delay"] = 5000
                };

                var body = System.Text.Encoding.ASCII.GetBytes(MESSAGE);
                channel.BasicPublish(REGULAR_EXCH, "", props, body);
                channel.BasicPublish(DELAY_EXCH, "", props, body);
            }
        }
    }
}
