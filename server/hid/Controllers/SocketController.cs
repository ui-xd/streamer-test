using Microsoft.AspNetCore.Mvc;
using DevSim.Interfaces;
using DevSim.Enums;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.Globalization;

namespace DevSim.Controllers
{
    [ApiController]
    [Route("Socket")]
    public class SocketController : ControllerBase
    {
        private readonly IKeyboardMouseInput _key;
        private readonly IGamepadInput _gamepad;
        private readonly IClipboardService _clipboard;
        private readonly Random _rand;
        public SocketController(IGamepadInput gamepad,
                                IClipboardService clipboard,
                                IKeyboardMouseInput key) {
            _key = key;
            _gamepad = gamepad;
            _clipboard = clipboard;
            _rand = new Random();
        }

        [HttpGet]
        public async Task Get(string? token)
        {
            var context = ControllerContext.HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                int random = _rand.Next();
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Task.Run(async () => {
                    try { while (webSocket.State == WebSocketState.Open) {
                        await this.SendMessage(webSocket,"ping");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }} catch{}
                });
                await Handle(random,webSocket);
            }
        }

        private async Task Handle(int id, WebSocket ws)
        {
            var connectedGamepad = new List<string>();
            try
            {
                var pinged = true;
                Task.Run(async () => {
                    try { while (ws.State == WebSocketState.Open) {
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                        if (pinged) {
                            pinged = false;
                            return;
                        }
                        await ws.CloseAsync(WebSocketCloseStatus.Empty,"ping timeout",CancellationToken.None);
                    }} catch{}
                });

                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var message = ReceiveMessage(ws, memoryStream).Result;
                        if (message.Count > 0) {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                            if (receivedMessage == "ping") {
                                pinged = true;
                            } else {
                                Task.Run(async () => { try { 
                                    await HandleKey(id,ws,receivedMessage,connectedGamepad); 
                                }catch(Exception e){Console.WriteLine(e.Message);} });
                            }
                        }
                    }
                } while (ws.State == WebSocketState.Open);
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.Message);
            }
            connectedGamepad.ForEach(x => _gamepad.DisConnect(x));
            Console.WriteLine("Connection closed");
        }

        private async Task<WebSocketReceiveResult> ReceiveMessage(WebSocket ws, Stream memoryStream)
        {
            var readBuffer = new ArraySegment<byte>(new byte[4 * 1024]);
            WebSocketReceiveResult result;

            do {
                result = await ws.ReceiveAsync(readBuffer, CancellationToken.None);
                await memoryStream.WriteAsync(readBuffer.Array, readBuffer.Offset, result.Count, CancellationToken.None);
            } while (!result.EndOfMessage);
            return result;
        }

        private async Task SendMessage(WebSocket ws, string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            var buffer = new ArraySegment<byte>(bytes);

            try {
                await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            } catch { 
                Console.WriteLine("Fail to send websocket to client"); 
                await ws.CloseAsync(WebSocketCloseStatus.Empty,"ping timeout",CancellationToken.None);
            }
        }

        private async Task HandleKey(int id, WebSocket ws, string receivedMessage, List<string> connectedGamepad) {
            var arr = receivedMessage.Split("|");
            switch (arr[0])
            {
                case "mmr":
                    await _key.ToggleRelativeMouse(true);
                    _key.SendMouseMove(Single.Parse(arr[1], CultureInfo.InvariantCulture),Single.Parse(arr[2], CultureInfo.InvariantCulture));
                    break;
                case "mma":
                    await _key.ToggleRelativeMouse(false);
                    _key.SendMouseMove(Single.Parse(arr[1],  CultureInfo.InvariantCulture),Single.Parse(arr[2],  CultureInfo.InvariantCulture));
                    break;
                case "mw":
                    _key.SendMouseWheel(Int32.Parse(arr[1]));
                    break;
                case "mu":
                    _key.SendMouseButtonAction((ButtonCode)Int32.Parse(arr[1]),ButtonAction.Up);
                    break;
                case "md":
                    _key.SendMouseButtonAction((ButtonCode)Int32.Parse(arr[1]),ButtonAction.Down);
                    break;

                case "kd":
                    _key.SendKeyDown(arr[1]);
                    break;
                case "ku":
                    _key.SendKeyUp(arr[1]);
                    break;
                case "kr":
                    _key.SetKeyStatesUp();
                    break;

                case "cs":
                    _clipboard.Set(Base64Decode(arr[1]));
                    break;
                case "cp":
                    _clipboard.Paste();
                    break;

                default:
                break;
            }

            if (this._gamepad.failed) {
                return;
            }


            switch (arr[0])
            {
                case "gcon":
                    var gp = $"{id}.{arr[1]}";
                    _gamepad.Connect(gp, (object sender,Xbox360FeedbackReceivedEventArgs arg) => {
                        int LargeMotor  = (int)arg.LargeMotor;
                        int SmallMotor  = (int)arg.SmallMotor;
                        int LedNumber  = (int)arg.LedNumber;
                        SendMessage(ws,$"grum|{arr[1]}|{LargeMotor}|{SmallMotor}|{LedNumber}");
                    });
                    connectedGamepad.Add(gp);
                    break;
                case "gdis":
                    var disgp = $"{id}.{arr[1]}";
                    _gamepad.DisConnect(disgp);
                    connectedGamepad.RemoveAll(x => x == disgp);
                    break;
                case "gs":
                    await _gamepad.pressSlider($"{id}.{arr[1]}",Int32.Parse(arr[2], CultureInfo.InvariantCulture),Single.Parse(arr[3], CultureInfo.InvariantCulture));
                    break;
                case "ga":
                    await _gamepad.pressAxis($"{id}.{arr[1]}",Int32.Parse(arr[2]),Single.Parse(arr[3], CultureInfo.InvariantCulture));
                    break;
                case "gb":
                    await _gamepad.pressButton($"{id}.{arr[1]}",Int32.Parse(arr[2]),arr[3] == "1");
                    break;

                default:
                break;
            }
        }
        public static string Base64Decode(string base64EncodedData) 
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Encode(string plainText) 
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}