using Microsoft.AspNetCore.SignalR;

public class PaymentHub : Hub
{
    public async Task JoinPayment(string sessionId)
    {
        Console.WriteLine($"DEBUG1 - [JoinPayment] for sessionId=[{sessionId}]");
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"payment:{sessionId}"
        );
    }
}
