namespace MoonImpact.Gui
{
    using SharpDX;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using SharpDX.Windows;
    using Utilities;
    using Device = SharpDX.Direct3D11.Device;

    class MoonAppLoop : AppLoop
    {
        private readonly Terrain _terrain;

        private readonly Camera _camera;

        private readonly Texture2D _terrainTexture;

        public MoonAppLoop(RenderForm form, Device device, SwapChain swapChain) : base(form, device, swapChain)
        {
            _terrain = new Terrain(device, 1024, 1024);
            _terrain.IsWireframe = true;
            _camera = new Camera(_terrain.CameraInput);
            _camera.Distance = 150;

            float aspectRatio = (float) form.Height / form.Width;

            double aspect = form.Width / (double) form.Height;
            double fov = MathUtil.PiOverTwo * aspect;
            var proj = Matrix.OrthoLH(10, 10 * aspectRatio, -8000, 8000);

            _terrain.CameraInput.Projection = proj;
            
        }

        protected override void Draw(GameTime time)
        {
            Context.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            Context.ClearRenderTargetView(RenderView, Color.Black);

            _terrain.Draw(Context);

            base.Draw(time);
        }

        protected override void Update(GameTime time)
        {
            _camera.Update(time);

            base.Update(time);
        }

        public override void Dispose()
        {
            _terrain?.Dispose();
            base.Dispose();
        }
    }
}
