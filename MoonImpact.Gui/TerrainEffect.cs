namespace MoonImpact.Gui
{
    using System;
    using System.IO;
    using SharpDX;
    using SharpDX.D3DCompiler;
    using SharpDX.Direct3D11;
    using Utilities;

    public class TerrainEffect : ICameraMatrices, IDisposable
    {
        private const string TerrainEffectFilename = @"Content\heightmap.fx";

        private TerrainDirtyFlags _dirtyFlags = TerrainDirtyFlags.All;

        private Texture2D _texture;
        private ShaderResourceView _textureView;
        private Matrix _projection;
        private Matrix _view = Matrix.Identity;
        private Matrix _world = Matrix.Identity;
        private Matrix _worldViewProjection;

        private EffectMatrixVariable _worldViewProjectionParameter, _worldParameter;
        private EffectVectorVariable _lightDirParameter, _lightColourParameter, _lightAmbientColourParameter, _resolutionParameter;
        private EffectScalarVariable _heightParameter;

        private EffectShaderResourceVariable _textureParameter;

        private Effect _effect;
        
        private float _heightMultiplier = 32;

        private Color _lightColour;
        private Color _lightAmbientColour;
        private Vector3 _lightDirection;
        private Vector3 _resolution;

        public Effect Effect => _effect;

        public TerrainEffect(Device device)
        {
            using (var fileIn = File.OpenRead(@"Content\heightmap.cso"))
            using (var effectBytecode = ShaderBytecode.FromStream(fileIn))
            {
                _effect = new Effect(device, effectBytecode, EffectFlags.None, TerrainEffectFilename);
            }

            CacheParameters();
        }

        public Matrix Projection
        {
            get => _projection;
            set
            {
                _projection = value;
                _dirtyFlags |= TerrainDirtyFlags.ModelViewProjection;
            }
        }

        public Matrix View
        {
            get => _view;
            set
            {
                _view = value;
                _dirtyFlags |= TerrainDirtyFlags.ModelViewProjection;
            }
        }

        public Matrix World
        {
            get => _world;
            set
            {
                _world = value;
                _dirtyFlags |= TerrainDirtyFlags.ModelViewProjection;
            }
        }

        public Color LightColour
        {
            get => _lightColour;
            set
            {
                _lightColour = value;
                _dirtyFlags |= TerrainDirtyFlags.Lighting;
            }
        }

        public Color LightAmbientColour
        {
            get => _lightAmbientColour;
            set
            {
                _lightAmbientColour = value;
                _dirtyFlags |= TerrainDirtyFlags.Lighting;
            }
        }

        public Vector3 LightDirection
        {
            get => _lightDirection;
            set
            {
                _lightDirection = value;
                _dirtyFlags |= TerrainDirtyFlags.Lighting;
            }
        }

        public void SetTexture(Device device, Texture2D texture)
        {
            _textureView?.Dispose();
            _textureView = null;

            _texture = texture;
            if (_texture != null)
            {
                _textureView = new ShaderResourceView(device, texture);
            }

        }

        public Vector3 Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                _dirtyFlags |= TerrainDirtyFlags.Lighting;
            }
        }
        
        public void Apply()
        {
            
            if (_dirtyFlags.HasFlag(TerrainDirtyFlags.ModelViewProjection))
            {
                // calculate the mvp matrix
                Matrix viewProj;
                Matrix.Multiply(ref _view, ref _projection, out viewProj);
                Matrix.Multiply(ref _world, ref viewProj, out _worldViewProjection);

                _worldViewProjectionParameter.SetMatrix(_worldViewProjection);
                _worldParameter.SetMatrix(_world);
            }

            if (_dirtyFlags.HasFlag(TerrainDirtyFlags.Texture) && _texture != null)
            {
                _textureParameter.SetResource(_textureView);
            }

            if (_dirtyFlags.HasFlag(TerrainDirtyFlags.HeightMultiplier))
            {
                _heightParameter.Set(_heightMultiplier);
            }

            if (_dirtyFlags.HasFlag(TerrainDirtyFlags.Lighting))
            {
                _lightColourParameter.Set(_lightColour.ToVector4());
                _lightDirParameter.Set(new Vector4(_lightDirection, 0));
                _lightAmbientColourParameter.Set(_lightAmbientColour.ToVector4());
                _resolutionParameter.Set(_resolution);
            }

            _dirtyFlags = TerrainDirtyFlags.None;
        }

        private void CacheParameters()
        {
            _worldViewProjectionParameter = _effect.GetVariableByName("WorldViewProjection").AsMatrix();
            if (_worldViewProjectionParameter == null)
                throw new InvalidOperationException(nameof(_worldViewProjectionParameter) + " cannot be null");
            _worldParameter = _effect.GetVariableByName("World").AsMatrix();
            if (_worldParameter == null)
                throw new InvalidOperationException(nameof(_worldParameter) + " cannot be null");
            
            _textureParameter = _effect.GetVariableByName("Tex").AsShaderResource();
            if (_textureParameter == null)
                throw new InvalidOperationException(nameof(_textureParameter) + " cannot be null");

            _heightParameter = _effect.GetVariableByName("HeightMultiplier").AsScalar();
            if (_heightParameter == null)
                throw new InvalidOperationException(nameof(_heightParameter) + " cannot be null");

            _lightDirParameter = _effect.GetVariableByName("lightDirection").AsVector();
            if (_lightDirParameter == null)
                throw new InvalidOperationException(nameof(_lightDirParameter) + " cannot be null");
            _lightColourParameter = _effect.GetVariableByName("lightColour").AsVector();

            if (_lightColourParameter == null)
                throw new InvalidOperationException(nameof(_lightColourParameter) + " cannot be null");
            _lightAmbientColourParameter = _effect.GetVariableByName("lightAmbient").AsVector();

            if (_lightAmbientColourParameter == null)
                throw new InvalidOperationException(nameof(_lightAmbientColourParameter) + " cannot be null");

            _resolutionParameter = _effect.GetVariableByName("Resolution").AsVector();
            if (_resolutionParameter == null)
                throw new InvalidOperationException(nameof(_resolutionParameter) + " cannot be null");

        }
        
        [Flags]
        private enum TerrainDirtyFlags
        {
            None = 0,
            ModelViewProjection = 1,
            Texture = 2,
            HeightMultiplier = 4,
            Lighting = 8,
            All = ModelViewProjection | Texture | HeightMultiplier | Lighting,
        }

        public void Dispose()
        {
            _textureView?.Dispose();
            _worldViewProjectionParameter?.Dispose();
            _worldParameter?.Dispose();
            _heightParameter?.Dispose();
            _lightDirParameter?.Dispose();
            _lightColourParameter?.Dispose();
            _effect?.Dispose();
        }
    }
}