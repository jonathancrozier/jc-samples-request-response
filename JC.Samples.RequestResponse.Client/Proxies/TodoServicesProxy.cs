using JC.Samples.RequestResponse.Client.Clients;
using JC.Samples.RequestResponse.Shared.Interfaces;
using JC.Samples.RequestResponse.Shared.Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace JC.Samples.RequestResponse.Client.Proxies
{
    /// <summary>
    /// Proxies Todo service calls.
    /// </summary>
    public class TodoServicesProxy : ITodoServices
    {
        #region Readonlys

        private readonly JsonRpcClient _client;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">The JSON-RPC client</param>
        public TodoServicesProxy(JsonRpcClient client)
        {
            _client = client;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a collection of Todos.
        /// </summary>
        /// <param name="userId">The ID of the User to get Todos for (optional)</param>
        /// <returns>A collection of all available Todos</returns>
        public IEnumerable<Todo> GetTodos(int? userId = null)
        {
            Log.Debug($"Getting Todos");

            var request  = _client.CreateRequest("getTodos", new { userId });
            var response = _client.SendRequest<IEnumerable<Todo>>(request);

            Log.Debug($"Found {response.Count()} Todos");

            return response;
        }

        #endregion
    }
}