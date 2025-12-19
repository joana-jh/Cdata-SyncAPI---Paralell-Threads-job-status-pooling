using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


class Program
{
    // Basic Auth
    private static readonly string username = "MyUser";
    private static readonly string password = "MyPass";


    private static readonly HttpClient client = new HttpClient();
    private static readonly string outputFolderPath = "C:\\MyOutputFolder\\";
    private static readonly int delayMs = 1; // set to 1ms or 0 for max speed
    private static readonly string url = "http://localhost:8181/api.rsc/getJobStatus";  // API endpoint

    private static String[] listOfJobNames = new string[] {
    "j1",
    "j2",
    "j3",
    "j4",
    "j5",
    "j6"
    };


    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting parallel request workers...");


        foreach (var directory in listOfJobNames)
        {
            String directoryFolder = outputFolderPath + directory;
            // Create folder if missing
            if (!Directory.Exists(directoryFolder))
                Directory.CreateDirectory(directoryFolder);
        }


        // Set Basic Auth ONCE
        var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));


        // Start parallel workers
        int parallelWorkers = listOfJobNames.Length;
        var tasks = new Task[parallelWorkers];
        Console.WriteLine("Starting request workers...");

        for (int i = 0; i < parallelWorkers; i++)
        {
            int workerId = i + 1;
            tasks[i] = Task.Run(() => WorkerLoop(workerId));
        }

        await Task.WhenAll(tasks);
    }

    static async Task WorkerLoop(int workerId)
    {
        Console.WriteLine($"Worker {workerId} started");

        while (true)
        {
            await SendRequestAndSave(workerId);

            if (delayMs > 0)
                await Task.Delay(delayMs);
        }
    }

    static async Task SendRequestAndSave(int workerId)
    {
        try
        {
            String jobName = listOfJobNames[workerId - 1];


            var content = new StringContent(@$"{{  ""JobName"":""{jobName}""   }}", Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            string responseContent = await response.Content.ReadAsStringAsync();

            // Try to pretty-format JSON (falls back to plain text if not JSON)
            string responseContentFormatted;
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                responseContentFormatted = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch
            {
                // Not valid JSON → save raw text
                responseContentFormatted = responseContent;
            }


            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            string filePath = Path.Combine($"{outputFolderPath}{jobName}", $"resp_{timestamp}.json");

            await File.WriteAllTextAsync(filePath, responseContentFormatted);

            Console.WriteLine($"Saved → {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
