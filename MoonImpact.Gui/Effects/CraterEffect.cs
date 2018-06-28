using System;
using System.IO;
using MoonImpact.Gui.Utilities;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace MoonImpact.Gui.Effects
{
    public class CraterEffect : ICameraMatrices, IDisposable
    {
        
        private const string EffectFilename = @"Content\crater.fx";

        private Matrix _projection;
        private Matrix _view = Matrix.Identity;
        private Matrix _world = Matrix.Identity;
        private Matrix _worldViewProjection;

        private CraterDirtyFlags _dirtyFlags;
        
        private readonly Effect _effect;

        public Effect Effect => _effect;
        
        private EffectMatrixVariable _worldViewProjectionParameter;
        public CraterEffect(Device device)
        {
            using (var fileIn = File.OpenRead(@"Content\crater.cso"))
            using (var effectBytecode = ShaderBytecode.FromStream(fileIn))
            {
                _effect = new Effect(device, effectBytecode, EffectFlags.None, EffectFilename);
            }

            CacheParameters();
        }
        
        [Flags]
        private enum CraterDirtyFlags
        {
            None = 0,
            ModelViewProjection = 1,
            All = ModelViewProjection
        }
        
        public Matrix Projection
        {
            get => _projection;
            set
            {
                _projection = value;
                _dirtyFlags |= CraterDirtyFlags.ModelViewProjection;
            }
        }

        public Matrix View
        {
            get => _view;
            set
            {
                _view = value;
                _dirtyFlags |= CraterDirtyFlags.ModelViewProjection;
            }
        }

        public Matrix World
        {
            get => _world;
            set
            {
                _world = value;
                _dirtyFlags |= CraterDirtyFlags.ModelViewProjection;
            }
        }
        public void Apply()
        {
            if (_dirtyFlags.HasFlag(CraterDirtyFlags.ModelViewProjection))
            {
                // calculate the mvp matrix
                Matrix.Multiply(ref _view, ref _projection, out var viewProj);
                Matrix.Multiply(ref _world, ref viewProj, out _worldViewProjection);

                _worldViewProjectionParameter.SetMatrix(_worldViewProjection);
            }

            _dirtyFlags = CraterDirtyFlags.None;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        
        private void CacheParameters()
        {
            _worldViewProjectionParameter = _effect.GetVariableByName("WorldViewProjection").AsMatrix();
            if (_worldViewProjectionParameter == null)
                throw new InvalidOperationException(nameof(_worldViewProjectionParameter) + " cannot be null");
        }
    }
}