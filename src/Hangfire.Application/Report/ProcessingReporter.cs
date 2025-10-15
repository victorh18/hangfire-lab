using System;

namespace Hangfire.Application.Report;

public class ProcessingReporter
{
    private decimal videoFPS = 0;
    private int processProgress = 0;
    private decimal secondsToProcess = 0;
    private decimal totalFramesToProcess = 0;
    private readonly Dictionary<string, ReportingFunction> reportMapping = new();

    private (string type, int value) StartProcessing(string text)
    {
        var fps = text?.Split(",")?.FirstOrDefault(t => t.Contains("fps"))?.Trim().Split(" ")[0];
        videoFPS = decimal.Parse(fps ?? "0");
        totalFramesToProcess = (int)(videoFPS * secondsToProcess);
        return ("Processing...", 0);
    }

    private (string type, int value) Processing(string text)
    {
        var currentFrame = decimal.Parse(text.Split("=")[1].Trim().Split(' ')[0].Trim());
        processProgress = (int)(currentFrame / totalFramesToProcess * 100);
        return ("Processing...", processProgress);
    }

    private (string type, int value) EndProcessing(string text)
    {
        return ("Processing...", 100);
    }
    public ProcessingReporter()
    {
        reportMapping.Add(@"Stream #0:0[0x1](und): Video", StartProcessing);
        reportMapping.Add(@"frame=", Processing);
        reportMapping.Add(@"progress=end", EndProcessing);
    }

    public ProcessingReporter(decimal _secondToProcess) : this()
    {
        secondsToProcess = _secondToProcess;
    }

    public (string type, int value) GetProgress(string text)
    {
        var toExecute = reportMapping.Where(keyPair => text.Contains(keyPair.Key)).FirstOrDefault().Value;
        var result = toExecute?.Invoke(text) ?? ("Processing...", processProgress);
        return result;
    }
}
