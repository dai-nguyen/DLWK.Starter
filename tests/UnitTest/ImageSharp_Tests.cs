using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace UnitTest
{
    [TestClass]
    public class ImageSharp_Tests
    {
        [TestMethod]
        public void AvatarWithRoundedCorner()
        {
            System.IO.Directory.CreateDirectory("output");
            using (var img = Image.Load("Images\\Dai.jpg"))
            {
                // as generate returns a new IImage make sure we dispose of it
                using (Image destRound = img.Clone(x => x.ConvertToAvatar(new Size(200, 0))))
                {
                    destRound.Save("output/fb.jpg");
                }
            }
        }
    }

    public static class Helpers
    {
        public static IImageProcessingContext ConvertToAvatar(
            this IImageProcessingContext processingContext, 
            Size size)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            });
        }
    }
}
