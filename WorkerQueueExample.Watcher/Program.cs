using RabbitMQ.Client;
using System;
using System.IO;
using System.Threading;

namespace WorkerQueueExample.Watcher
{
    class Program
    {
        static void Main(string[] args)
        {
            string watchLocation = @"C:\LogsAndReports\FileWatcher";
            FileSystemWatcher fileWatcher = new FileSystemWatcher(watchLocation);
            fileWatcher.Created += WatchEvent;
            fileWatcher.EnableRaisingEvents = true;

            Console.WriteLine($"Watching {watchLocation}");

            var fileList = Directory.GetFiles(watchLocation);
            foreach (var file in fileList)
            {
                Process(file);
            }

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        public static void WatchEvent(object sender, FileSystemEventArgs e)
        {
            Process(e.FullPath);
        }

        public static void Process(string fileLocation)
        {
            try
            {
                if (!IsFileAvailable(fileLocation))
                {
                    Console.WriteLine("File is not available");
                    return;
                }

                var fileContent = File.ReadAllBytes(fileLocation);

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
                        Console.WriteLine("[x] Sent {0}", fileLocation);
                        File.Delete(fileLocation);
                        Console.WriteLine("{0} Deleted", fileLocation);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static bool IsFileAvailable(string filepath, bool wait = true)
        {
            bool fileAvailable = false;
            int retries = 60;
            const int delay = 1000; // Max time spent here = retries*delay milliseconds

            if (!File.Exists(filepath))
                return false;
            do
            {
                try
                {
                    // Attempts to open then close the file in RW mode, denying other users to place any locks.
                    FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.Close();
                    fileAvailable = true; // success
                }
                catch (IOException)
                {
                }

                if (!wait) break;

                retries--;

                if (!fileAvailable)
                    Thread.Sleep(delay);
            } while (!fileAvailable && retries > 0);

            return fileAvailable;
        }
    }
}
