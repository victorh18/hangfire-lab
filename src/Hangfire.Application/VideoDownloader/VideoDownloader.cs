using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Hangfire.Application.Common;
using Hangfire.Application.Config;
using Microsoft.Extensions.Options;

namespace Hangfire.Application.VideoDownloader;

public interface IVideoDownloader
{
    public void EnqueueVideoDownload(string id, string videoUrl, string startTime, string endTime, ExtractionType extractionType);
}
public class VideoDownloader : IVideoDownloader
{

    private readonly ClientWebSocket _webSocketClient = new();

    private int fragmentDownloaded = 0;
    private int progress = 0;

    private decimal videoFPS = 0;
    private int processProgress = 0;
    private decimal secondsToProcess = 0;
    private decimal totalFramesToProcess = 0;

    private readonly IOptions<AppSettings> _options;

    public VideoDownloader(IOptions<AppSettings> options)
    {
        _options = options;
    }

    public void EnqueueVideoDownload(string id, string videoUrl, string startTime, string endTime, ExtractionType extractionType)
    {
        var downloadJobId = BackgroundJob.Enqueue(() =>
            DownloadProcess(id, videoUrl, extractionType)
        );

        BackgroundJob.ContinueJobWith(downloadJobId, () => ProcessDownload(id, startTime, endTime, extractionType));
    }

