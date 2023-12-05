using Core.TCPServer;
using Microsoft.AspNetCore.Mvc;
using tcpmvc.Services;

namespace tcpmvc.Controllers
{
    [ApiController]
    [Route("api/tcpserver")]
    public class TcpServerController : ControllerBase
    {
        private TcpServer _tcpServer = new TcpServer();

        [HttpPost("start")]
        public IActionResult StartServer()
        {
            if (IsServerRunningService.IsServerRunning())
            {
                Console.WriteLine("Server is already running.");
                return BadRequest("Server is already running.");
            }

            try
            {
                _tcpServer.StartServer();
                IsServerRunningService.WriteToJsonFile(true);
                Console.WriteLine("Server started successfully.");

                return Ok("Server started successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
                return StatusCode(500, $"Failed to start server: {ex.Message}");
            }
        }

        [HttpPost("stop")]
        public IActionResult StopServer()
        {
            if (!IsServerRunningService.IsServerRunning())
            {
                Console.WriteLine("Server is not running.");
                return BadRequest("Server is not running.");
            }

            try
            {
                _tcpServer.StopServer();
                IsServerRunningService.WriteToJsonFile(false);
                Console.WriteLine("Server stopped successfully.");

                return Ok("Server stopped successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop server: {ex.Message}");
                return StatusCode(500, $"Failed to stop server: {ex.Message}");
            }
        }

        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] string message, [FromQuery] int clientId)
        {
            if (!IsServerRunningService.IsServerRunning())
            {
                Console.WriteLine("Server is not running.");
                return BadRequest("Server is not running.");
            }

            if (string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Message cannot be empty.");
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                TcpServer.SendMessageToClient(clientId, message);
                Console.WriteLine($"Message sent successfully to client {clientId}.");
                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message: {ex.Message}");
                return StatusCode(500, $"Failed to send message: {ex.Message}");
            }
        }

        [HttpGet("clients")]
        public IActionResult GetClients()
        {
            if (!IsServerRunningService.IsServerRunning())
            {
                Console.WriteLine("Server is not running.");
                return BadRequest("Server is not running.");
            }

            try
            {
                var clients = _tcpServer.GetAllClientIDs();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get clients: {ex.Message}");
                return StatusCode(500, $"Failed to get clients: {ex.Message}");
            }
        }
    }
}
