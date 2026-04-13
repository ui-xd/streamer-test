// using Microsoft.AspNetCore.Mvc;
// using DevSim.Interfaces;
// using DevSim.Enums;
// using Nefarius.ViGEm.Client.Targets.Xbox360;

// namespace DevSim.Controllers
// {
//     [ApiController]
//     [Route("GamePad")]
//     public class GamepadController : ControllerBase
//     {
//         private readonly IGamepadInput _key;

//         public GamepadController( IGamepadInput key)
//         {
//             _key = key;
//         }


//         [HttpPost("Slider")]
//         public void Slider(int index, float val)
//         {
//             _key.pressSlider("1",index,val);
//         }
//         [HttpPost("Axis")]
//         public void Axis(int index, float val)
//         {
//             _key.pressAxis("1",index,val);
//         }
//         [HttpPost("Button")]
//         public void Button(int index, bool val)
//         {
//             _key.pressButton("1",index,val);
//         }

//         [HttpPost("Status")]
//         public void Status(bool val)
//         {
//             if (val == true) {
//                 _key.Connect("1", (object sender,Xbox360FeedbackReceivedEventArgs arg) => {
//                     int LargeMotor  = (int)arg.LargeMotor;
//                     int SmallMotor  = (int)arg.SmallMotor;
//                     int LedNumber  = (int)arg.LedNumber;

//                     Console.WriteLine($" {LargeMotor} {SmallMotor} {LedNumber} ");
//                 });
//             } else {
//                 _key.DisConnect("1");
//             }
//         }
//     }
// }