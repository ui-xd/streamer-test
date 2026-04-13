using DevSim.Enums;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DevSim.Interfaces
{
    public interface IGamepadInput
    {
        public bool failed {get;}
        public IXbox360Controller Connect(string gamepad_id,Xbox360FeedbackReceivedEventHandler rumble);
        public void DisConnect(string gamepad_id);
        public Task pressButton(string gamepad_id, int index, bool pressed);
        public Task pressSlider(string gamepad_id, int index, float val);
        public Task pressAxis(string gamepad_id, int index, float val);
    }
}
