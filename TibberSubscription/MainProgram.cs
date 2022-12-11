using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tibber.Sdk;

// Using Tibber.SDK from https://github.com/tibber/Tibber.SDK.NET
// Simple console application to subscribe to Tibber live data and publish to MQTT

namespace TibberSubscription
{
    /// <summary>
    /// Remember to configure Resources.xml to your API and MQTT settings.
    /// </summary>
    internal class MainProgram
    {
        public static int resets = 0;
        public static TibberResource tibberRes;
        public static Logger logger = new Logger();
        static void Main()
        {
            try
            {
                tibberRes = ReadXml.ReadXML();
                RunTask();
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                logger.LogEntry($"Main exception thrown:{ex.Message}", "CRITICAL");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }

        public static void RunTask()
        {
            Task.Run(() =>
            {
                Console.WriteLine("Running");
            }).ContinueWith(t => Subscript());
        }

        /// <summary>
        /// Subscribe to Tibber API
        /// </summary>
        /// <returns></returns>
        public static async Task Subscript()
        {
            try
            {
                if (
                    tibberRes.productheader == null ||
                    tibberRes.productversion == null ||
                    tibberRes.apikey == null ||
                    tibberRes.homeid == null ||
                    tibberRes.basetopic == null)
                    throw new Exception("Error in Resource.xml, please check file");

                Console.WriteLine("Initializing...");
                logger.LogEntry("Connecting to Tibber", "STARTUP");
                // Setup needed variables for tibber api subscription, get details from Resources.xml
                var userAgent = new ProductInfoHeaderValue(tibberRes.productheader, tibberRes.productversion);
                var client = new TibberApiClient(tibberRes.apikey, userAgent);
                var homeId = Guid.Parse(tibberRes.homeid);
                var listener = await client.StartRealTimeMeasurementListener(homeId);
                listener.Subscribe(new RealTimeMeasurementObserver());
            }
            catch (Exception ex)
            {
                logger.LogEntry(ex.Message, "CONNERR");
            }
        }
    }

    public class RealTimeMeasurementObserver : IObserver<RealTimeMeasurement>
    {
        // Create MQTT_Tibber object
        MQTT_Tibber tibberMqtt = new MQTT_Tibber();
        // Count published messages
        int lines = 1;
        // runs is used to delay MQTT published data to be slower than tibber live subscription broadcast speed
        int runs = 0;
        // Set base MQTT topic to use
        string mainTopic = MainProgram.tibberRes.basetopic;
        List<string[]> topicList = new List<string[]>();
        
