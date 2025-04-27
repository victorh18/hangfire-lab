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
        argumentList.Add(@"-ss 00:00:46");
        argumentList.Add(@"-to 00:00:48");
        argumentList.Add($@"{id}.gif");

        processingProcess.StartInfo.Arguments = string.Join(' ', argumentList);

        Console.WriteLine($"Calling process: {processingProcess.StartInfo.FileName} {processingProcess.StartInfo.Arguments}");

        processingProcess.Start();
        processingProcess.WaitForExit();
    }

    public static (string description, int percentage) GetProgressPercentage(string text)
    {
        if (text.Contains("Downloading m3u8 information"))
        {
            return ("Downloading...", 10);
        }
        else if (text.Contains("Downloading m3u8 manifest"))
        {
            return ("Downloading...", 20);
        }
        else if (text.Contains("[download]"))
        {
            var parts = text.Split(' ');
            var possiblePercentage = parts[1].Replace("%", "");

            if (int.TryParse(possiblePercentage, out int actualPercentage))
            {
                if (actualPercentage <= 25)
                {
                    return ("Downloading...", 25);
                }
                else
                {
                    return ("Downloading...", actualPercentage);
                }
            }
            else
            {
                return ("Downloading...", 25);
            }
        }

        return ("Downloading...", 50);

    }

    public void DownloadProcessOutputHandler(object downloadingProcess, DataReceivedEventArgs outline)
    {
        var progressReport = GetProgressPercentage(outline.Data ?? "");
        Console.WriteLine($"{progressReport.description} {progressReport.percentage}");
    }
}