    public void DownloadProcess(string id, string videoUrl, ExtractionType extractionType)
    {
        Console.WriteLine($"Enqueueing video with id: {id} url: {videoUrl}");

        Console.WriteLine("Connecting to web socket...");
        Uri _webSocketUrl = new($"{_options.Value.HostUrls.InternalWebSocket}/api/report/{id}");
        Task.WaitAll([_webSocketClient.ConnectAsync(_webSocketUrl, CancellationToken.None)]);
        Console.WriteLine("Connected!");

        Process downloadProcess = new();
        List<string> argumentList = new();

        downloadProcess.StartInfo.FileName = "yt-dlp";
        argumentList.Add(videoUrl);
        List<string> additionalArguments = GetDownloadAdditionalArgs(extractionType, id);
        argumentList.AddRange(additionalArguments);

        downloadProcess.StartInfo.Arguments = string.Join(' ', argumentList);
        downloadProcess.StartInfo.UseShellExecute = false;
        downloadProcess.StartInfo.RedirectStandardOutput = true;
        downloadProcess.OutputDataReceived += DownloadProcessOutputHandler;

        Console.WriteLine($"Calling process: {downloadProcess.StartInfo.FileName} {downloadProcess.StartInfo.Arguments}");

        downloadProcess.Start();

        downloadProcess.BeginOutputReadLine();

        downloadProcess.WaitForExit();
        Task.WaitAll([_webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None)]);

    }

    public void ProcessDownload(string id, string startTime, string endTime, ExtractionType extractionType)
    {
        Console.WriteLine($"Processing video with id: {id}");

        Console.WriteLine("Connecting to web socket...");
        Uri _webSocketUrl = new($"{_options.Value.HostUrls.InternalWebSocket}/api/report/{id}");
        Task.WaitAll([_webSocketClient.ConnectAsync(_webSocketUrl, CancellationToken.None)]);
        Console.WriteLine("Connected!");

        Process processingProcess = new();
        List<string> argumentList = new();

        //var path = Environment.OSVersion.Platform.ToString() == "Win32NT" ? @"D:\" + Path.Combine("Projects", "labs") : @"/" + Path.Combine("Users", "Videlarosa", "Projects", "personal");
        var path = _options.Value.FilePaths.DownloadPath;
        secondsToProcess = GetIntervalLength(startTime, endTime);

        processingProcess.StartInfo.FileName = "ffmpeg";
        var fileName = GetDownloadFileName(extractionType, id);
        argumentList.Add($@"-i {path}/{fileName}");

        List<string> additionalArguments = GetProcessAdditionalArgs(extractionType, id, startTime, endTime);
        argumentList.AddRange(additionalArguments);

        processingProcess.StartInfo.Arguments = string.Join(' ', argumentList);

        processingProcess.StartInfo.UseShellExecute = false;
        processingProcess.StartInfo.RedirectStandardOutput = true;
        processingProcess.StartInfo.RedirectStandardError = true;
        processingProcess.OutputDataReceived += ProcessProcessOutputHandler;
        processingProcess.ErrorDataReceived += ProcessProcessOutputHandler;

        Console.WriteLine($"Calling process: {processingProcess.StartInfo.FileName} {processingProcess.StartInfo.Arguments}");

        processingProcess.Start();
        processingProcess.BeginOutputReadLine();
        processingProcess.BeginErrorReadLine();
        processingProcess.WaitForExit();
        Task.WaitAll([_webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None)]);


        // For the processing progress, get the video fps, and then the duration, and use that to determine the progress, also consider using the minimized version (-progress - -nostats) to make it easier to process 
    }

    public (string description, int percentage) GetProgressPercentage(string text)
    {

        if (text.Contains("Downloading m3u8 information"))
        {
            progress = 5;
        }
        else if (text.Contains("Downloading m3u8 manifest"))
        {
            progress = 9;
        }
        else if (text.Contains("[download]") && !text.Contains("(frag 0"))
        {
            if (text.Contains("Destination:"))
            {
                fragmentDownloaded++;
            }
            var downloadBasePercentage = fragmentDownloaded == 1 ? 10 : 50;
            var parts = text.Split(" ");
            var possiblePercentage = parts?.FirstOrDefault(s => s.Contains("%"))?.Replace("%", "");

            if (decimal.TryParse(possiblePercentage, out decimal actualPercentage))
            {
                progress = Math.Max(progress, downloadBasePercentage + (int)Math.Floor(actualPercentage / 2));
            }
            else
            {
                return ("Weird text: " + text, 8);
            }
        }

        var messageToWs = Encoding.UTF8.GetBytes($"Downloading: {progress}");
        _webSocketClient.SendAsync(messageToWs, WebSocketMessageType.Text, true, CancellationToken.None);

        return ("Downloading...", progress);

    }

    public (string description, int percentage) GetProcessPercentage(string text)
    {
        if (text.Contains(@"Stream #0:0[0x1](und): Video"))
        {
            var fps = text?.Split(",")?.FirstOrDefault(t => t.Contains("fps"))?.Trim().Split(" ")[0];
            videoFPS = decimal.Parse(fps ?? "0");
            totalFramesToProcess = (int)(videoFPS * secondsToProcess);
        }

        if (text.Contains("frame=") && !text.Contains(" "))
        {
            var currentFrame = decimal.Parse(text.Split("=")[1]);
            processProgress = (int)(currentFrame / totalFramesToProcess * 100);
        }

        if (text.Contains("progress=end"))
        {
            processProgress = 100;
        }

        var messageToWs = Encoding.UTF8.GetBytes($"Processing: {processProgress}");
        _webSocketClient.SendAsync(messageToWs, WebSocketMessageType.Text, true, CancellationToken.None);

        return ("Processing", processProgress);
    }

    public void DownloadProcessOutputHandler(object downloadingProcess, DataReceivedEventArgs outline)
    {
        var progressReport = GetProgressPercentage(outline.Data ?? "");
        Console.WriteLine($"{progressReport.description} {progressReport.percentage}");
    }

    public void ProcessProcessOutputHandler(object processProcess, DataReceivedEventArgs outline)
    {
        var processReport = GetProcessPercentage(outline.Data ?? "");
        Console.WriteLine($"{processReport.description} {processReport.percentage}");
    }

    public List<string> GetGIFProcessingArgs(string id, string startTime, string endTime)
    {
        var basePath = _options.Value.FilePaths.ResultsPath;
        return new List<string>
        {
            @$"-ss {startTime}",
            @$"-to {endTime}",
            @"-progress - -nostats",
            $@"{basePath}/{id}.gif"
        };
    }

    public List<string> GetAudioProcessingArgs(string id, string startTime, string endTime)
    {
        var basePath = _options.Value.FilePaths.ResultsPath;
        return new List<string>
        {
            @$"-ss {startTime}",
            @$"-to {endTime}",
            @"-progress - -nostats",
            $@"{basePath}/{id}.mp3"
        };
    }

    public List<string> GetGIFDownloadArgs(string id)
    {
        return new()
        {
            "--remux-video mp4",
            $"-o {id}.mp4"
        };
    }

    public List<string> GetAudioDownloadArgs(string id)
    {
        return new()
        {
            "-x",
            "--audio-format mp3",
            $"-o {id}_audio.mp3"
        };
    }

    private List<string> GetDownloadAdditionalArgs(ExtractionType extractionType, string id)
    {
        return extractionType == ExtractionType.AUDIO ? GetAudioDownloadArgs(id) : GetGIFDownloadArgs(id);
    }

    private List<string> GetProcessAdditionalArgs(ExtractionType extractionType, string id, string startTime, string endTime)
    {
        return extractionType == ExtractionType.AUDIO ? GetAudioProcessingArgs(id, startTime, endTime) : GetGIFProcessingArgs(id, startTime, endTime);
    }

    private string GetDownloadFileName(ExtractionType extractionType, string id)
    {
        if (extractionType == ExtractionType.AUDIO)
            return $"{id}_audio.mp3";

        return $"{id}.mp4";
    }

    private decimal GetSecondsFromFormat(string time)
    {
        var sections = time.Split(":").Select(decimal.Parse).ToArray();
        var seconds = (sections[0] * 3600) + (sections[1] * 60) + sections[2];

        return seconds;
    }
    private decimal GetIntervalLength(string startTime, string endTime)
    {
        var startSeconds = GetSecondsFromFormat(startTime);
        var endSeconds = GetSecondsFromFormat(endTime);

        return endSeconds - startSeconds;
    }

}
