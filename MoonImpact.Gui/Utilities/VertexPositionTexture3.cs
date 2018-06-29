using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MoonImpact.Gui.Utilities
{
    public struct VertexPositionTexture3
    {
        public Vector3 Position;
        public Vector3 TextureCoordinate;

        public VertexPositionTexture3(Vector3 position, Vector3 textureCoordinate)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }

        public static readonly InputElement[] VertexLayout =
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32B32_Float, 12, 0)
        };

        public static readonly int VertexStride = 24;
    }
}