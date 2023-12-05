using System;
using System.IO;
using System.Text.Json;

namespace tcpmvc.Services
{
    public class IsServerRunningService
    {
        public static void WriteToJsonFile(bool value)
        {
            string jsonFilePath = "./JsonFile/IsServerRunning.json";
            var jsonObject = new { IsServerRunning = value };

            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string existingJsonString = File.ReadAllText(jsonFilePath);
                    var existingJsonObject = JsonSerializer.Deserialize<ServerStatus>(existingJsonString);
                    if (existingJsonObject != null)
                    {
                        existingJsonObject.IsServerRunning = value;
                        string updatedJsonString = JsonSerializer.Serialize(existingJsonObject);
                        File.WriteAllText(jsonFilePath, updatedJsonString);
                    }
                }
                else
                {
                    string jsonString = JsonSerializer.Serialize(jsonObject);
                    File.WriteAllText(jsonFilePath, jsonString);
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here
                Console.WriteLine($"An error occurred while writing to the JSON file: {ex.Message}");
            }
        }

        public static bool IsServerRunning()
        {
            string jsonFilePath = "./JsonFile/IsServerRunning.json";

            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string jsonString = File.ReadAllText(jsonFilePath);
                    var result = JsonSerializer.Deserialize<ServerStatus>(jsonString);
                    return result?.IsServerRunning ?? false;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here
                Console.WriteLine($"An error occurred while reading from the JSON file: {ex.Message}");
            }

            return false;
        }
    }

    internal class ServerStatus
    {
        public bool IsServerRunning { get; set; }
    }
}