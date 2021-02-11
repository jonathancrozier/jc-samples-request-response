using AustinHarris.JsonRpc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace JC.Samples.RequestResponse.Client.Clients
{
    /// <summary>
    /// Provides a means of calling JSON-RPC endpoints over a WebSocket connection.
    /// </summary>
    public class JsonRpcClient
    {
        #region Properties

        /// <summary>
        /// Allows the maximum request ID value to be configured.
        /// </summary>
        public int MaximumRequestId { get; set; } = int.MaxValue;

        #endregion

        #region Static Fields

        /// <summary>
        /// Used to keep track of the current request ID.
        /// </summary>
        private static int _requestId = 0;

        #endregion

        #region Static Readonlys

        /// <summary>
        /// Used to keep track of server responses.
        /// </summary>
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _responses
            = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        #endregion

        #region Readonlys

        private readonly WebSocket _webSocket;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="webSocket">The WebSocket channel to use</param>
        public JsonRpcClient(WebSocket webSocket)
        {
            _webSocket            = webSocket;
            _webSocket.OnMessage += ProcessMessage;
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Creates a JSON-RPC request for the specified method and parameters.
        /// </summary>
        /// <param name="method">The method name</param>
        /// <param name="parameters">The list of parameters to pass to the method</param>
        /// <returns><see cref="JsonRequest"/></returns>
        public JsonRequest CreateRequest(string method, object parameters)
        {
            // Get the next available Request ID.
            int nextRequestId = Interlocked.Increment(ref _requestId);

            if (nextRequestId > MaximumRequestId)
            {
                // Reset the Request ID to 0 and start again.
                Interlocked.Exchange(ref _requestId, 0);

                nextRequestId = Interlocked.Increment(ref _requestId);
            }

            // Create and return the Request object.
            var request = new JsonRequest(method, parameters, nextRequestId);

            return request;
        }

        /// <summary>
        /// Sends the specified request to the WebSocket server and gets the response.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result object</typeparam>
        /// <param name="request">The JSON-RPC request to send</param>
        /// <param name="timeout">The timeout (in milliseconds) for the request</param>
        /// <returns>The response result</returns>
        public TResult SendRequest<TResult>(JsonRequest request, int timeout = 30000)
        {
            var tcs       = new TaskCompletionSource<string>();
            var requestId = request.Id;

            try
            {
                string requestString = JsonConvert.SerializeObject(request);

                // Add the Request details to the Responses dictionary so that we have   
                // an entry to match up against whenever the response is received.
                _responses.TryAdd(Convert.ToString(requestId), tcs);

                // Send the request to the server.
                Log.Verbose($"Sending request: {requestString}");
                _webSocket.Send(requestString);
                Log.Verbose("Finished sending request");

                var task = tcs.Task;

                // Wait here until either the response has been received,
                // or we have reached the timeout limit.
                Task.WaitAll(new Task[] { task }, timeout);
                
                if (task.IsCompleted)
                {
                    // Parse the result, now that the response has been received.
                    JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(task.Result);

                    string responseString = JsonConvert.SerializeObject(response);
                    Log.Verbose($"Received response: {responseString}");

                    // Throw an Exception if there was an error.
                    if (response.Error != null) throw response.Error;

                    // Return the result.
                    return JsonConvert.DeserializeObject<TResult>(
                        Convert.ToString(response.Result),
                        new JsonSerializerSettings 
                        { 
                            Error = (sender, args) => args.ErrorContext.Handled = true
                        });
                }
                else // Timeout response.
                {
                    Log.Error($"Client timeout of {timeout} milliseconds has expired, throwing TimeoutException");
                    throw new TimeoutException();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred.");
                throw;
            }
            finally
            {
                // Remove the request/response entry in the 'finally' block to avoid leaking memory.
                _responses.TryRemove(Convert.ToString(requestId), out tcs);
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Processes messages received over the WebSocket connection.
        /// </summary>
        /// <param name="sender">The sender (WebSocket)</param>
        /// <param name="e">The Message Event Arguments</param>
        private void ProcessMessage(object sender, MessageEventArgs e)
        {
            // Check for Pings.
            if (e.IsPing)
            {
                Log.Verbose("Received Ping");
                return;
            }

            Log.Debug("Processing message");

            // Log when the message is Binary.
            if (e.IsBinary)
            {
                Log.Verbose("Message Type is Binary");
            }

            Log.Verbose($"Message Data: {e.Data}");

            // Parse the response from the server.
            JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(
                e.Data,
                new JsonSerializerSettings 
                { 
                    Error = (sender, args) => args.ErrorContext.Handled = true 
                });

            // Check for an error.
            if (response.Error != null)
            {
                // Log the error details.
                Log.Error("Error Message: " + response.Error.message);
                Log.Error("Error Code: "    + response.Error.code);
                Log.Verbose("Error Data: "  + response.Error.data);
            }

            // Set the response result.
            if (_responses.TryGetValue(Convert.ToString(response.Id), out TaskCompletionSource<string> tcs))
            {
                tcs.TrySetResult(e.Data);
            }
            else
            {
                Log.Error("Unexpected response received. ID: " + response.Id);
            }

            Log.Debug("Finished processing message");
        }

        #endregion

        #endregion
    }
}