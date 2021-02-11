using JC.Samples.RequestResponse.Shared.Models;
using System.Collections.Generic;

namespace JC.Samples.RequestResponse.Shared.Interfaces
{
    /// <summary>
    /// Todo services interface.
    /// </summary>
    public interface ITodoServices
    {
        #region Methods

        IEnumerable<Todo> GetTodos(int? userId);

        #endregion
    }
}