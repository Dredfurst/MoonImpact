namespace MoonImpact.Gui.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SharpDX;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    using SharpDX.WIC;

    public class TextureHelper
    {
        private static readonly ImagingFactory imageFactory = new ImagingFactory();

        private static readonly Dictionary<Guid, Format> FormatLookup = new Dictionary<Guid, Format>()
        {
            { PixelFormat.Format128bppRGBAFloat, Format.R32G32B32A32_Float},

            { PixelFormat.Format64bppRGBAHalf, Format.R16G16B16A16_Float },
            { PixelFormat.Format64bppRGBA, Format.R16G16B16A16_UNorm},

            { PixelFormat.Format32bppRGBA, Format.R8G8B8A8_UNorm },
            { PixelFormat.Format32bppBGRA, Format.B8G8R8A8_UNorm },
            { PixelFormat.Format32bppBGR, Format.B8G8R8X8_UNorm },

            { PixelFormat.Format32bppRGBA1010102XR, Format.R10G10B10_Xr_Bias_A2_UNorm },
            { PixelFormat.Format32bppRGBA1010102, Format.R10G10B10A2_UNorm },
            { PixelFormat.Format32bppRGBE, Format.R9G9B9E5_Sharedexp },

            { PixelFormat.Format16bppBGRA5551, Format.B5G5R5A1_UNorm },
            { PixelFormat.Format16bppBGR565, Format.B5G6R5_UNorm },

            { PixelFormat.Format32bppGrayFloat, Format.R32_Float },
            { PixelFormat.Format16bppGrayHalf, Format.R16_Float },
            { PixelFormat.Format16bppGray, Format.R16_UNorm },
            { PixelFormat.Format8bppGray, Format.R8_UNorm },

            { PixelFormat.Format8bppAlpha, Format.A8_UNorm },

            { PixelFormat.Format96bppRGBFloat, Format.R32G32B32_Float }
        };

        public static Texture2D FromFile(SharpDX.Direct3D11.Device device, string path, int mipLevels = 4)
        {
            using (var stream = File.OpenRead(path))
            {
                var imageLoader = new BitmapDecoder(imageFactory, stream, DecodeOptions.CacheOnLoad);
                var frame = imageLoader.GetFrame(0);
                return CreateTexture2DFromBitmap(device, frame, mipLevels);
            }
        }

        public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmap(SharpDX.Direct3D11.Device device, SharpDX.WIC.BitmapSource bitmapSource, int mipLevels = 4)
        {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmapSource.Size.Width * GetBitsPerPixel(bitmapSource.PixelFormat);

            using (var buffer = new SharpDX.DataStream(stride * bitmapSource.Size.Height, true, true))
            {
                // Copy the content of the WIC to the buffer
                bitmapSource.CopyPixels(stride, buffer);
                //bitmapSource.CopyPixels(imageBytes, stride);

                var imageFormat = ConvertImageFormat(bitmapSource.PixelFormat);

                var imageFormatSupport = device.CheckFormatSupport(imageFormat);

                if (mipLevels > 1 && !(imageFormatSupport.HasFlag(FormatSupport.Mip) || imageFormatSupport.HasFlag(FormatSupport.MipAutogen)))
                {
                    throw new ArgumentException("Miplevel is > 1 and supported input texture doesn't support generating mips");
                }

                return new Texture2D(device, new Texture2DDescription
                {
                    Width = bitmapSource.Size.Width,
                    Height = bitmapSource.Size.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = imageFormat,
                    MipLevels = mipLevels,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(buffer.DataPointer, stride));
            }
        }

        public static SharpDX.DXGI.Format ConvertImageFormat(Guid format)
        {
            return FormatLookup[format];
        }

        private static int GetBitsPerPixel(Guid targetGuid)
        {
            using (var info = new ComponentInfo(imageFactory, targetGuid))
            {
                if (info.ComponentType != ComponentType.PixelFormat)
                    return 0;

                var pixelFormatInfo = info.QueryInterfaceOrNull<PixelFormatInfo>();
                if (pixelFormatInfo == null)
                    return 0;

                var bpp = pixelFormatInfo.BitsPerPixel;
                pixelFormatInfo.Dispose();
                return bpp;
            }
        }
    }
}