using System;
using System.Diagnostics;

namespace Hangfire.Application.VideoDownloader;

public interface IVideoDownloader {
    public void EnqueueVideoDownload(string videoUrl);
}
public class VideoDownloader : IVideoDownloader
{
    public void EnqueueVideoDownload(string videoUrl)
    {
        BackgroundJob.Enqueue(() => 
            DownloadProcess(videoUrl)
        );
    }

    public void DownloadProcess(string videoUrl) {
        Console.WriteLine($"Enqueueing video with url: {videoUrl}");
        var downloadProcess = new Process();
        downloadProcess.StartInfo.FileName = "yt-dlp.exe";
        downloadProcess.StartInfo.Arguments = videoUrl;

        downloadProcess.Start();

        
        downloadProcess.WaitForExit();
        
    }
}
