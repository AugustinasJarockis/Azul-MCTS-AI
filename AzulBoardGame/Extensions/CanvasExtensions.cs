using System.Windows;
using System.Windows.Controls;

namespace AzulBoardGame.Extensions
{
    internal static class CanvasExtensions
    {
        public static void SetRelativePos(this Canvas canvas, UIElement element, double xPos, double yPos) {
            var height = canvas.ActualHeight;
            var width = canvas.ActualWidth;

            Canvas.SetTop(element, yPos * height);
            Canvas.SetLeft(element, xPos * width);
        }

        public static void SetRelativeDimensions(this Canvas canvas, FrameworkElement element, double height, double width) {
            element.Height = height * canvas.ActualHeight;
            element.Width = width * canvas.ActualWidth;
        }

        public static void SetRelativePosCentered(
            this Canvas canvas,
            FrameworkElement element,
            double xPos,
            double yPos,
            double height,
            double width
            ) {
            element.Height = height * canvas.ActualHeight;
            element.Width = width * canvas.ActualWidth;
            Canvas.SetTop (element, (yPos - height / 2) * canvas.ActualHeight);
            Canvas.SetLeft(element, (xPos -  width / 2) * canvas.ActualWidth);
        }

        public static void SetRelativePosCenteredSquare(
            this Canvas canvas,
            FrameworkElement element,
            double xPos,
            double yPos,
            double length
            ) {
            element.Height = length * canvas.ActualHeight;
            element.Width = element.Height;
            Canvas.SetTop(element, (yPos - length / 2.0) * canvas.ActualHeight);
            Canvas.SetLeft(element, xPos * canvas.ActualWidth - element.Width / 2);
        }
    }
}
