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

        Console.WriteLine($"Calling process: {downloadProcess.StartInfo.FileName} {downloadProcess.StartInfo.Arguments}");

        downloadProcess.Start();


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
}
