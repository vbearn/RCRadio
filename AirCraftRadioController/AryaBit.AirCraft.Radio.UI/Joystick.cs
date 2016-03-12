using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectInput;

namespace AryaBit.AirCraft.Radio.UI
{
    class Joystick
    {


        #region Fields

        List<DeviceInstance> directInputList = new List<DeviceInstance>();
        DirectInput directInput = new DirectInput();
        SlimDX.DirectInput.Joystick gamepad;
        SlimDX.DirectInput.JoystickState state;

        System.Timers.Timer aTimer;

        public event Action<SlimDX.DirectInput.JoystickState> OnStateUpdated;

        #endregion

        #region init

        public Joystick()
        {

            directInputList.AddRange(directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly));
            gamepad = new SlimDX.DirectInput.Joystick(directInput, directInputList[0].InstanceGuid);

        }

        #endregion

        #region Capture

        public void StartCapture()
        {
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.Interval = 20;
            aTimer.Enabled = true;
        }

        private void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            try
            {
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
            catch (Exception ex)
            {

            }


            if (OnStateUpdated != null)
                OnStateUpdated(state);

            try
            {
                gamepad.Unacquire();
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

    }
}
