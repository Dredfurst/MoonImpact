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

        private readonly RenderTargetView _terrainRenderTarget;

        private readonly ImpactEventFactory _impactEventFactory;

        public MoonAppLoop(RenderForm form, Device device, SwapChain swapChain) : base(form, device, swapChain)
        {
            var terrainDesc = new Texture2DDescription
            {
                Width = 1024,
                Height = 1024,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R16_UNorm,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0)
            };

            _terrainTexture = new Texture2D(device, terrainDesc);
            var rtvDesc = new RenderTargetViewDescription
            {
                Format = terrainDesc.Format,
                Dimension = RenderTargetViewDimension.Texture2D,
                Texture2D = {MipSlice = 0},
            };


            _terrainRenderTarget = new RenderTargetView(device, _terrainTexture, rtvDesc);

            _terrain = new Terrain(device, _terrainTexture, 1024, 1024);
            _terrain.IsWireframe = false;
            _camera = new Camera(_terrain.CameraInput);
            _camera.Distance = 150;

            float aspectRatio = (float) form.Height / form.Width;

            double aspect = form.Width / (double) form.Height;
            double fov = MathUtil.PiOverTwo * aspect;
            var proj = Matrix.OrthoLH(10, 10 * aspectRatio, 8000, -8000);

            _terrain.CameraInput.Projection = proj;
            
            _impactEventFactory = new ImpactEventFactory(500, 10, 100, new RectangleF(0, 0, _terrain.Width, _terrain.Height), _terrainRenderTarget);

            InitialiseMoonTerrain(device);
        }

        protected override void Draw(GameTime time)
        {
            Context.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            Context.ClearRenderTargetView(RenderView, Color.Black);

            _impactEventFactory.Draw(Context, time);

            Context.OutputMerger.SetRenderTargets(DepthView, RenderView);
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
            _terrainRenderTarget?.Dispose();
            _terrainTexture?.Dispose();
            _impactEventFactory?.Dispose();
            base.Dispose();
        }

        private void InitialiseMoonTerrain(Device device)
        {
            _impactEventFactory.Initialise(device);
        }
    }
}
