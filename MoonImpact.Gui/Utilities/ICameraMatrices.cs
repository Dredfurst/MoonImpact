namespace MoonImpact.Gui.Utilities
{
    using SharpDX;

    public interface ICameraMatrices
    {
        Matrix World { get; set; }
        Matrix View { get; set; }
        Matrix Projection { get; set; }

        void Apply();
    }
}