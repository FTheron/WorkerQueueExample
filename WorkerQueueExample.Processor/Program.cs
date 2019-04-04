using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;

namespace WorkerQueueExample.Processor
{
    class Program
    {
        static void Main(string[] args)
        {
            var mailLocation = "C:\\LogsAndReports\\Processed\\";
            Console.WriteLine("Please enter the sublocation you want the file to drop:");
            var sublocation = Console.ReadLine();

            Directory.CreateDirectory($"{mailLocation}\\{sublocation}");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        File.WriteAllBytes($"{mailLocation}\\{sublocation}\\file_{Guid.NewGuid()}.txt", body);
                        Console.WriteLine("[x] Received {0}", message);
                    };
                    channel.BasicConsume(queue: "hello",
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine("Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
