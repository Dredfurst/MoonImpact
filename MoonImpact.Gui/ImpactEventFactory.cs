using System;
using System.Collections.Generic;
using MoonImpact.Gui.Effects;
using MoonImpact.Gui.Utilities;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace MoonImpact.Gui
{
    public class ImpactEventFactory : IComponent
    {
        private readonly List<ImpactEvent> _impactEvents = new List<ImpactEvent>();

        private readonly RenderTargetView _renderTarget;
        
        private int _indexCount;

        private Buffer _vertexBuffer;

        private Buffer _indexBuffer;

        private BlendState _blendState;

        private VertexBufferBinding _vertexBufferBinding;

        private CraterEffect _effect;

        private InputLayout _vertexLayout;

        public int NumberOfImpacts { get; }

        public float MinimumSize { get; }

        public float MaximumSize { get; }

        public RectangleF TerrainDimensions { get; }

        public IEnumerable<ImpactEvent> ImpactEvents => _impactEvents;

        private RasterizerState _normalMode;


        public ImpactEventFactory(int numberOfImpacts, float minimumSize, float maximumSize, RectangleF terrainDimensions, RenderTargetView renderTarget)
        {
            NumberOfImpacts = numberOfImpacts;
            MinimumSize = minimumSize;
            MaximumSize = maximumSize;
            TerrainDimensions = terrainDimensions;
            _renderTarget = renderTarget;
        }

        public void Dispose()
        {

        }

        public void Initialise(Device device)
        {
            InitialiseRandomImpacts();
            InitialiseGeometryBuffers(device);
            
            var normalModeDescription = RasterizerStateDescription.Default();
            _normalMode = new RasterizerState(device, normalModeDescription);

            var blendDesc = BlendStateDescription.Default();
            blendDesc.AlphaToCoverageEnable = true;
            blendDesc.RenderTarget[0].IsBlendEnabled = true;
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Minimum;
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            _blendState = new BlendState(device, blendDesc);
        }

        public void Draw(DeviceContext context, GameTime time)
        {
            context.ClearRenderTargetView(_renderTarget, Color.White);
            context.OutputMerger.SetRenderTargets(_renderTarget);
            
            _effect.Apply();
            
            context.InputAssembler.InputLayout = _vertexLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);
            context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            
            for (var i = 0; i < _effect.Effect.GetTechniqueByIndex(0).Description.PassCount; i++)
            {
                var pass = _effect.Effect.GetTechniqueByIndex(0).GetPassByIndex(i);
                context.Rasterizer.State = _normalMode;
                context.OutputMerger.BlendState = _blendState;
                pass.Apply(context);
                
                context.DrawIndexed(_indexCount, 0, 0);
                
            }
            
        }

        public void Update(GameTime time)
        {
        }

        private void InitialiseRandomImpacts()
        {
            
            var rng = new Random(1);
            for (int i = 0; i < NumberOfImpacts; i++)
            {
                var width = rng.NextFloat(MinimumSize, MaximumSize) * 0.5f;
                var depth = rng.NextFloat(MinimumSize, MaximumSize) * 0.5f;

                var center = rng.NextVector2(TerrainDimensions.TopLeft, TerrainDimensions.BottomRight);

                var impact = new ImpactEvent
                {
                    Dimensions = new RectangleF(center.X - width, center.Y - width, width * 2, width * 2),
                    Depth = depth
                };

                _impactEvents.Add(impact);
            }

        }

        private void InitialiseGeometryBuffers(Device device)
        {
            var verticesList = new List<VertexPositionTexture3>();
            var indiciesList = new List<int>();

            foreach (var impact in ImpactEvents)
            {
                GenerateGeometryForImpact(impact, ref verticesList, ref indiciesList);
            }

            var vertices = verticesList.ToArray();
            var indices = indiciesList.ToArray();

            _effect = new CraterEffect(device)
            {
                Projection = Matrix.OrthoOffCenterLH(TerrainDimensions.Left, TerrainDimensions.Right,
                    TerrainDimensions.Bottom, TerrainDimensions.Top, -1, 1),
                View = Matrix.Identity,
                World = Matrix.Identity
            };

            var pass = _effect.Effect.GetTechniqueByIndex(0).GetPassByIndex(0);
            
            using (var passSignature = pass.Description.Signature)
            {
                try
                {
                    _vertexLayout = new InputLayout(device, passSignature, VertexPositionTexture3.VertexLayout);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }
            
            _vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
            _indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);
            
            _vertexBufferBinding = new VertexBufferBinding(_vertexBuffer, VertexPositionTexture3.VertexStride, 0);

            _indexCount = indices.Length;
        }

        private void GenerateGeometryForImpact(ImpactEvent e, ref List<VertexPositionTexture3> vertices, ref List<int> indices)
        {
            int offset = vertices.Count;
            var depth = GetDepthWithinBounds(e.Depth);

            vertices.Add(new VertexPositionTexture3(new Vector3(e.Dimensions.TopLeft, 0), new Vector3(0, 0, depth)));
            vertices.Add(new VertexPositionTexture3(new Vector3(e.Dimensions.TopRight, 0), new Vector3(1, 0, depth)));
            vertices.Add(new VertexPositionTexture3(new Vector3(e.Dimensions.BottomRight, 0), new Vector3(1, 1, depth)));
            vertices.Add(new VertexPositionTexture3(new Vector3(e.Dimensions.BottomLeft, 0), new Vector3(0, 1, depth)));

            indices.Add(offset + 0);
            indices.Add(offset + 1);
            indices.Add(offset + 3);
            
            indices.Add(offset + 3);
            indices.Add(offset + 1);
            indices.Add(offset + 2);
        }

        private float GetDepthWithinBounds(float depth)
        {
            // max depth is MaximumSize,
            // we want a number between 0 and 1
            return depth / MaximumSize;
        }
    }
}