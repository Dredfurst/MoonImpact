namespace MoonImpact.Gui.Utilities
{
    using System;
    using SharpDX.Multimedia;
    using SharpDX.RawInput;

    public class RawInput
    {
        public event EventHandler<MouseInputEventArgs> MouseInput;

        public RawInput()
        {
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
            Device.MouseInput += DeviceOnMouseInput;
        }

        private void DeviceOnMouseInput(object sender, MouseInputEventArgs mouseInputEventArgs)
        {
            OnMouseInput(mouseInputEventArgs);
        }

        protected virtual void OnMouseInput(MouseInputEventArgs e)
        {
            MouseInput?.Invoke(this, e);
        }
    }
}