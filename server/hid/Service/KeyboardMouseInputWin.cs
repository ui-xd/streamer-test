using DevSim.Interfaces;
using DevSim.Enums;
using DevSim.Utilities;
using System;
using System.Collections.Concurrent;
using System.Threading;
using static DevSim.Win32.User32;
using DevSim.Win32;

namespace DevSim.Services
{
    public class KeyboardMouseInputWin : IKeyboardMouseInput
    {
        private readonly ConcurrentQueue<Action> _inputActions = new();
        private CancellationTokenSource _cancelTokenSource;
        private Thread _inputProcessingThread;

        private bool relativeMouse;

        public KeyboardMouseInputWin()
        {
            relativeMouse = false;
        }

        public async Task ToggleRelativeMouse(bool IsOn) 
        {
            this.relativeMouse = IsOn;
        }

        public async Task SendKeyUp(string key)
        {
            Try(() =>
            {
                if (!ConvertJavaScriptKeyToVirtualKey(key, out var keyCode) || keyCode is null)
                {
                    return;
                }

                var union = new InputUnion()
                {
                    ki = new KEYBDINPUT()
                    {
                        wVk = keyCode.Value,
                        wScan = (ScanCodeShort)MapVirtualKeyEx((uint)keyCode.Value, VkMapType.MAPVK_VK_TO_VSC, GetKeyboardLayout()),
                        time = 0,
                        dwFlags = KEYEVENTF.KEYUP,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                };
                var input = new INPUT() { type = InputType.KEYBOARD, U = union };
                SendInput(1, new INPUT[] { input }, INPUT.Size);
            });
        }



        public async Task SendKeyDown(string key)
        {
            Try(() =>
            {
                if (!ConvertJavaScriptKeyToVirtualKey(key, out var keyCode) || keyCode is null)
                    return;

                var union = new InputUnion()
                {
                    ki = new KEYBDINPUT()
                    {
                        wVk = keyCode.Value,
                        wScan = (ScanCodeShort)MapVirtualKeyEx((uint)keyCode.Value, VkMapType.MAPVK_VK_TO_VSC, GetKeyboardLayout()),
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                };
                var input = new INPUT() { type = InputType.KEYBOARD, U = union };
                SendInput(1, new INPUT[] { input }, INPUT.Size);
            });
        }


        public async Task SendMouseButtonAction(ButtonCode button, ButtonAction buttonAction)
        {
            Try(() =>
            {
                MOUSEEVENTF mouseEvent;
                switch (button)
                {
                    case ButtonCode.Left:
                        switch (buttonAction)
                        {
                            case ButtonAction.Down:
                                mouseEvent = MOUSEEVENTF.LEFTDOWN;
                                break;
                            case ButtonAction.Up:
                                mouseEvent = MOUSEEVENTF.LEFTUP;
                                break;
                            default:
                                return;
                        }
                        break;
                    case ButtonCode.Middle:
                        switch (buttonAction)
                        {
                            case ButtonAction.Down:
                                mouseEvent = MOUSEEVENTF.MIDDLEDOWN;
                                break;
                            case ButtonAction.Up:
                                mouseEvent = MOUSEEVENTF.MIDDLEUP;
                                break;
                            default:
                                return;
                        }
                        break;
                    case ButtonCode.Right:
                        switch (buttonAction)
                        {
                            case ButtonAction.Down:
                                mouseEvent = MOUSEEVENTF.RIGHTDOWN;
                                break;
                            case ButtonAction.Up:
                                mouseEvent = MOUSEEVENTF.RIGHTUP;
                                break;
                            default:
                                return;
                        }
                        break;
                    default:
                        return;
                }
                var union = new InputUnion() { mi = new MOUSEINPUT() { dwFlags = mouseEvent | MOUSEEVENTF.VIRTUALDESK, dx = 0, dy = 0, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() } };
                var input = new INPUT() { type = InputType.MOUSE, U = union };
                SendInput(1, new INPUT[] { input }, INPUT.Size);
            });
        }

        public async Task SendMouseMove(float percentX, float percentY)
        {
            Try(() =>
            {
                if (this.relativeMouse) {
                    var union = new InputUnion() { 
                        mi = new MOUSEINPUT() 
                        { 
                            dwFlags = MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK, 
                            dx = (int)percentX, 
                            dy = (int)percentY, 
                            time = 0, 
                            mouseData = 0, 
                            dwExtraInfo = GetMessageExtraInfo() 
                        } 
                    };
                    var input = new INPUT() { type = InputType.MOUSE, U = union };
                    SendInput(1, new INPUT[] { input }, INPUT.Size);
                } else {
                    var normalizedX = (double)percentX * 65535D;
                    var normalizedY = (double)percentY * 65535D;
                    var union = new InputUnion() { 
                        mi = new MOUSEINPUT() 
                        { 
                            dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK, 
                            dx = (int)normalizedX, 
                            dy = (int)normalizedY, 
                            time = 0, 
                            mouseData = 0, 
                            dwExtraInfo = GetMessageExtraInfo() 
                        } 
                    };
                    var input = new INPUT() { type = InputType.MOUSE, U = union };
                    SendInput(1, new INPUT[] { input }, INPUT.Size);
                }
            });
        }

        public async Task SendMouseWheel(int deltaY)
        {
            Try(() =>
            {
                if (deltaY < 0)
                    deltaY = -120;
                else if (deltaY > 0)
                    deltaY = 120;

                var union = new InputUnion() { mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.WHEEL, dx = 0, dy = 0, time = 0, mouseData = deltaY, dwExtraInfo = GetMessageExtraInfo() } };
                var input = new INPUT() { type = InputType.MOUSE, U = union };
                SendInput(1, new INPUT[] { input }, INPUT.Size);
            });
        }


        public async Task SetKeyStatesUp()
        {
            var keys = new List<string> { "Down" , "Up" , "Left" , "Right" , "Enter" , "Esc" , "Alt" , "Control" ,
                "Shift" , "PAUSE" , "BREAK" , "Backspace" , "Tab" , "CapsLock" , "Delete" , "Home" , "End" , "PageUp" ,
                "PageDown" , "NumLock" , "Insert" , "ScrollLock" , "F1" , "F2" , "F3" , "F4" , "F5" ,
                "F6" , "F7" , "F8" , "F9" , "F10" , "F11" , "F12" , "Meta" };
            
            foreach (var k in keys) {
                await this.SendKeyUp(k);
            }
        }


        private bool ConvertJavaScriptKeyToVirtualKey(string key, out VirtualKey? result)
        {
            result = key switch
            {
                "Down" or "ArrowDown" => VirtualKey.DOWN,
                "Up" or "ArrowUp" => VirtualKey.UP,
                "Left" or "ArrowLeft" => VirtualKey.LEFT,
                "Right" or "ArrowRight" => VirtualKey.RIGHT,
                "Enter" => VirtualKey.RETURN,
                "Esc" or "Escape" => VirtualKey.ESCAPE,
                "Alt" => VirtualKey.MENU,
                "Control" => VirtualKey.CONTROL,
                "Shift" => VirtualKey.SHIFT,
                "PAUSE" => VirtualKey.PAUSE,
                "BREAK" => VirtualKey.PAUSE,
                "Backspace" => VirtualKey.BACK,
                "Tab" => VirtualKey.TAB,
                "CapsLock" => VirtualKey.CAPITAL,
                "Delete" => VirtualKey.DELETE,
                "Home" => VirtualKey.HOME,
                "End" => VirtualKey.END,
                "PageUp" => VirtualKey.PRIOR,
                "PageDown" => VirtualKey.NEXT,
                "NumLock" => VirtualKey.NUMLOCK,
                "Insert" => VirtualKey.INSERT,
                "ScrollLock" => VirtualKey.SCROLL,
                "F1" => VirtualKey.F1,
                "F2" => VirtualKey.F2,
                "F3" => VirtualKey.F3,
                "F4" => VirtualKey.F4,
                "F5" => VirtualKey.F5,
                "F6" => VirtualKey.F6,
                "F7" => VirtualKey.F7,
                "F8" => VirtualKey.F8,
                "F9" => VirtualKey.F9,
                "F10" => VirtualKey.F10,
                "F11" => VirtualKey.F11,
                "F12" => VirtualKey.F12,
                "Meta" => VirtualKey.LWIN,
                "ContextMenu" => VirtualKey.MENU,
                _ => key.Length == 1 ? 
                        (VirtualKey)VkKeyScan(Convert.ToChar(key)) :
                        null
            };

            if (result is null)
            {
                Logger.Write($"Unable to parse key input: {key}.");
                return false;
            }
            return true;
        }

        private void Try(Action inputAction)
        {
            try { inputAction(); }
            catch (Exception ex) { Logger.Write(ex); }
        }
    }
}