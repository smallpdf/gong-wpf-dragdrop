using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop.Utilities;
using System.Windows.Controls.Primitives;
using System.Globalization;
//using System.Net.Http.Headers;

namespace GongSolutions.Wpf.DragDrop
{
    // This will draw the horizontal or vertical line in the destination
    public class DropTargetInsertionAdorner : DropTargetAdorner
    {
        public DropTargetInsertionAdorner(UIElement adornedElement, DropInfo dropInfo)
            : base(adornedElement, dropInfo)
        {
        }

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system.
        /// The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for
        /// later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var dropInfo = this.DropInfo;
            var itemsControl = dropInfo.VisualTarget as ItemsControl;

            if (itemsControl != null)
            {
                // Get the position of the item at the insertion index. If the insertion point is
                // to be after the last item, then get the position of the last item and add an 
                // offset later to draw it at the end of the list.
                ItemsControl itemParent;

                var visualTargetItem = dropInfo.VisualTargetItem;
                if (visualTargetItem != null)
                {
                    itemParent = ItemsControl.ItemsControlFromItemContainer(visualTargetItem);
                }
                else
                {
                    itemParent = itemsControl;
                }

                // this could be happen with a thread scenario where items are removed very quickly
                if (itemParent == null)
                {
                    return;
                }

                var itemsCount = itemParent.Items.Count;
                var index = Math.Min(dropInfo.InsertIndex, itemsCount - 1);

                var lastItemInGroup = false;
                var targetGroup = dropInfo.TargetGroup;
                if (targetGroup != null && targetGroup.IsBottomLevel && dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
                {
                    var indexOf = targetGroup.Items.IndexOf(dropInfo.TargetItem);
                    lastItemInGroup = indexOf == targetGroup.ItemCount - 1;
                    if (lastItemInGroup && dropInfo.InsertIndex != itemsCount)
                    {
                        index--;
                    }
                }

                var itemContainer = (UIElement)itemParent.ItemContainerGenerator.ContainerFromIndex(index);

                var showAlwaysDropTargetAdorner = itemContainer == null && DragDrop.GetShowAlwaysDropTargetAdorner(itemParent);
                if (showAlwaysDropTargetAdorner)
                {
                    itemContainer = itemParent;
                }

                if (itemContainer != null)
                {
                    var itemRect = new Rect(itemContainer.TranslatePoint(new Point(), this.AdornedElement), itemContainer.RenderSize);
                    Point point1,
                          point2;
                    double rotation = 0;

                    // I really don't know why I did this
                    //
                    // var viewportWidth = double.MaxValue;
                    // var viewportHeight = double.MaxValue;
                    // if (DropInfo.TargetScrollViewer != null)
                    // {
                    //     if (DropInfo.TargetScrollViewer.ScrollableWidth != 0)
                    //     {
                    //         viewportWidth = DropInfo.TargetScrollViewer.ViewportWidth;
                    //     }
                    //
                    //     if (DropInfo.TargetScrollViewer.ScrollableHeight != 0)
                    //     {
                    //         viewportHeight = DropInfo.TargetScrollViewer.ViewportHeight;
                    //     }
                    // }

                    var useImprovedVisualTargetInsertLine = dropInfo.VisualTargetInfo.UseImprovedVisualTargetInsertLine;  // SmallPdf


                    if (dropInfo.VisualTargetOrientation == Orientation.Vertical)
                    {
                        if ((dropInfo.InsertIndex == itemsCount) || lastItemInGroup)
                        {
                            if (itemsCount > 0)
                            {
                                itemRect.Y += itemContainer.RenderSize.Height;
                            }
                            else
                            {
                                if ((itemsControl as ListView)?.View is GridView)
                                {
                                    var header = itemsControl.GetVisualDescendent<GridViewHeaderRowPresenter>();
                                    if (header != null)
                                    {
                                        itemRect.Y += header.RenderSize.Height;
                                    }
                                }
                                else if (itemsControl is DataGrid)
                                {
                                    var header = itemsControl.GetVisualDescendent<DataGridColumnHeadersPresenter>();
                                    if (header != null)
                                    {
                                        itemRect.Y += header.RenderSize.Height;
                                    }
                                }

                                itemRect.Y += this.Pen.Thickness;
                            }
                        }

                        var itemRectRight = itemRect.Right; //Math.Min(itemRect.Right, viewportWidth);
                        var itemRectLeft = itemRect.X < 0 ? 0 : itemRect.X;

                        // Draw horizontal line
                        point1 = new Point(itemRectLeft, itemRect.Y);
                        point2 = new Point(itemRectRight, itemRect.Y);
                        if (useImprovedVisualTargetInsertLine)  // SmallPdf
                        {
                            point1.X = point1.X + dropInfo.VisualTargetInfo.HorizontalInsertLineLeft;
                            point2.X = point1.X + dropInfo.VisualTargetInfo.HorizontalInsertLineWidth;
                            CreateMoveHereLabel(drawingContext, point1.X + (point2.X - point1.X)/2, point1.Y - 4);
                        }
                    }
                    else
                    {
                        if (dropInfo.VisualTargetFlowDirection == FlowDirection.LeftToRight && dropInfo.InsertIndex == itemsCount)
                        {
                            if (itemsCount > 0)
                            {
                                itemRect.X += itemContainer.RenderSize.Width;
                            }
                            else
                            {
                                itemRect.X += this.Pen.Thickness;
                            }
                        }
                        else if (dropInfo.VisualTargetFlowDirection == FlowDirection.RightToLeft && dropInfo.InsertIndex != itemsCount)
                        {
                            if (itemsCount > 0)
                            {
                                itemRect.X += itemContainer.RenderSize.Width;
                            }
                            else
                            {
                                itemRect.X += this.Pen.Thickness;
                            }
                        }

                        var itemRectTop = itemRect.Y < 0 ? 0 : itemRect.Y;
                        var itemRectBottom = itemRect.Bottom; //Math.Min(itemRect.Bottom, viewportHeight);

                        // draw vertical line
                        point1 = new Point(itemRect.X, itemRectTop);
                        point2 = new Point(itemRect.X, itemRectBottom);
                        rotation = 90;

                        if (useImprovedVisualTargetInsertLine)  // SmallPdf
                        {
                            point1.Y = point1.Y + dropInfo.VisualTargetInfo.VerticalInsertLineTop;
                            point2.Y = point1.Y + dropInfo.VisualTargetInfo.VerticalInsertLineHeight;
                            CreateMoveHereLabel(drawingContext, point1.X + 1, point1.Y - 4);
                        }
                    }

                    drawingContext.DrawLine(this.Pen, point1, point2);

                    if (!useImprovedVisualTargetInsertLine)  // SmallPdf
                    {
                        this.DrawTriangle(drawingContext, point1, rotation);
                        this.DrawTriangle(drawingContext, point2, 180 + rotation);
                    }
                }
            }
        }

