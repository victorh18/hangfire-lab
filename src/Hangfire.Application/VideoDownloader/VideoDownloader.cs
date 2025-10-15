using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Hangfire.Application.Common;
using Hangfire.Application.Config;
using Hangfire.Application.Report;
using Microsoft.Extensions.Options;

namespace Hangfire.Application.VideoDownloader;

public interface IVideoDownloader
{
    public void EnqueueVideoDownload(string id, string videoUrl, string startTime, string endTime, ExtractionType extractionType);
}
public class VideoDownloader : IVideoDownloader
{
    private decimal secondsToProcess = 0;
    private readonly ClientWebSocket _webSocketClient = new();
    private readonly IOptions<AppSettings> _options;
    private readonly DownloadReporter _downloadReporter = new();
    private ProcessingReporter? _processReporter;

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
        downloadProcess.StartInfo = GetDownloadProcessStartInfo(extractionType, id, videoUrl);
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

        var path = _options.Value.FilePaths.DownloadPath;
        secondsToProcess = GetIntervalLength(startTime, endTime);

        var fileName = GetDownloadFileName(extractionType, id);

        Console.WriteLine("Connecting to web socket...");
        Uri _webSocketUrl = new($"{_options.Value.HostUrls.InternalWebSocket}/api/report/{id}");
        Task.WaitAll([_webSocketClient.ConnectAsync(_webSocketUrl, CancellationToken.None)]);
        Console.WriteLine("Connected!");

        Process processingProcess = new();
        processingProcess.StartInfo = GetProcessingProcessStartInfo(extractionType, id, startTime, endTime, path, fileName);
        processingProcess.OutputDataReceived += ProcessProcessOutputHandler;
        processingProcess.ErrorDataReceived += ProcessProcessOutputHandler;

        Console.WriteLine($"Calling process: {processingProcess.StartInfo.FileName} {processingProcess.StartInfo.Arguments}");
        processingProcess.Start();
        processingProcess.BeginOutputReadLine();
        processingProcess.BeginErrorReadLine();
        processingProcess.WaitForExit();
        Task.WaitAll([_webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None)]);
    }

    public (string description, int percentage) GetProgressPercentage(string text)
    {
        var result = _downloadReporter.GetProgress(text);

        var messageToWs = Encoding.UTF8.GetBytes($"Downloading: {result.value}");
        _webSocketClient.SendAsync(messageToWs, WebSocketMessageType.Text, true, CancellationToken.None);

        return result;
    }

    public (string description, int percentage) GetProcessPercentage(string text)
    {
        if (_processReporter == null)
        {
            _processReporter = new(secondsToProcess);
        }
        var result = _processReporter.GetProgress(text);

        var messageToWs = Encoding.UTF8.GetBytes($"Processing: {result.value}");
        _webSocketClient.SendAsync(messageToWs, WebSocketMessageType.Text, true, CancellationToken.None);

        return result;
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

    private ProcessStartInfo GetDownloadProcessStartInfo(ExtractionType extractionType, string id, string videoUrl)
    {
        List<string> additionalArguments = [videoUrl, .. GetDownloadAdditionalArgs(extractionType, id)];

        return new()
        {
            FileName = "yt-dlp",
            Arguments = string.Join(' ', additionalArguments),
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
    }

    private ProcessStartInfo GetProcessingProcessStartInfo(ExtractionType extractionType, string id, string startTime, string endTime, string path, string fileName)
    {
        var inputArgument = $@"-i {path}/{fileName}";
        List<string> additionalArguments = [inputArgument, .. GetProcessAdditionalArgs(extractionType, id, startTime, endTime)];

        return new()
        {
            FileName = "ffmpeg",
            Arguments = string.Join(' ', additionalArguments),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
    }

}
