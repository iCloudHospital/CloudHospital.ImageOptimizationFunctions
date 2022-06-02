using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;

namespace CloudHospital.ImageOptimizationFunctions;

public class ImageOptimizationService
{
    /// <summary>
    /// Get content type from file extension
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public string GetContentType(string? extension) => extension?.ToLower() switch
    {
        "jpg" => "image/jpeg",
        "jpeg" => "image/jpeg",
        "png" => "image/png",
        "bmp" => "image/bmp",
        "gif" => "image/gif",
        "webp" => "image/webp",
        _ => throw new NotSupportedException(),
    };

    /// <summary>
    /// Resize image
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="contentType"></param>
    /// <param name="maxWidth"></param>
    /// <param name="maxHeight"></param>
    /// <returns></returns>
    public async Task OptimizeAsync(Stream input, Stream output, string contentType, int maxWidth = 800, int maxHeight = 600, CancellationToken cancellationToken = default)
    {
        using var image = Image.Load(input);
        if (image != null)
        {
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                var size = GetResizedSize(new Size(image.Width, image.Height), new Size(maxWidth, maxHeight));

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.BoxPad,
                    Size = size,
                }));
            }

            IImageEncoder encoder = GetEncoder(contentType);

            await image.SaveAsync(output, encoder, cancellationToken);
        }
    }

    /// <summary>
    /// Resize image
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="contentType"></param>
    /// <param name="maxSize"></param>
    /// <returns></returns>
    public Task OptimizeAsync(Stream input, Stream output, string contentType, Size maxSize, CancellationToken cancellationToken = default)
    {
        return OptimizeAsync(input, output, contentType, maxSize.Width, maxSize.Height, cancellationToken);
    }

    private Size GetResizedSize(Size input, Size basis)
    {
        if (input.Width < basis.Width && input.Height < basis.Height)
        {
            return input;
        }

        double ratio;

        if (input.Width < input.Height)
        {
            ratio = (basis.Height * 1.0) / input.Height;
        }
        else
        {
            ratio = (basis.Width * 1.0) / input.Width;
        }

        var w = (int)(input.Width * ratio);
        var h = (int)(input.Height * ratio);

        return new Size(w, h);
    }

    private IImageEncoder GetEncoder(string? contentType) => contentType?.ToLower() switch
    {
        "image/jpeg" => new JpegEncoder(),
        "image/png" => new PngEncoder(),
        "image/bmp" => new BmpEncoder(),
        "image/gif" => new GifEncoder(),
        "image/webp" => new WebpEncoder(),
        _ => throw new NotSupportedException(),
    };
}
