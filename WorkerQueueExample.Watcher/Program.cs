using RabbitMQ.Client;
using System;
using System.IO;

namespace WorkerQueueExample.Watcher
{
    class Program
    {
        static void Main(string[] args)
        {
            FileSystemWatcher fileWatcher = new FileSystemWatcher(@"C:\LogsAndReports\FileWatcher");
            fileWatcher.Created += Process;
            fileWatcher.EnableRaisingEvents = true;

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        public static void Process(object sender, FileSystemEventArgs e)
        {
            var fileContent = File.ReadAllBytes(e.FullPath);

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

                    channel.BasicPublish(exchange: "",
                                         routingKey: "hello",
                                         basicProperties: null,
                                         body: fileContent);
                    Console.WriteLine(" [x] Sent {0}", e.FullPath);
                }
            }
        }
    }
}
