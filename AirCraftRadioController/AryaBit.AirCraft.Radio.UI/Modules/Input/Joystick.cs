using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectInput;
using System.Threading;

namespace AryaBit.AirCraft.Radio.UI
{
    class Joystick : IDisposable
    {

        #region Fields

        DirectInput directInput = new DirectInput();
        SlimDX.DirectInput.Joystick gamepad;
        SlimDX.DirectInput.JoystickState state;

        public event Action<SlimDX.DirectInput.JoystickState> OnStateUpdated;

        private bool isCapturing = false;

        #endregion

        #region List

        public static IList<DeviceInstance> GetControllers()
        {
            DirectInput directInput = new DirectInput();
            var devices = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
            return devices;
        }

        #endregion

        #region init

        public Joystick(Guid controllerGuid)
        {
            gamepad = new SlimDX.DirectInput.Joystick(directInput, controllerGuid);
        }

        public void Dispose()
        {
            this.isCapturing = false;
        }

        #endregion

        #region Capture

        public void StartCaptureThread()
        {
            new Thread(() =>
            {
                this.isCapturing = true;
                CaptureThread();
            }).Start();
        }

        private void CaptureThread()
        {

            while (true)
            {
                try
                {
                    if (!this.isCapturing)
                        return;

                    if (gamepad.Acquire().IsFailure)
                        return;

                    if (gamepad.Poll().IsFailure)
                        return;

                    if (SlimDX.Result.Last.IsFailure)
                        return;

                    state = gamepad.GetCurrentState();

                    bool[] buttons = state.GetButtons();
                    //for (int i = 0; i < buttons.Length; i++)
                    //    if (buttons[i])
                    //        label2.Text = i.ToString();

                }
                catch (Exception ex) { }


                if (OnStateUpdated != null)
                    OnStateUpdated(state);

                try
                {
                    gamepad.Unacquire();
                }
                catch (Exception ex) { }

                Thread.Sleep(30);
            }

        }


        #endregion

    }
}
