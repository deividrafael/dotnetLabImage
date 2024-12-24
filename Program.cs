using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        string clientId;
        if (context.Request.Cookies.ContainsKey("ClientId"))
        {
            clientId = context.Request.Cookies["ClientId"] ?? string.Empty;
        }
        else
        {
            clientId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("ClientId", clientId);
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine($"WebSocket connection established with client ID: {clientId}");
        await SendClientIdAsync(webSocket, clientId);
        await EchoMessagesAsync(webSocket, clientId);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
    await next();
});

app.Run();

static async Task SendClientIdAsync(WebSocket webSocket, string clientId)
{
    var clientIdMessage = $"Your client ID is {clientId}";
    var encodedMessage = Encoding.UTF8.GetBytes(clientIdMessage);
    await webSocket.SendAsync(new ArraySegment<byte>(encodedMessage), WebSocketMessageType.Text, true, CancellationToken.None);
}

static async Task EchoMessagesAsync(WebSocket webSocket, string clientId)
{
    var buffer = new byte[1024 * 4];
    while (webSocket.State == WebSocketState.Open)
    {
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (receiveResult.MessageType == WebSocketMessageType.Close)
        {
            Console.WriteLine($"WebSocket connection closed for client ID: {clientId}");
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
        else
        {
            var serverMessage = $"Server message to client {clientId} at {DateTime.UtcNow}";
            var encodedMessage = Encoding.UTF8.GetBytes(serverMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(encodedMessage), WebSocketMessageType.Text, true, CancellationToken.None);
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}