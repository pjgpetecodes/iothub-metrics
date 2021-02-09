using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace iothub_metrics
{
    class Program
    {

		static string DeviceConnectionString = "";
		static DeviceClient Client = null;

        static async Task Main(string[] args)
        {
            Console.WriteLine("****************************************************");
			Console.WriteLine("    Welcome to the Azure IoT Hub Metrics Tester");
			Console.WriteLine();
			Console.WriteLine("Author: Pete Gallagher");
			Console.WriteLine("Twitter: @pete_codes");
			Console.WriteLine("Date: 9th February 2021");
			Console.WriteLine();
            Console.WriteLine("****************************************************");
			Console.WriteLine();

            try
            {
                Console.WriteLine("Enter the Device Connection String");
				DeviceConnectionString = Console.ReadLine();
				
				InitClient();

                await Client.SetReceiveMessageHandlerAsync(OnReceiveMessage, null);
                await Client.SetMethodHandlerAsync("performUpdate", performUpdate, null);

                while (true)
                {
                    Console.WriteLine("Enter Number of messages to send (Leave Empty to exit)");
					var messagesToSend = Console.ReadLine();

					if (messagesToSend != null && messagesToSend != "")
					{
                        BeginSendingMessages(int.Parse(messagesToSend)).Wait();
					}
					else
					{
						return;
					}
                }
            }
            catch (System.Exception ex)
            {                
                Console.WriteLine();
				Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        public static void InitClient()
		{
			try
			{
				Console.WriteLine("Connecting to hub");
				Client = DeviceClient.CreateFromConnectionString(DeviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
			}
			catch (Exception ex)
			{
				Console.WriteLine();
				Console.WriteLine("Error in sample: {0}", ex.Message);
			}
		}

        private static async Task OnReceiveMessage(Message message, object userContext)
        {
            var reader = new StreamReader(message.BodyStream);
            var messageContents = reader.ReadToEnd();
            Console.WriteLine($"Message Contents: {messageContents}");

            Console.WriteLine("Message Propeties:");

            foreach (var property in message.Properties)
            {
                Console.WriteLine($"Key: {property.Key}, Value: {property.Value}");
            }

            await Client.CompleteAsync(message);            
        }

        private static Task<MethodResponse> performUpdate(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("IoT Hub invoked the 'performUpdate' method.");
            Console.WriteLine("Payload:");
            Console.WriteLine(methodRequest.DataAsJson);

            var responseMessage = "{\"response\": \"OK\"}";

            return Task.FromResult(new MethodResponse(Encoding.ASCII.GetBytes(responseMessage), 200));
        }

        private static async Task BeginSendingMessages(int noOfMessagesToSend)
        {
            Random randomTemperature = new Random();
            Random randomHumidity = new Random();
            

            for (var i = 0; i < noOfMessagesToSend; i++)
            {
                var messageToSend = "{\"Temperature:\" + \"" + randomTemperature.Next(0,40) + "\"," + "\"Humidity:\" + \"" + randomTemperature.Next(0,100) + "\"}";

                Console.WriteLine("Message " + i + " : " + messageToSend);
                
                await SendDeviceToCloudMessageAsync(messageToSend);
            }

        }

        private static async Task SendDeviceToCloudMessageAsync(String messageToSend)
        {
            Message message = new Message(Encoding.ASCII.GetBytes(messageToSend));

            message.Properties.Add("buttonEvent", "true");

            Console.WriteLine("Sending Message {0}", messageToSend);

            try
            {
                await Client.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

    }
}