        public void OnCompleted()
        {
            // Current live subscription will end.
            Console.WriteLine("Real time measurement stream has been terminated.\n");
            MainProgram.logger.LogEntry("Real time measurement stream has been terminated", "STOPPED");
            // Start new subscription.
            if (MainProgram.tibberRes.reconnect == "yes")
            {
                Console.WriteLine("Sleeping for 60 sec");
                Thread.Sleep(60 * 1000);
                MainProgram.logger.LogEntry("Trying to reconnect", "RECONNECTING");
                Task.Run(() => MainProgram.RunTask());
                //Task.Run(() => MainProgram.Subscript());
                return;
            }
            Console.WriteLine("Auto reconnect is off, change in Resources.xml\nPress any key to exit.");
        }
        public void OnError(Exception error)
        {
            // If tibber subscription is lost, add 1 to resets and goto "OnComplete"
            Console.WriteLine($"An error occurred: {error}\n");
            MainProgram.logger.LogEntry($"An error occurred: {error}", "ERROR");
            MainProgram.resets++;
        }
        public void OnNext(RealTimeMeasurement value)
        {
            topicList.Clear();
            // Get live Tibber data, map to MQTT topic configured in Resources.xml and publish enabled topics to MQTT
            if (runs % MainProgram.tibberRes.delay == 0)
            {
                // Map subscription values to enabled topics defined in Resources.xml
                foreach (var topic in MainProgram.tibberRes.topics)
                {
                    if(topic.Enabled == "true")
                    {
                        switch(topic.Name)
                        {
                            case ("Timestamp"):
                                topicList.Add(new string[] { mainTopic + "/Timestamp", value.Timestamp.ToString() });
                                break;
                            case ("Power"):
                                topicList.Add(new string[] { mainTopic + "/Power", value.Power.ToString() });
                                break;
                            case ("LastMeterConsumption"):
                                topicList.Add(new string[] { mainTopic + "/LastMeterConsumption", value.LastMeterConsumption.ToString() });
                                break;
                            case ("AccumulatedConsumption"):
                                topicList.Add(new string[] { mainTopic + "/AccumulatedConsumption", value.AccumulatedConsumption.ToString() });
                                break;
                            case ("AccumulatedConsumptionLastHour"):
                                topicList.Add(new string[] { mainTopic + "/AccumulatedConsumptionLastHour", value.AccumulatedConsumptionLastHour.ToString() });
                                break;
                            case ("AccumulatedProduction"):
                                topicList.Add(new string[] { mainTopic + "/AccumulatedProduction", value.AccumulatedProduction.ToString() });
                                break;
                            case ("AccumulatedProductionLastHour"):
                                topicList.Add(new string[] { mainTopic + "/AccumulatedProductionLastHour", value.AccumulatedProductionLastHour.ToString() });
                                break;
                            case ("AccumulatedCost"):
                                topicList.Add(new string[] { mainTopic + "/AccumulatedCost", value.AccumulatedCost.ToString() });
                                break;
                            case ("AccumulatedReward"):
                                topicList.Add(new string[] { mainTopic + "/AccumulatedReward", value.AccumulatedReward.ToString() });
                                break;
                            case ("Currency"):
                                topicList.Add(new string[] { mainTopic + "/Currency", value.Currency.ToString() });
                                break;
                            case ("MinPower"):
                                topicList.Add(new string[] { mainTopic + "/MinPower", value.MinPower.ToString() });
                                break;
                            case ("AveragePower"):
                                topicList.Add(new string[] { mainTopic + "/AveragePower", value.AveragePower.ToString() });
                                break;
                            case ("MaxPower"):
                                topicList.Add(new string[] { mainTopic + "/MaxPower", value.MaxPower.ToString() });
                                break;
                            case ("PowerProduction"):
                                topicList.Add(new string[] { mainTopic + "/PowerProduction", value.PowerProduction.ToString() });
                                break;
                            case ("PowerReactive"):
                                topicList.Add(new string[] { mainTopic + "/PowerReactive", value.PowerReactive.ToString() });
                                break;
                            case ("PowerProductionReactive"):
                                topicList.Add(new string[] { mainTopic + "/PowerProductionReactive", value.PowerProductionReactive.ToString() });
                                break;
                            case ("MinPowerProduction"):
                                topicList.Add(new string[] { mainTopic + "/MinPowerProduction", value.MinPowerProduction.ToString() });
                                break;
                            case ("MaxPowerProduction"):
                                topicList.Add(new string[] { mainTopic + "/MaxPowerProduction", value.MaxPowerProduction.ToString() });
                                break;
                            case ("LastMeterProduction"):
                                topicList.Add(new string[] { mainTopic + "/LastMeterProduction", value.LastMeterProduction.ToString() });
                                break;
                            case ("VoltagePhase1"):
                                topicList.Add(new string[] { mainTopic + "/VoltagePhase1", value.VoltagePhase1.ToString() });
                                break;
                            case ("VoltagePhase2"):
                                topicList.Add(new string[] { mainTopic + "/VoltagePhase2", value.VoltagePhase2.ToString() });
                                break;
                            case ("VoltagePhase3"):
                                topicList.Add(new string[] { mainTopic + "/VoltagePhase3", value.VoltagePhase3.ToString() });
                                break;
                            case ("CurrentPhase1"):
                                topicList.Add(new string[] { mainTopic + "/CurrentPhase1", value.CurrentPhase1.ToString() });
                                break;
                            case ("CurrentPhase2"):
                                topicList.Add(new string[] { mainTopic + "/CurrentPhase2", value.CurrentPhase2.ToString() });
                                break;
                            case ("CurrentPhase3"):
                                topicList.Add(new string[] { mainTopic + "/CurrentPhase3", value.CurrentPhase3.ToString() });
                                break;
                            case ("PowerFactor"):
                                topicList.Add(new string[] { mainTopic + "/PowerFactor", value.PowerFactor.ToString() });
                                break;
                            case ("SignalStrength"):
                                topicList.Add(new string[] { mainTopic + "/SignalStrength", value.SignalStrength.ToString() });
                                break;
                            default:
                                Console.WriteLine("Topic " + topic.Name + " is not defined.");
                                break;
                        }

                    }
                }
                // If no topics defined or no MQTT broker defined, no point in running.
                if(topicList.Count == 0 || MainProgram.tibberRes.brokeraddress == null)
                {
                    Console.Clear();
                    Console.WriteLine("No topics configured, missing MQTT broker details or error in XML. Please check Resource.xml");
                    Console.WriteLine("Subscription ended.");
                    Console.WriteLine("Press any key to exit");
                    MainProgram.logger.LogEntry("No Topics or MQTT Broker address", "TOPICS");
                    // End thread
                    throw new TaskCanceledException();
                }
                // Run MQTT publish task.
                Task.Run(() => tibberMqtt.PublishMessage(topicList, lines, MainProgram.resets));
                lines++;
            }
            runs++;
        }
    }
}
