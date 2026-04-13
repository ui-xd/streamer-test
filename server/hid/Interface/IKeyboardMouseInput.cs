using DevSim.Enums;




namespace DevSim.Interfaces
{
    public interface IKeyboardMouseInput
    {
        Task SendKeyDown(string key);
        Task SendKeyUp(string key);
        Task SendMouseMove(float percentX, float percentY);
        Task SendMouseWheel(int deltaY);
        Task SetKeyStatesUp();
        Task ToggleRelativeMouse(bool IsOn);
        Task SendMouseButtonAction(ButtonCode button, ButtonAction buttonAction);
    }
}
