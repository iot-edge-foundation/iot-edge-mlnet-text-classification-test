namespace MlNetTextClassificationTestModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Newtonsoft.Json;

    class Program
    {
        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            var ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");


            await ioTHubModuleClient.SetMethodHandlerAsync(
                "meassureSentiment",
                MeassureSentimentCallBack,
                ioTHubModuleClient);

            System.Console.WriteLine("Direct method 'meassureSentiment' is now attached.");
        }

        private static async Task<MethodResponse> MeassureSentimentCallBack(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"Executing MeassureSentimentCallBack at {DateTime.UtcNow}");

           var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            try
            {
                var request = JsonConvert.DeserializeObject<Request>(methodRequest.DataAsJson);

                var jsonString = JsonConvert.SerializeObject(request);
    
                using (var pipeMessage = new Message(UTF8Encoding.UTF8.GetBytes(jsonString)))
                {
                    Console.WriteLine($"Message '{request.comment}' sent");

                    await moduleClient.SendEventAsync("output1", pipeMessage);                
                }

                Console.WriteLine($"MeassureSentimentCallBack ready at {DateTime.UtcNow}.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception {ex.Message}");
            }
            
            var response = new MethodResponse(200);

            return response;  
        }

    }
    
    public class Request
    {
        public string comment {get; set;}
    }
}