        #region CreteMoveHereLabel

        const int TriangleHeight = 6;  // Verified
        const int TriangleWidth = 10;
        const int BlackRectangleHeight = 24;  // Verified
        const int BlackRectangleWidth = 85;   // Verified

        private void CreateMoveHereLabel(DrawingContext drawingContext, double bottomCenterX, double bottomCenterY)  // SmallPdf
        {
            CreateBlackRectangle(drawingContext, bottomCenterX, bottomCenterY);
            CreateBlackTriangle(drawingContext, bottomCenterX, bottomCenterY);
            CreateWhiteText(drawingContext, bottomCenterX, bottomCenterY);
        }

        private static void CreateBlackRectangle(DrawingContext drawingContext, double bottomCenterX, double bottomCenterY)
        {
            Rect rect = new Rect()
            {
                X = bottomCenterX - BlackRectangleWidth / 2,
                Y = bottomCenterY - TriangleHeight - BlackRectangleHeight,
                Width = BlackRectangleWidth,
                Height = BlackRectangleHeight
            };
            var brush = new SolidColorBrush(Colors.Black);
            var pen = new Pen(brush, 1);
            drawingContext.DrawRoundedRectangle(brush, pen, rect, 5, 5);
        }

        private void CreateBlackTriangle(DrawingContext drawingContext, double bottomCenterX, double bottomCenterY)
        {
            var yTopTriangle = bottomCenterY - TriangleHeight;
            var xLeftTriangle = bottomCenterX - TriangleWidth / 2;
            var point1 = new Point(bottomCenterX, yTopTriangle);
            var rotation = 90;
            this.DrawTriangle(drawingContext, point1, rotation, new SolidColorBrush(Colors.Black));
        }

        private static void CreateWhiteText(DrawingContext drawingContext, double bottomCenterX, double bottomCenterY)
        {
            const int LeftMargin = 12;
            const int TopMargin = 4;
            const int FontSize = 13;  /* 14 was in the original design */
            const string FontName = "MS Sans Serif";   // SourceSansPro-Regular was in the original design

            var origin = new Point(bottomCenterX - BlackRectangleWidth / 2 + LeftMargin, bottomCenterY - TriangleHeight - BlackRectangleHeight + TopMargin);
            var formattedText = new FormattedText("Move here", CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight, new Typeface(FontName), FontSize, Brushes.White);
            
            drawingContext.DrawText(formattedText, origin);
        }

        #endregion

        private void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation, Brush brush = null)
        {
            if (brush == null)
                brush = this.Pen.Brush;
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(rotation));

            drawingContext.DrawGeometry(brush, null, m_Triangle);   // Smallpdf

            drawingContext.Pop();
            drawingContext.Pop();
        }

        static DropTargetInsertionAdorner()
        {
            // Create the pen and triangle in a static constructor and freeze them to improve performance.
            const int triangleSize = 5;

            var firstLine = new LineSegment(new Point(0, -triangleSize), false);
            firstLine.Freeze();
            var secondLine = new LineSegment(new Point(0, triangleSize), false);
            secondLine.Freeze();

            var figure = new PathFigure { StartPoint = new Point(triangleSize, 0) };
            figure.Segments.Add(firstLine);
            figure.Segments.Add(secondLine);
            figure.Freeze();

            m_Triangle = new PathGeometry();
            m_Triangle.Figures.Add(figure);
            m_Triangle.Freeze();
        }

        private static readonly PathGeometry m_Triangle;
    }
}