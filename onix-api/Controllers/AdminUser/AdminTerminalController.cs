using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using k8s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Its.Onix.Api.Controllers
{
    // Web shell into the "terminal" pod, same pattern as please-protect-api's TerminalController.
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminTerminalController : ControllerBase
    {
        private const string TerminalNamespace = "terminal";
        private const string TerminalPodLabelSelector = "app=terminal";

        [ExcludeFromCodeCoverage]
        public AdminTerminalController()
        {
        }

        [ExcludeFromCodeCoverage]
        private async Task HandleTerminal(WebSocket clientSocket, CancellationToken requestAborted)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(requestAborted);

            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                using var k8sClient = new Kubernetes(config);

                var pods = await k8sClient.CoreV1.ListNamespacedPodAsync(
                    namespaceParameter: TerminalNamespace,
                    labelSelector: TerminalPodLabelSelector,
                    cancellationToken: cts.Token);

                var pod = pods.Items.FirstOrDefault(p => p.Status.Phase == "Running");
                if (pod == null)
                {
                    await CloseWithError(clientSocket, $"No running pod found in namespace '{TerminalNamespace}' with label '{TerminalPodLabelSelector}'.");
                    return;
                }

                using var k8sStream = await k8sClient.WebSocketNamespacedPodExecAsync(
                    pod.Metadata.Name,
                    TerminalNamespace,
                    command: new[] { "/bin/bash", "-i" },
                    container: null,
                    tty: true,
                    stdin: true,
                    stdout: true,
                    stderr: true,
                    cancellationToken: cts.Token);

                // "Wake up" the shell/PTY by sending an initial resize (channel 4).
                var resizeMsg = "{\"Width\":120,\"Height\":40}";
                var resizeBuffer = new byte[resizeMsg.Length + 1];
                resizeBuffer[0] = 4;
                Encoding.UTF8.GetBytes(resizeMsg, 0, resizeMsg.Length, resizeBuffer, 1);
                await k8sStream.SendAsync(new ArraySegment<byte>(resizeBuffer), WebSocketMessageType.Binary, true, cts.Token);

                var clientToK8s = Task.Run(async () =>
                {
                    var buffer = new byte[8192];
                    try
                    {
                        while (clientSocket.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                        {
                            var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close) break;
                            if (result.Count <= 0) continue;

                            var k8sIn = new byte[result.Count + 1];
                            k8sIn[0] = 0; // channel 0: stdin
                            Array.Copy(buffer, 0, k8sIn, 1, result.Count);
                            await k8sStream.SendAsync(new ArraySegment<byte>(k8sIn), WebSocketMessageType.Binary, true, cts.Token);
                        }
                    }
                    catch (OperationCanceledException) { /* expected on shutdown */ }
                }, cts.Token);

                var k8sToClient = Task.Run(async () =>
                {
                    var buffer = new byte[8192];
                    try
                    {
                        while (k8sStream.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                        {
                            var result = await k8sStream.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close) break;
                            if (result.Count <= 1) continue;

                            var channel = buffer[0];
                            if (channel == 1 || channel == 2) // stdout / stderr
                            {
                                await clientSocket.SendAsync(
                                    new ArraySegment<byte>(buffer, 1, result.Count - 1),
                                    WebSocketMessageType.Text,
                                    true,
                                    cts.Token);
                            }
                        }
                    }
                    catch (OperationCanceledException) { /* expected on shutdown */ }
                }, cts.Token);

                await Task.WhenAny(clientToK8s, k8sToClient);
            }
            catch (Exception ex)
            {
                await CloseWithError(clientSocket, ex.Message);
                return;
            }
            finally
            {
                cts.Cancel();
                if (clientSocket.State == WebSocketState.Open)
                {
                    await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            }
        }

        [ExcludeFromCodeCoverage]
        private static async Task CloseWithError(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open) return;
            var bytes = Encoding.UTF8.GetBytes(message);
            try
            {
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch { /* best-effort */ }
            await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, message, CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/Connect")]
        public async Task Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var clientSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleTerminal(clientSocket, HttpContext.RequestAborted);
        }
    }
}
