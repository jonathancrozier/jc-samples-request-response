using AustinHarris.JsonRpc;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace JC.Samples.RequestResponse.Server.Behaviors
{
    /// <summary>
    /// WebSocket behaviour for handling JSON-RPC requests.
    /// </summary>
    public class JsonRpc : WebSocketBehavior
    {
        /// <summary>
        /// Called when the WebSocket used in a session receives a message.
        /// </summary>
        /// <param name="e">Represents the event data passed to the OnMessage event</param>
        protected override void OnMessage(MessageEventArgs e)
        {
            Serilog.Log.Debug("Processing message");

            // Log when the message is Binary.
            if (e.IsBinary)
            {
                Serilog.Log.Verbose("Message Type is Binary");
            }

            Serilog.Log.Verbose($"Message Data: {e.Data}");

            // Configure the response behaviour for when the RPC method completes.
            var asyncState = new JsonRpcStateAsync(ar =>
            {
                // Get the JSON response from the RPC method.
                string responseString = ((JsonRpcStateAsync)ar).Result;

                // There will be no response to send for Notifications.
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    // Send the JSON response back to the server.
                    Serilog.Log.Verbose($"Sending response: {responseString}");
                    Send(responseString);
                }

                Serilog.Log.Debug("Finished processing message");
            },
            null);

            // Set the JSON which represents the RPC method call.
            asyncState.JsonRpc = e.Data;

            // Execute the RPC method call asynchronously.
            JsonRpcProcessor.Process(asyncState);
        }
    }
}
