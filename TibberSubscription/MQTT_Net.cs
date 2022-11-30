using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TibberSubscription
{
    class MQTT_Tibber
    {
        /// <summary>
        /// Publish tibber data to MQTT broker using connection info in Resources.xml
        /// </summary>
        /// <param name="data">
        /// List of enabled topics
        /// </param>
        /// <param name="lines">
        /// Number of MQTT updates sent
        /// </param>
        /// <param name="resets">
        /// Number of API resets
        /// </param>
        /// <returns></returns>
        public async Task PublishMessage(List<string[]> data, int lines, int resets)
        {
            try
            {
                // Update console for info
                Console.Clear();
                Console.WriteLine("Number of API resets: " + resets);
                Console.WriteLine("Number of MQTT updates: " + lines);

                var mqttFactory = new MqttFactory();
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    // Setup MQTT using broker address and port from Resources.xml and connect
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(MainProgram.tibberRes.brokeraddress, MainProgram.tibberRes.brokerport)
                        .Build();
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    Console.WriteLine("\nPublished: ");
                    // Create MQTT message array that matches number of enabled topics in Resources.xml
                    MqttApplicationMessage[] applicationMessage = new MqttApplicationMessage[data.Count];
                    for (int i = 0; i < data.Count; i++)
                    {
                        // Add each enabled topics to MQTT message array
                        applicationMessage[i] = new MqttApplicationMessageBuilder()
                            .WithTopic(data[i][0])
                            .WithPayload(data[i][1])
                            .Build();
                        // Do not clear published data if data is empty, to keep them in broker
                        // due to some live subscription objects does not update each broadcast
                        // And publish non-empty topics
                        if (data[i][1] != "")
                            await mqttClient.PublishAsync(applicationMessage[i], CancellationToken.None);
                        Console.WriteLine(data[i][0] + ": " + data[i][1]);
                    }
                    // Disconnect MQTT client
                    await mqttClient.DisconnectAsync();
                    Console.WriteLine("\nRunning... Press any key to exit" + new string('.', (lines % 3) + 1));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
