using Grpc.Core;

using GrpcServer;


namespace GrpcServer.Services
{
    public class StreamDemoService : StreamDemo.StreamDemoBase
    {
        private Random random;

        public StreamDemoService()
        {
            random = new Random();
        }

        public override async Task SterverStreamingDemo(Test request, IServerStreamWriter<Test> responseStream, ServerCallContext context)
        {
            for (int i = 0; i < 20; i++)
            {
                await responseStream.WriteAsync(new Test { Testmessage = $"Menssagem {i.ToString()}" });
                var randomnumber = random.Next(1, 10);
                await Task.Delay(randomnumber * 1000);
            }
        }
        public override async Task<Test> ClientStreamingDemo(IAsyncStreamReader<Test> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                Console.WriteLine(requestStream.Current.Testmessage);
            }
            Console.WriteLine("Client Streaming Complete");
            return new Test { Testmessage = "Sample" };
        }
        public override async Task BidirectionalStreamingDemo(IAsyncStreamReader<Test> requestStream, IServerStreamWriter<Test> responseStream, ServerCallContext context)
        {
            var tasks = new List<Task>();
            while (await requestStream.MoveNext())
            {
                Console.WriteLine("Received request: " + requestStream.Current.Testmessage);
                var task = Task.Run(async () =>
                {
                    var message = requestStream.Current.Testmessage;
                    var randomnunber = random.Next(1, 10);
                    await Task.Delay(randomnunber * 1000);
                    await responseStream.WriteAsync(new Test { Testmessage = message });
                    Console.WriteLine($"Sent Respomse : {message}");
                }
                );
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
            Console.WriteLine("Bidirectional Streaming Complete");

        }
    }
}
