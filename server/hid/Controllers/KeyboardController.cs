// using Microsoft.AspNetCore.Mvc;
// using DevSim.Interfaces;
// using DevSim.Enums;

// namespace DevSim.Controllers
// {
//     [ApiController]
//     [Route("Keyboard")]
//     public class KeyboardController : ControllerBase
//     {
        
//         private readonly IKeyboardMouseInput _key;

//         public KeyboardController( IKeyboardMouseInput key)
//         {
//             _key = key;
//         }


//         [HttpPost("Up")]
//         public void PostKeyUp([FromBody]string key)
//         {
//             _key.SendKeyUp(key);
//         }
//         [HttpPost("Down")]
//         public void PostKeyDown([FromBody]string key)
//         {
//             _key.SendKeyDown(key);
//         }



//         [HttpPost("Press")]
//         public async Task PostKeyPress([FromBody]string key)
//         {
//             _key.SendKeyDown(key);
//             await Task.Delay(1);
//             _key.SendKeyUp(key);
//         }
//         [HttpPost("Reset")]
//         public async Task KeyReset()
//         {
//             _key.SetKeyStatesUp();
//         }
//     }
// }