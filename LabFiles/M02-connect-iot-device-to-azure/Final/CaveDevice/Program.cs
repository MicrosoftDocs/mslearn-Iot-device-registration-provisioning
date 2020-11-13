// MIT License

// Copyright (c) Microsoft Corporation. All rights reserved.

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

// INSERT using statements below here

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

// INSERT namespace below here
namespace CaveDevice
{
    class Program
    {
        // INSERT variables below here
        // Contains methods that a device can use to send messages to and receive from an IoT Hub.
        private static DeviceClient deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Note: in real-world applications you would not "hard-code" the connection string
        // It could be stored within an environment variable, passed in via the command-line or
        // store securely within a TPM module.
        private readonly static string connectionString = "{Your device connection string here}";

        // INSERT Main method below here
        private static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub C# Simulated Cave Device. Ctrl-C to exit.\n");

            // Connect to the IoT hub using the MQTT protocol
            deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        // INSERT - SendDeviceToCloudMessagesAsync below here
        // Async method to send simulated telemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            // Create an instance of our sensor 
            var sensor = new EnvironmentSensor();

            while (true)
            {
                // read data from the sensor
                var currentTemperature = sensor.ReadTemperature();
                var currentHumidity = sensor.ReadHumidity();

                var messageString = CreateMessageString(currentTemperature, currentHumidity);

                // create a byte array from the message string using ASCII encoding
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }

        // INSERT CreateMessageString method below here
        private static string CreateMessageString(double temperature, double humidity)
        {
            // Create an anonymous object that matches the data structure we wish to send
            var telemetryDataPoint = new
            {
                temperature = temperature,
                humidity = humidity
            };

            // Create a JSON string from the anonymous object
            return JsonConvert.SerializeObject(telemetryDataPoint);
        }
    }


    // INSERT EnvironmentSensor class below here
    /// <summary>
    /// This class represents a sensor 
    /// real-world sensors would contain code to initialize
    /// the device or devices and maintain internal state
    /// a real-world example can be found here: https://bit.ly/IoT-BME280
    /// </summary>
    internal class EnvironmentSensor
    {
        // Initial telemetry values
        double minTemperature = 20;
        double minHumidity = 60;
        Random rand = new Random();

        internal EnvironmentSensor()
        {
            // device initialization could occur here
        }

        internal double ReadTemperature()
        {
            return minTemperature + rand.NextDouble() * 15;
        }

        internal double ReadHumidity()
        {
            return minHumidity + rand.NextDouble() * 20;
        }
    }
}