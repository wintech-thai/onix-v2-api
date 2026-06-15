using Microsoft.AspNetCore.SignalR;

public class PaymentHub : Hub
{
    public async Task JoinPayment(string sessionId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"payment:{sessionId}"
        );
    }
}
