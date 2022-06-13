using SkiaSharp;
using System;
using System.CommandLine;
using Topten.RichTextKit;

Option<FileInfo> baseImage = new Option<FileInfo>("--base-image", "The base image for the thumbnail")
    .ExistingOnly();
Option<string> title = new("--title", "The title for the thumbnail");
Option<string> project = new("--project", "The project (top section) to use");
Option<string> technology = new("--technology", "The technology (bottom section) to use");
Option<FileInfo> outputFile = new("--output", "The output file to write");
RootCommand rootCommand = new()
{
    baseImage,
    title,
    project,
    technology,
    outputFile,
};

SKTypeface typeface = SKTypeface.FromFamilyName("Roboto Medium");

rootCommand.SetHandler(GenerateThumbnail, baseImage, title, project, technology, outputFile);

await rootCommand.InvokeAsync(args);

Task GenerateThumbnail(FileInfo baseImage, string title, string project, string technology, FileInfo outputFile)
{
    using var fileStream = baseImage.OpenRead();
    using var bitmap = SKBitmap.Decode(fileStream);

    SKRect titleArea = new(130, 180, 1150, 630);
    DrawText(bitmap, title, titleArea, "Roboto Medium");

    SKRect projectArea = new(385, 30, 912, 80);
    DrawText(bitmap, project, projectArea, "Roboto Condensed");


    SKRect techArea = new(557, 651, 738, 697);
    DrawText(bitmap, technology, techArea, "Roboto Condensed");

    using var outputStream = outputFile.OpenWrite();
    bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);
    return Task.CompletedTask;
}

void DrawText(SKBitmap bitmap, string text, SKRect area, string fontFamily)
{
    float min = 8;
    float max = 120;

    RichString rs;
    while (true)
    {
        float fontSize = ((max - min) / 2f) + min;
        rs = CreateString(fontSize);
        if (rs.MeasuredHeight < area.Height)
        {
            //increase font size
            min = fontSize;
        }
        else
        {
            max = fontSize;
        }
        if (max - min < 1)
        {
            rs = CreateString(min);
            break;
        }

        RichString CreateString(float fontSize)
        {
            var rv = new RichString()
                .Alignment(TextAlignment.Center)
                .Add(text, fontFamily:fontFamily, fontSize:fontSize, textColor: new(237, 244, 224));
            rv.MaxWidth = area.Width;
            return rv;
        }
    }

    using SKCanvas canvas = new(bitmap);

    rs.Paint(canvas, new SKPoint(area.Left, area.Top));
}
