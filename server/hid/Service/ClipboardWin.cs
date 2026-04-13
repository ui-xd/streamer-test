using DevSim.Interfaces;
using TextCopy;


namespace DevSim.Services
{
    public class ClipboardServiceWin : IClipboardService
    {
        private readonly IKeyboardMouseInput _key;
        public ClipboardServiceWin(IKeyboardMouseInput key) {
            this._key = key;
        }



        public async Task<string?> Get()
        {
            return await ClipboardService.GetTextAsync();
        }

        public async Task Paste()
        {
            await _key.SendKeyDown("Control");
            await _key.SendKeyDown("v");
            await _key.SendKeyUp("v");
            await _key.SendKeyUp("Control");
        }

        public async Task Set(string val)
        {
            await ClipboardService.SetTextAsync(val);
        }
    }
}