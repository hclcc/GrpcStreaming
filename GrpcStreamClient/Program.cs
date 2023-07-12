
using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using GrpcServer;

using System;
using System.IO;
using System.Xml.Serialization;


namespace GrpcStreamClient
{
    public class Program
    {
        private static Random random;
        public static async Task Main(string[] args)
        {
            //await ServerStreamingDemo();
            await BidirectionalStreamingDemo();
            Console.ReadKey();
        }

        private static async Task BidirectionalStreamingDemo()
        {

            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new StreamDemo.StreamDemoClient(channel);
            var stream = client.BidirectionalStreamingDemo();

            var requestTask = Task.Run(async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    random = new Random();
                    var randomnunber = random.Next(1, 10);
                    await Task.Delay(randomnunber * 1000);
                    await stream.RequestStream.WriteAsync(new Test { Testmessage = i.ToString() });
                    Console.WriteLine("Sent request: " + i.ToString());
                }
                await stream.RequestStream.CompleteAsync();

            });
            var responseTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    Console.WriteLine("Received response: " + stream.ResponseStream.Current.Testmessage);
                }
                Console.WriteLine("Response stream completed: " );

            });

            await Task.WhenAll(requestTask, responseTask);
        }

        private static async Task ClientStreamingDemo()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new StreamDemo.StreamDemoClient(channel);

            var stream = client.ClientStreamingDemo();
            for (int i = 0; i < 10; i++)
            {
                await stream.RequestStream.WriteAsync(new Test { Testmessage = $"Message {i}" });
            }
            stream.RequestStream.CompleteAsync();
            Console.WriteLine("Complete Client Streaming");
        }


        private static async Task ServerStreamingDemo()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new StreamDemo.StreamDemoClient(channel);
            var response = client.SterverStreamingDemo(new Test { Testmessage = "Test" });

            while (await response.ResponseStream.MoveNext(CancellationToken.None))
            {
                var value = response.ResponseStream.Current.Testmessage;
                Console.WriteLine(value);
            }

            Console.WriteLine("complete");


            await channel.ShutdownAsync();
        }

    }

}