# Parallel Job Status Poller (C#)

## Overview
This C# console application sends **parallel HTTP POST requests** to an API endpoint to retrieve job status information and **saves each response to disk**.  
Each job runs in its own worker loop and continuously polls the API at a configurable interval.

The application is intended for **high-frequency polling**, storing every response with a timestamp for troubleshooting, auditing, or analysis.

---

## Features
- Parallel workers (one per job)
- HTTP Basic Authentication
- Continuous polling loop
- Configurable request delay
- Automatic output folder creation
- JSON pretty-printing (falls back to raw text if response is not JSON)
- Timestamped response files

---

## How It Works
1. A list of job names is defined.
2. One worker task is created per job.
3. Each worker repeatedly:
   - Sends a POST request with `{ "JobName": "<job>" }`
   - Reads the response
   - Formats JSON if possible
   - Saves the response to a job-specific folder
4. The application runs continuously until manually stopped.

---

## Configuration

### API & Authentication
Update these values in the source code:

```csharp
private static readonly string username = "MyUser";
private static readonly string password = "MyPass";
private static readonly string url = "http://localhost:8181/api.rsc/getJobStatus";

```

## Jobs to Monitor
Define the list of jobs that will be polled in parallel.  
Each job runs in its own worker loop and sends requests independently.

```csharp
private static String[] listOfJobNames = new string[] {
    "j1",
    "j2",
    "j3",
    "j4",
    "j5",
    "j6"
};
```

## Output Directory
Specify the base directory where all job response files will be written.
```csharp
private static readonly string outputFolderPath = "C:\\MyOutputFolder\\";
```
Example structure:

```csharp
C:\MyOutputFolder\
 ├── j1\
 │    ├── resp_20240101_120000_123.json
 ├── j2\
 ├── j3\
 ```


## Polling Delay
Controls the delay between consecutive API requests for each worker.

```csharp
private static readonly int delayMs = 1;
```
