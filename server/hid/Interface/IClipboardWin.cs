using DevSim.Enums;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DevSim.Interfaces
{
    public interface IClipboardService
    {
        public Task Set(string val);
        public Task<string?> Get();
        public Task Paste();
    }
}
