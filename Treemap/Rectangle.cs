using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Microsoft.DirectX;

namespace Treemap
{
    class TreeRectangle
    {
        private float iFreeWidth;
        private float iFreeHeight;

        private float iWidth;
        private float iHeight;

        private Vector2 iLowerLeft;
        private Vector2 iUpperRight;

        /// <summary>
        /// Static scale
        /// </summary>
        private static Vector2 iScale;
        /// <summary>
        /// Static border
        /// </summary>
        private static float iBorder;

        private float iArea;
        List<TreeRectangle> iChildRectangles;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aX1">Lowerleft X</param>
        /// <param name="aY1">Lowerleft Y</param>
        /// <param name="aX2">UpperLeft X</param>
        /// <param name="aY2">UpperLeft Y</param>
        public TreeRectangle(float aArea, float aX1, float aY1, float aX2, float aY2)
        {
            iArea = aArea;
            iLowerLeft = new Vector2(aX1, aY1);
            iUpperRight = new Vector2(aX2, aY2);
            iScale = new Vector2(1, 1);
            iBorder = 2.0f;

            iChildRectangles = new List<TreeRectangle>();

            UpdateFreeSize();
            iWidth = iFreeWidth;
            iHeight = iFreeHeight;
        }

        /// <summary>
        /// Updates the free size of the rectangle
        /// </summary>
        private void UpdateFreeSize()
        {
            iFreeWidth = iUpperRight.X - iLowerLeft.X;
            iFreeHeight = iUpperRight.Y - iLowerLeft.Y;
        }

        /// <summary>
        /// Get the length of the minimum free side of the rectangle (height or width)
        /// </summary>
        /// <returns>Length of the minimum side</returns>
        public float MinSideLength()
        {
            return Math.Min(iFreeWidth, iFreeHeight);
        }

        /// <summary>
        /// Set the scales for the rectangle according to the available space
        /// </summary>
        /// <param name="aWidth">width in screen pixels</param>
        /// <param name="aHeight">height in screen pixels</param>
        public void SetScale(int aWidth, int aHeight)
        {
            iScale.X = aWidth / iWidth;
            iScale.Y = aHeight / iHeight;
            iBorder = (float)Math.Ceiling(Math.Max(iScale.X, iScale.Y) / 100.0f);
        }

        /// <summary>
        /// Draws the rectangle and its children
        /// </summary>
        /// <param name="aGraphics">the graphics container</param>
        public void Draw(Graphics aGraphics)
        {
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF(iLowerLeft.X * iScale.X, iLowerLeft.Y * iScale.Y, iFreeWidth * iScale.X - iBorder, iFreeHeight * iScale.Y - iBorder);

            //SolidBrush brush = new SolidBrush(Color.FromArgb(10, 10, 100));
            System.Drawing.Drawing2D.LinearGradientBrush brush = new
                       System.Drawing.Drawing2D.LinearGradientBrush(
                       rect,
                       Color.MidnightBlue, Color.Indigo, System.Drawing.Drawing2D.
                       LinearGradientMode.Vertical);
            
            aGraphics.FillRectangle( brush, rect );
            DrawValue(aGraphics);

            foreach (TreeRectangle rectangle in iChildRectangles)
            {
                rectangle.Draw(aGraphics);
            }
        }

        private void DrawValue(Graphics aGraphics)
        {
            SolidBrush brush = new SolidBrush(Color.White);
            float fontsize = 10.0f * iScale.Y / 100.0f;
            System.Drawing.Font font = new System.Drawing.Font(FontFamily.GenericMonospace, fontsize);
            aGraphics.DrawString(iArea.ToString(), font, brush, (iLowerLeft.X) * iScale.X + iBorder, (iLowerLeft.Y) * iScale.Y + iBorder);
        }

        /// <summary>
        /// Adds another row in the correct orientation (horizontal / vertical), according to the longest side.
        /// </summary>
        /// <param name="aRow">Data values</param>
        /// <returns>Size of the free space on this Rectangle</returns>
        public float LayOutRow(List<float> aRow)
        {
            float rowArea = Sum(aRow);

            //Adding the new row horizontally
            if (iFreeWidth < iFreeHeight)
            {
                float rowHeight = rowArea / iFreeWidth;
                float rowWidth = 0;
                float xOffset = 0;

                foreach (float area in aRow)
                {
                    rowWidth = area / rowHeight;
                    iChildRectangles.Add(new TreeRectangle(
                                  area
                        //lower left
                                , iLowerLeft.X + xOffset
                                , iLowerLeft.Y

                                //upper right
                                , iLowerLeft.X + xOffset + rowWidth
                                , iLowerLeft.Y + rowHeight)
                                );
                    xOffset += rowWidth;
                }

                //update the rectangle height
                iLowerLeft.Y += rowHeight;
            }
            //adding the new row vertically
            else
            {
                float rowWidth = rowArea / iFreeHeight;
                float rowHeight = 0;
                float yOffset = 0;

                foreach (float area in aRow)
                {
                    rowHeight = area / rowWidth;
                    iChildRectangles.Add(new TreeRectangle(
                                 area
                        //lower left
                               , iLowerLeft.X
                               , iLowerLeft.Y + yOffset

                               //upper right
                               , iLowerLeft.X + rowWidth
                               , iLowerLeft.Y + yOffset + rowHeight
                               )
                               );
                    yOffset += rowHeight;
                }
                //update the rectangle width
                iLowerLeft.X += rowWidth;
            }

            UpdateFreeSize();
            return MinSideLength();
        }

        private float Sum(List<float> aRow)
        {
            float sum = 0;
            foreach (float value in aRow)
            {
                sum += value;
            }
            return sum;
        }
    }
}
