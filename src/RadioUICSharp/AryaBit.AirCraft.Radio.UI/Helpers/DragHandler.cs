using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AryaBit.AirCraft.Radio.UI
{
    class DragHandler
    {
        //public static void DragMoveWindow(System.Windows.Window window)
        //{
        //    DragMoveWindow(window, null);
        //}

        public static void DragMoveWindow(System.Windows.Window window,
            Func<bool> condition, Action dragStartCallback, Action<bool> dragEndCallback, UIElement[] excludedUiElements)
        {

            Point startPoint = new Point();
            bool hasMoved = false;

            window.PreviewMouseLeftButtonDown += (object sender, MouseButtonEventArgs e) =>
            {
                if (condition != null)
                {
                    if (!condition())
                        return;
                }
                hasMoved = false;
                startPoint = e.GetPosition(window);
                //int a = 10;
            };

            window.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                if (dragEndCallback != null)
                    dragEndCallback(hasMoved);
            };

            window.PreviewMouseMove += (object sender, MouseEventArgs e) =>
            {
                var currentPoint = e.GetPosition(window);

                if (excludedUiElements != null)
                {
                    foreach (UIElement uiElem in excludedUiElements)
                    {
                        if (uiElem.IsMouseDirectlyOver)
                        {
                            e.Handled = false;
                            return;
                        }
                    }
                }

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if ((Math.Abs(currentPoint.X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(currentPoint.Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                    {
                        if (dragStartCallback != null)
                            dragStartCallback();
                        // Prevent Click from firing
                        window.ReleaseMouseCapture();
                        hasMoved = true;
                        window.DragMove();

                    }
                }
            };


        }


    }
}
