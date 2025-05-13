using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Hangfire.Application.VideoDownloader;

public interface IVideoDownloader
{
    public void EnqueueVideoDownload(string id, string videoUrl);
}
public class VideoDownloader : IVideoDownloader
{
    private int fragmentDownloaded = 0;
    private int progress = 0;
    public void EnqueueVideoDownload(string id, string videoUrl)
    {
        var downloadJobId = BackgroundJob.Enqueue(() =>
            DownloadProcess(id, videoUrl)
        );

        BackgroundJob.ContinueJobWith(downloadJobId, () => ProcessDownload(id));
        // BackgroundJob.Enqueue(() => ProcessDownload(id));
    }

    public void DownloadProcess(string id, string videoUrl)
    {
        Console.WriteLine($"Enqueueing video with id: {id} url: {videoUrl}");
        Process downloadProcess = new();
        List<string> argumentList = new();

        downloadProcess.StartInfo.FileName = "yt-dlp";
        argumentList.Add(videoUrl);
        argumentList.Add($"-o {id}.mp4");

        downloadProcess.StartInfo.Arguments = string.Join(' ', argumentList);
        downloadProcess.StartInfo.UseShellExecute = false;
        downloadProcess.StartInfo.RedirectStandardOutput = true;
        downloadProcess.OutputDataReceived += DownloadProcessOutputHandler;

        Console.WriteLine($"Calling process: {downloadProcess.StartInfo.FileName} {downloadProcess.StartInfo.Arguments}");

        downloadProcess.Start();

        downloadProcess.BeginOutputReadLine();


        downloadProcess.WaitForExit();

    }

    public void ProcessDownload(string id)
    {
        Console.WriteLine($"Processing video with id: {id}");

        Process processingProcess = new();
        List<string> argumentList = new();

        processingProcess.StartInfo.FileName = "ffmpeg";
        argumentList.Add($@"-i /Users/Videlarosa/Projects/personal/hangfire-lab/src/Hangfire.API/{id}.mp4");
        argumentList.Add(@"-ss 00:00:05");
        argumentList.Add(@"-to 00:00:10");
        argumentList.Add($@"{id}.gif");

        processingProcess.StartInfo.Arguments = string.Join(' ', argumentList);

        Console.WriteLine($"Calling process: {processingProcess.StartInfo.FileName} {processingProcess.StartInfo.Arguments}");

        processingProcess.Start();
        processingProcess.WaitForExit();
    }

    public (string description, int percentage) GetProgressPercentage(string text)
    {
        var downloadBasePercentage = fragmentDownloaded == 1 ? 10 : 50;
        if (text.Contains("Downloading m3u8 information"))
        {
            progress = 5;
        }
        else if (text.Contains("Downloading m3u8 manifest"))
        {
            progress = downloadBasePercentage;
        }
        else if (text.Contains("[download]") && !text.Contains("(frag 0"))
        {
            if (text.Contains("Destination:"))
            {
                fragmentDownloaded++;
            }
            var parts = text.Split(" ");
            var possiblePercentage = parts?.FirstOrDefault(s => s.Contains("%"))?.Replace("%", "");

            if (decimal.TryParse(possiblePercentage, out decimal actualPercentage))
            {
                progress = downloadBasePercentage + (int)Math.Floor(actualPercentage / 2);
            }
            else
            {
                return ("Weird text: " + text, 8);
            }
        }

        return ("Downloading...", progress);

    }

    public void DownloadProcessOutputHandler(object downloadingProcess, DataReceivedEventArgs outline)
    {
        var progressReport = GetProgressPercentage(outline.Data ?? "");
        Console.WriteLine($"{progressReport.description} {progressReport.percentage}");
    }
}
