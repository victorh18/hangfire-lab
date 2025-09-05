using System.Net.WebSockets;
using System.Text;
using Hangfire.Application.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ReportWebSocketProvider _webSocketProvider;

        public Dictionary<string, EventHandler> events = new();

        public ReportController(ReportWebSocketProvider webSocketProvider)
        {
            _webSocketProvider = webSocketProvider;
        }

        [Route("frontend/{id}")]
        public async Task FrontendWs(string id)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                _webSocketProvider.events.Add(id, HandleFrontendWebSocket(id, webSocket));
                while (true) { }
            }

            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        [Route("{id}")]
        public async Task Ws(string id)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _webSocketProvider.webSockets.Add(id, webSocket);
                await HandleWebSocket(id, webSocket);
                // await KeepWebSocketOpen(id, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task HandleWebSocket(string id, WebSocket webSocket)
        {
            var buffer = new byte[1];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var text = "";

            while (!receiveResult.CloseStatus.HasValue)
            {
                if (buffer[0] == 0)
                {
                    receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    continue;
                }

                text += Encoding.UTF8.GetString(buffer);
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.EndOfMessage)
                {
                    text += Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"Message received from the {id} connection: {text}");
                    var _event = _webSocketProvider.events.GetValueOrDefault(id);
                    _event?.Invoke(this, text);

                    text = "";
                    buffer[0] = 0;
                    Console.WriteLine("message finished");
                }
            }

            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
            _webSocketProvider.webSockets.Remove(id);
        }

        // private async Task KeepWebSocketOpen(string id, WebSocket webSocket)
        // {
        //     // BEWARE WITH THIS BECAUSE, UNLESS YOU KEEP READING THE MESSAGES, YOU WILL NOT GET THE END 
        //     // OF CONNECTION FRAME, AND WILL NEVER BE ABLE TO DETECT THE CLOSURE OF THE WS
        //     while ((webSocket.State != WebSocketState.CloseReceived)) { }

        //     await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
        //     _webSocketProvider.webSockets.Remove(id);
        // }

        private EventHandler<string> HandleFrontendWebSocket(string id, WebSocket frontendWebSocket)
        {
            return (sender, message) =>
            {
                var messageBits = Encoding.UTF8.GetBytes(message);
                frontendWebSocket.SendAsync(messageBits, WebSocketMessageType.Text, true, CancellationToken.None);
            };
        }
    }
}
