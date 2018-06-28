using System;
using System.Collections;
using System.Collections.Generic;
using MoonImpact.Gui.Utilities;
using SharpDX.Direct3D11;

namespace MoonImpact.Gui
{
    public interface IComponent : IDisposable
    {
        void Initialise(Device device);
        void Draw(DeviceContext context, GameTime time);
        void Update(GameTime time);
    }
}