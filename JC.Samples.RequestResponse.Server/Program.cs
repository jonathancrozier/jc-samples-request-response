using JC.Samples.RequestResponse.Server.Behaviors;
using JC.Samples.RequestResponse.Server.Services;
using Serilog;
using System;
using WebSocketSharp.Server;

namespace JC.Samples.RequestResponse.Server
{
    /// <summary>
    /// Main Program class.
    /// </summary>
    class Program
    {
        #region Static Readonlys

        private static readonly object[] _jsonRpcservices;

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor.
        /// Registers the JSON RPC 2.0 services.
        /// </summary>
        static Program()
        {
            // Register the services in a static constructor to prevent
            // the compiler 'optimising away' an 'unused' readonly field.
            _jsonRpcservices = new object[] { new TodoServices() };
        }

        #endregion

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

            // Create the WebSocket server and configure it to accept JSON-RPC requests.
            Log.Debug("Starting server");
            var server = new WebSocketServer(4649);
            server.AddWebSocketService<JsonRpc>("/json-rpc");

            // Start the server and keep it running until the user presses a key.
            server.Start();
            Log.Information("Server started");
            Console.ReadKey(true);
        }

        #endregion
    }
}