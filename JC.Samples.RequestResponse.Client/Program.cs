using JC.Samples.RequestResponse.Client.Clients;
using JC.Samples.RequestResponse.Client.Proxies;
using Serilog;
using System;
using WebSocketSharp;

namespace JC.Samples.RequestResponse.Client
{
    /// <summary>
    /// Main Program class.
    /// </summary>
    class Program
    {
        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command-line arguments</param>
        static void Main(string[] args)
        {
            // Configure logging.
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.Console()
               .CreateLogger();

            // Connect to the WebSocket server.
            using var webSocket = new WebSocket("ws://localhost:4649/json-rpc");
            webSocket.Connect();

            // Create the JSON-RPC client.
            var client = new JsonRpcClient(webSocket);

            // Send the request and get the response.
            var proxy = new TodoServicesProxy(client);
            var todos = proxy.GetTodos(userId: 2);

            // Keep the client running until the user presses a key.
            Console.ReadKey(true);
        }

        #endregion
    }
}