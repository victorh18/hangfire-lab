using System;

namespace Hangfire.Application.Report;

public class DownloadReporter
{
    private readonly Dictionary<List<string>, ReportingFunction> reportMapping = new();
    private int fragmentDownloaded;
    private int progress;

    private (string type, int value) ReportStarting(string text)
    {
        progress = 2;
        return ("Downloading...", progress);
    }
    private (string type, int value) ReportStartDownload(string text)
    {
        fragmentDownloaded++;
        progress = fragmentDownloaded == 1 ? 4 : progress;
        return ("Downloading...", progress);
    }

    private (string type, int value) ReportDownloading(string text)
    {
        var downloadBasePercentage = fragmentDownloaded == 1 ? 10 : 50;
        var parts = text.Split(" ");
        var possiblePercentage = parts?.FirstOrDefault(s => s.Contains("%"))?.Replace("%", "");

        if (decimal.TryParse(possiblePercentage, out decimal actualPercentage))
        {
            progress = Math.Max(progress, downloadBasePercentage + (int)Math.Floor(actualPercentage / 2));
        }
        else
        {
            progress = 8;
        }

        return ("Downloading...", progress);
    }

    public DownloadReporter()
    {
        reportMapping.Add(["Downloading webpage"], ReportStarting);
        reportMapping.Add(["[download] ", "of"], ReportDownloading);
        reportMapping.Add(["[download] Destination:"], ReportStartDownload);
    }

    public (string type, int value) GetProgress(string text)
    {
        var toExecute = reportMapping
            .Where(keyPair => keyPair.Key
                .All(value => text.Contains(value)))
            .FirstOrDefault().Value;

        var result = toExecute?.Invoke(text) ?? ("Downloading...", progress);
        return result;
    }
}
