# Samples - Request-Response Pattern
An implementation of the Request-Response pattern using C# with JSON-RPC and WebSockets.

```csharp
// Connect to the WebSocket server.
using var webSocket = new WebSocket("ws://localhost:4649/json-rpc");
webSocket.Connect();

// Create the JSON-RPC client.
var client = new JsonRpcClient(webSocket);

// Send the request and get the response.
var proxy = new TodoServicesProxy(client);
var todos = proxy.GetTodos(userId: 2);
```
