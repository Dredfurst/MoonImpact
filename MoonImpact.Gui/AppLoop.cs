namespace MoonImpact.Gui
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using SharpDX;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using SharpDX.Windows;
    using Utilities;
    using Device = SharpDX.Direct3D11.Device;

    public class AppLoop : IDisposable
    {
        public AppLoop(RenderForm form, Device device, SwapChain swapChain)
        {
            Device = device;
            Context = device.ImmediateContext;
            
            // New RenderTargetView from the backbuffer
            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            RenderView = new RenderTargetView(device, backBuffer);


            // Create Depth Buffer & View 
            var depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = form.ClientSize.Width,
                Height = form.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            DepthView= new DepthStencilView(device, depthBuffer);

            _form = form;
            SwapChain = swapChain;

            _gameTimer = new Stopwatch();
            _gameTimer.Start();
        }

        protected Device Device { get; }

        protected DeviceContext Context { get; } 

        protected SwapChain SwapChain { get; }

        protected RenderTargetView RenderView { get; }

        protected DepthStencilView DepthView { get; }

        private readonly GameTime _gameTime = new GameTime();
        private readonly RenderForm _form;
        
        private TimeSpan _accumulatedElapsedTime;
        private Stopwatch _gameTimer;
        private long _previousTicks;
        private int _updateFrameLag;
        private bool _suppressDraw;
        private bool _shouldExit;

        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667L);
        private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02);
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500.0);

        public bool IsFixedTimeStep { get; set; }
        public TimeSpan TargetElapsedTime
        {
            get => this._targetElapsedTime;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The time must be positive and non-zero.", (Exception) null);
                if (!(value != this._targetElapsedTime))
                    return;
                this._targetElapsedTime = value;
            }
        }

        public void DoUpdate(GameTime time)
        {
            Update(time);
        }

        public void DoDraw(GameTime time)
        {
            Context.Rasterizer.SetViewport(new Viewport(0, 0, _form.ClientSize.Width, _form.ClientSize.Height, 0.0f, 1.0f));
            Context.OutputMerger.SetTargets(DepthView, RenderView);

            Draw(time);
            SwapChain.Present(0, PresentFlags.None);
        }

        protected virtual void Draw(GameTime time)
        {
        }

        protected virtual void Update(GameTime time)
        {
        }

        public virtual void Dispose()
        {
            Context?.Dispose();
            RenderView?.Dispose();
        }

        public void Tick()
        {
            while (true)
            {
                long ticks = this._gameTimer.Elapsed.Ticks;
                this._accumulatedElapsedTime += TimeSpan.FromTicks(ticks - this._previousTicks);
                this._previousTicks = ticks;
                if (this.IsFixedTimeStep && this._accumulatedElapsedTime < this.TargetElapsedTime)
                    Thread.Sleep((int) (this.TargetElapsedTime - this._accumulatedElapsedTime).TotalMilliseconds);
                else
                    break;
            }
            if (this._accumulatedElapsedTime > this._maxElapsedTime)
                this._accumulatedElapsedTime = this._maxElapsedTime;
            if (this.IsFixedTimeStep)
            {
                this._gameTime.ElapsedGameTime = this.TargetElapsedTime;
                int num = 0;
                while (this._accumulatedElapsedTime >= this.TargetElapsedTime && !this._shouldExit)
                {
                    this._gameTime.TotalGameTime += this.TargetElapsedTime;
                    this._accumulatedElapsedTime -= this.TargetElapsedTime;
                    ++num;
                    this.DoUpdate(this._gameTime);
                }
                this._updateFrameLag += Math.Max(0, num - 1);
                if (this._gameTime.IsRunningSlowly)
                {
                    if (this._updateFrameLag == 0)
                        this._gameTime.IsRunningSlowly = false;
                }
                else if (this._updateFrameLag >= 5)
                    this._gameTime.IsRunningSlowly = true;
                if (num == 1 && this._updateFrameLag > 0)
                    --this._updateFrameLag;
                this._gameTime.ElapsedGameTime = TimeSpan.FromTicks(this.TargetElapsedTime.Ticks * (long) num);
            }
            else
            {
                this._gameTime.ElapsedGameTime = this._accumulatedElapsedTime;
                this._gameTime.TotalGameTime += this._accumulatedElapsedTime;
                this._accumulatedElapsedTime = TimeSpan.Zero;
                this.DoUpdate(this._gameTime);
            }
            if (this._suppressDraw)
                this._suppressDraw = false;
            else
                this.DoDraw(this._gameTime);
            if (!this._shouldExit)
                return;
            _form.Close();
        }
    }
}