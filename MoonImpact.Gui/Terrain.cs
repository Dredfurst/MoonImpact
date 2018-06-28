using MoonImpact.Gui.Effects;

namespace MoonImpact.Gui
{
    using System;
    using System.Collections.Generic;
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using Utilities;
    using Buffer = SharpDX.Direct3D11.Buffer;
    using Device = SharpDX.Direct3D11.Device;

    public class Terrain : IDisposable, IComponent
    {
        private int _indexCount;

        private Buffer _vertexBuffer;

        private Buffer _indexBuffer;

        private VertexBufferBinding _vertexBufferBinding;

        private readonly TerrainEffect _effect;

        private InputLayout _vertexLayout;

        private readonly RasterizerState _wireframeMode;

        private readonly RasterizerState _normalMode;

        public int Width { get; }

        public int Height { get; }

        public ICameraMatrices CameraInput => _effect;

        public bool IsWireframe { get; set; }

        public Terrain(Device device, Texture2D heightmap, int width, int height)
        {
            Width = width;
            Height = height;
            
            var lightDir = new Vector3(0.25f, 0.25f, 0.75f);
            lightDir.Normalize();

            _effect = new TerrainEffect(device);
            _effect.SetTexture(device, heightmap);
            _effect.LightColour = Color.DarkGray;
            _effect.LightAmbientColour = Color.White;
            _effect.LightDirection = lightDir;
            
            _effect.Resolution = new Vector3(1 / (float) width, 1 / (float)height, 0);

            InitializeGrid(device, width, height);

            var wireframeModeDescription = RasterizerStateDescription.Default();
            wireframeModeDescription.FillMode = FillMode.Wireframe;
            wireframeModeDescription.CullMode = CullMode.None;

            _wireframeMode = new RasterizerState(device, wireframeModeDescription);

            var normalModeDescription = RasterizerStateDescription.Default();
            _normalMode = new RasterizerState(device, normalModeDescription);

        }

        private void InitializeGrid(Device device, int width, int height)
        {
            const float terrainScale = 1f;
            var positionY = -(height / 2.0f) * terrainScale;
            
            var vertices = new VertexPositionTexture[width * height];

            for (var y = 0; y < height; y++)
            {
                var vcoordinate = y / (float) height;
                var positionX = -(width / 2.0f) * terrainScale;
                for (var x = 0; x < width; x++)
                {
                    float ucoordinate = x / (float) width;
                    vertices[x + y * width] = new VertexPositionTexture(new Vector3(positionX, positionY, 0), new Vector2(ucoordinate, vcoordinate));
                    positionX += terrainScale;
                }

                positionY += terrainScale;
            }

            var indicesList = new List<int>();
            for (var y = 0; y < height - 1; y++)
            {
                for (var x = 0; x < width - 1; x++)
                {
                    // top left
                    var topLeft = x + y * width;

                    // top right
                    var topRight = x + 1 + y * width;

                    // bottom left
                    var bottomLeft = x + (y + 1) * width;

                    // bottom right
                    var bottomRight =  x + 1 + (y + 1) * width;

                    // top top right
                    var topTopRight = x + 1 + Math.Max(0, y - 1) * width;

                    // top right right
                    var topRightRight = Math.Min(x + 2, width) + y * width;

                    // bottom bottom left
                    var bottomBottomLeft = x + Math.Min(y + 2, height) * width;

                    // bottom left left
                    var bottomLeftLeft = Math.Max(0, x - 1) + (y + 1) * width;

                    indicesList.Add(topLeft);
                    indicesList.Add(topTopRight);
                    indicesList.Add(topRight);
                    indicesList.Add(bottomRight);
                    indicesList.Add(bottomLeft);
                    indicesList.Add(bottomLeftLeft);
                    
                    indicesList.Add(bottomLeft);
                    indicesList.Add(topLeft);
                    indicesList.Add(topRight);
                    indicesList.Add(topRightRight);
                    indicesList.Add(bottomRight);
                    indicesList.Add(bottomBottomLeft);
                }
            }


            //var technique = _effect.Effect.GetTechniqueByIndex(0);
            var pass = _effect.Effect.GetTechniqueByIndex(0).GetPassByIndex(0);
            
            using (var passSignature = pass.Description.Signature)
            {
                try
                {
                    _vertexLayout = new InputLayout(device, passSignature, VertexPositionTexture.VertexLayout);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }

            _vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
            _indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indicesList.ToArray());
            
            _vertexBufferBinding = new VertexBufferBinding(_vertexBuffer, VertexPositionTexture.VertexStride, 0);

            _indexCount = indicesList.Count;
        }

        public void Initialise(Device device)
        {
        }

        public void Draw(DeviceContext context, GameTime time)
        {
            _effect.Apply();
            
            context.InputAssembler.InputLayout = _vertexLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleListWithAdjacency;
            context.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);
            context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            
            for (var i = 0; i < _effect.Effect.GetTechniqueByIndex(0).Description.PassCount; i++)
            {
                var pass = _effect.Effect.GetTechniqueByIndex(0).GetPassByIndex(i);
                context.Rasterizer.State = IsWireframe ? _wireframeMode : _normalMode;
                pass.Apply(context);
                
                context.DrawIndexed(_indexCount, 0, 0);
                
            }
        }

        public void Update(GameTime time)
        {
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _effect?.Dispose();
            _vertexLayout?.Dispose();
            _wireframeMode?.Dispose();
        }
    }
}