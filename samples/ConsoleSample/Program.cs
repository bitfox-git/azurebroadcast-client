using Bitfox.AzureBroadcast;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ConsoleSample.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            //setup appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            //read configuration values
            var url = configuration["AzureFunction:url"].ToString();
            var code = configuration["AzureFunction:code"].ToString();

            //Setup client
            var client = new BroadcastClient<string>(url, code);
            client.onMessage = (msg, info) =>
            {
                Console.WriteLine($"{msg} from {info.fromUser} for group: {info.toGroupName}");
            };

            Console.WriteLine("Press enter to exit, or message to send.");
            client.Start();

            var input = Console.ReadLine();
            while (input!="")
            {
                client.Send(input);
                input = Console.ReadLine();
            }
        }
    }
}
