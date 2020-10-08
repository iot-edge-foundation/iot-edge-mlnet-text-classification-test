namespace MlNetModule
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using MlNetAppML.Model;
    using System.Linq;
    using Newtonsoft.Json;

    class Program
    {
        static int counter;

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
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (string.IsNullOrEmpty(messageString))
            {
                System.Console.WriteLine($"Request '{messageString}' cannot be converted into the right request. Ignored.");

                return MessageResponse.Completed;
            }

            var request = JsonConvert.DeserializeObject<Request>(messageString);

            ModelInput modelInput = new ModelInput()
            {
                Comment = request.comment,
            };

            try
            {
                // Make a single prediction on the sample data and print results
                var predictionResult = ConsumeModel.Predict(modelInput);

                var scores = predictionResult.Score.Select(x => new Score{entry = x}).ToArray();

                var response = new Response()
                {
                    comment = modelInput.Comment,
                    prediction = predictionResult.Prediction,
                    scores = scores,
                };

                var jsonString = JsonConvert.SerializeObject(response);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    using (var pipeMessage = new Message(UTF8Encoding.UTF8.GetBytes(jsonString)))
                    {
                        await moduleClient.SendEventAsync("output1", pipeMessage);
                    
                        Console.WriteLine($"Scored '{response.comment}' message ({response.prediction}) sent.");
                    }
                }
                else
                {
                    System.Console.WriteLine($"Response '{jsonString}' cannot be converted into the right reponse. Ignored.");
                }
            }
            catch(Exception ex)
            {
                System.Console.WriteLine($"Exception: {ex.Message}");
            }

            return MessageResponse.Completed;
        }
    }
    
    public class Request
    {
        public string comment {get; set;}
    }

    public class Response
    {
        public string comment {get; set;}
        public string prediction {get; set;}

        public Score[] scores {get; set;}
    }

    public class Score
    {
        public float entry {get; set;}
    }
}
