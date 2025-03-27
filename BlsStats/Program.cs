using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BlsStats
{
    public class Program
    {
        public static async Task Main()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string apiKey = configuration["API_KEY"] ?? throw new Exception("API key not found");

            // BLS API endpoint for retrieving time series data
            string url = "https://api.bls.gov/publicAPI/v2/timeseries/data/";

            // Define the payload with the API key, series IDs, and desired time range
            var payload = new
            {
                registrationKey = apiKey,
                seriesid = new string[] { "SMU18000000000000001" },
                startyear = "2023",
                endyear = "2025"
            };

            // Serialize the payload to JSON
            string jsonPayload = JsonSerializer.Serialize(payload);

            using HttpClient client = new();
            // Create the content for the POST request
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request
                HttpResponseMessage httpResponse = await client.PostAsync(url, content);

                // Check if the request was successful
                if (httpResponse.IsSuccessStatusCode)
                {
                    // Read the JSON response as a string
                    string responseContent = await httpResponse.Content.ReadAsStringAsync();

                    // Deserialize the JSON string into the ApiResponse object
                    ApiResponse response = JsonSerializer.Deserialize<ApiResponse>(responseContent);

                    // Output some data to verify parsing
                    Console.WriteLine($"Status: {response.status}");
                    Console.WriteLine($"Response Time: {response.responseTime}");

                    Console.WriteLine(responseContent);

                    foreach (var series in response.Results.series)
                    {
                        Console.WriteLine($"\nSeries ID: {series.seriesID}");
                        foreach (var data in series.data)
                        {
                            Console.WriteLine($"Year: {data.year}, Month: {data.periodName}, Value: {data.value}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {httpResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(ex.Message);
            }
        }

        public class ApiResponse
        {
            public string status { get; set; }
            public int responseTime { get; set; }
            public List<string> message { get; set; }
            public Results Results { get; set; }
        }

        public class Results
        {
            public List<Series> series { get; set; }
        }

        public class Series
        {
            public string seriesID { get; set; }
            public List<DataPoint> data { get; set; }
        }

        public class DataPoint
        {
            public string year { get; set; }
            public string period { get; set; }
            public string periodName { get; set; }
            public string value { get; set; }
            public List<Dictionary<string, object>> footnotes { get; set; }
        }
    }
}
