using System;
using System.Net.WebSockets;

namespace Hangfire.Application.Report;

public class ReportWebSocketProvider
{
    public Dictionary<string, WebSocket> webSockets = new Dictionary<string, WebSocket>();
}
