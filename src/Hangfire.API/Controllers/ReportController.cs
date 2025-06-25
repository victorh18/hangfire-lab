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

        public ReportController(ReportWebSocketProvider webSocketProvider)
        {
            _webSocketProvider = webSocketProvider;
        }

        [Route("{id}")]
        public async Task Ws(string id)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _webSocketProvider.webSockets.Add(id, webSocket);
                await HandleWebSocket(id, webSocket);
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
                    text = "";
                    buffer[0] = 0;
                    Console.WriteLine("message finished");
                }
            }

            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
            _webSocketProvider.webSockets.Remove(id);
        }
    }
}
