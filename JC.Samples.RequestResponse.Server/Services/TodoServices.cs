using AustinHarris.JsonRpc;
using JC.Samples.RequestResponse.Shared.Interfaces;
using JC.Samples.RequestResponse.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace JC.Samples.RequestResponse.Server.Services
{
    /// <summary>
    /// Exposes Todo endpoints.
    /// </summary>
    public class TodoServices : JsonRpcService, ITodoServices
    {
        #region Fields

        /// <summary>
        /// Holds an in-memory list of Todo items for simulation purposes.
        /// </summary>
        private static IEnumerable<Todo> _todos = new List<Todo>
            {
                new Todo { Id = 1, Title = "Buy milk", UserId = 1 },
                new Todo { Id = 2, Title = "Leave out the trash", UserId = 2 },
                new Todo { Id = 3, Title = "Clean room", UserId = 2 }
            };

        #endregion

        #region Methods

        /// <summary>
        /// Gets a collection of Todos.
        /// </summary>
        /// <param name="userId">The ID of the User to get Todos for (optional)</param>
        /// <returns>A collection of all available Todos</returns>
        [JsonRpcMethod("getTodos")]
        public IEnumerable<Todo> GetTodos(int? userId = null)
        {
            // Fetch and return Todos from the in-memory list.
            var todos = userId != null && userId > 0 ? _todos.Where(t => t.UserId == userId) : _todos;

            return todos;
        }

        #endregion
    }
}