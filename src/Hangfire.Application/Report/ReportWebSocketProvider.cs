using System;
using System.Net.WebSockets;

namespace Hangfire.Application.Report;

//public delegate void FrontendReportHandler(object sender, Evente);

public class ReportWebSocketProvider
{
    public Dictionary<string, WebSocket> webSockets = new Dictionary<string, WebSocket>();
    public Dictionary<string, EventHandler<string>> events = new Dictionary<string, EventHandler<string>>();
}
