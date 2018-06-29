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

        private readonly ImpactEventFactory _impactEventFactory;

        public MoonAppLoop(RenderForm form, Device device, SwapChain swapChain) : base(form, device, swapChain)
        {
            var terrainSize = new Size2(1024, 1024);
            
            _impactEventFactory = new ImpactEventFactory(5000, 10, 100, terrainSize);
            _impactEventFactory.Initialise(device);

            _terrain = new Terrain(device, _impactEventFactory.TerrainTexture, terrainSize.Width, terrainSize.Height);
            _terrain.IsWireframe = false;
            _camera = new Camera(_terrain.CameraInput);
            _camera.Distance = 150;

            float aspectRatio = (float) form.Height / form.Width;

            double aspect = form.Width / (double) form.Height;
            double fov = MathUtil.PiOverTwo * aspect;
            var proj = Matrix.OrthoLH(10, 10 * aspectRatio, 8000, -8000);

            _terrain.CameraInput.Projection = proj;
        }

        protected override void Draw(GameTime time)
        {
            Context.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            Context.ClearRenderTargetView(RenderView, Color.Black);

            _impactEventFactory.Draw(Context, time);

            Context.OutputMerger.SetRenderTargets(DepthView, RenderView);
            Context.Rasterizer.SetViewport(Viewport);
            _terrain.Draw(Context, time);

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
            _impactEventFactory?.Dispose();
            base.Dispose();
        }
    }
}
