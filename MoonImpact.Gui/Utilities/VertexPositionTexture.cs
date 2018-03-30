namespace MoonImpact.Gui.Utilities
{
    using System.Runtime.InteropServices;
    using SharpDX;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionTexture
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;

        public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }

        public static readonly InputElement[] VertexLayout =
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
        };

        public static readonly int VertexStride = 20;
    }
}