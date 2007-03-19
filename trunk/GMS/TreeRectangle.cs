using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Microsoft.DirectX;
using Gav.Data;

namespace GMS
{
    class TreeRectangle
    {
        /// <summary>
        /// The remaining space when squarifying the current
        /// rectangle
        /// </summary>
        private float iFreeWidth;
        private float iFreeHeight;

        private float iWidth;
        private float iHeight;

        private Vector2 iLowerLeft;
        private Vector2 iUpperRight;

        /// <summary>
        /// The current position of the lower left corner
        /// while squarifying the current rectangle
        /// </summary>
        private Vector2 iCurrentLowerLeft;

        /// <summary>
        /// Static scale
        /// </summary>
        private static Vector2 iScale;
        /// <summary>
        /// Static border
        /// </summary>
        private static float iBorder;

        //DATA:
        private float iArea;
        private object iData;
        private string iLabel;

        /************************************************************************/
        /* TODO: REMOVE AND FIND A BETTER WAY OF DOING IT!!!!!                  */
        /************************************************************************/
        public int id;

        List<TreeRectangle> iChildRectangles;


        public TreeRectangle(float aArea)
            : this( aArea, 0.0F, 0.0F, 0.0F, 0.0F)
        { 
        }

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
            iLabel = "";
            iScale = new Vector2(1, 1);
            iBorder = 2.0f;

            iChildRectangles = new List<TreeRectangle>();
            SetSize(aX1, aY1, aX2, aY2);
        }

        public List<TreeRectangle> GetChildren()
        {
            return iChildRectangles;
        }


        public float GetArea()
        {
            return iArea;
        }

        public void SetSize(float aX1, float aY1, float aX2, float aY2)
        {
            iLowerLeft = new Vector2(aX1, aY1);
            iCurrentLowerLeft = new Vector2(aX1, aY1);
            iUpperRight = new Vector2(aX2, aY2);

            UpdateFreeSize();
            iWidth = iFreeWidth;
            iHeight = iFreeHeight;
        }

        public string Label
        {
            get
            {
                return iLabel;
            }
            set
            {
                iLabel = value;
            }
            
        }

        public void SetData(object aData)
        {
            iData = aData;
        }

        /// <summary>
        /// Updates the free size of the rectangle
        /// </summary>
        private void UpdateFreeSize()
        {
            //iFreeWidth = iUpperRight.X - iLowerLeft.X;
            //iFreeHeight = iUpperRight.Y - iLowerLeft.Y;
            iFreeWidth = iUpperRight.X - iCurrentLowerLeft.X;
            iFreeHeight = iUpperRight.Y - iCurrentLowerLeft.Y;

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
        /// Add an existing rectangle to current rectangles childrens
        /// </summary>
        /// <param name="aRectangle">a rectangle to be added to the child list</param>
        public void AddRectangle( TreeRectangle aRectangle )
        {
            iArea += aRectangle.iArea;
            iChildRectangles.Add(aRectangle);
        }


        /// <summary>
        /// Draws the rectangle and its children
        /// </summary>
        /// <param name="aGraphics">the graphics container</param>
        public void Draw(Graphics aGraphics, IColorMap aColorMap)
        {
            foreach (TreeRectangle rectangle in iChildRectangles)
            {
                rectangle.DrawChild(aGraphics, aColorMap);
            }

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                (int)(iLowerLeft.X * iScale.X),
                (int)(iLowerLeft.Y * iScale.Y),
                (int)(iWidth * iScale.X),
                (int)(iHeight * iScale.Y));

            if (rect.Height <= 1)
            {
                return;
            }

            //System.Drawing.Drawing2D.LinearGradientBrush brush = new
            //           System.Drawing.Drawing2D.LinearGradientBrush(
            //           rect,
            //           Color.MidnightBlue, Color.Indigo, 
            //           System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            
            //aGraphics.FillRectangle( brush, rect );

            SolidBrush borderPen = new SolidBrush(Color.YellowGreen);
            Pen pen = new Pen(borderPen, iBorder);
            aGraphics.DrawRectangle(pen, rect);

            borderPen.Dispose();
            pen.Dispose();

            DrawLabel(aGraphics);
        }

        private void DrawChild(Graphics aGraphics, IColorMap aColorMap)
        {
            System.Drawing.RectangleF fillRectangle = new System.Drawing.RectangleF(
                (iLowerLeft.X * iScale.X),
                (iLowerLeft.Y * iScale.Y),
                (iWidth * iScale.X),
                (iHeight * iScale.Y));

            if (fillRectangle.Height <= 1)
            {
                return;
            }


            // Last level: Draw Label and Fill Rectangle
            if (iChildRectangles.Count == 0)
            {
                int amount = aColorMap.GetColors().GetLength(0);
                if (aColorMap.GetColors().GetLength(0) < id) 
                {
                    id = 0;
                }
                System.Drawing.Drawing2D.LinearGradientBrush brush = new
                       System.Drawing.Drawing2D.LinearGradientBrush(
                       fillRectangle,
                       aColorMap.GetColor(id), Color.Black,
                       System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                aGraphics.FillRectangle(brush, fillRectangle);
                
                
                DrawLabel(aGraphics);
            }
            else
            {
                // Call children recursively
                foreach (TreeRectangle rectangle in iChildRectangles)
                {
                    rectangle.DrawChild(aGraphics, aColorMap);
                }

                System.Drawing.Rectangle borderRectangle = new System.Drawing.Rectangle(
                    (int)(iLowerLeft.X * iScale.X),
                    (int)(iLowerLeft.Y * iScale.Y),
                    (int)(iWidth * iScale.X),
                    (int)(iHeight * iScale.Y));
                
                SolidBrush borderPen = new SolidBrush(Color.White);
                Pen pen = new Pen(borderPen, iBorder);
                aGraphics.DrawRectangle(pen, borderRectangle);
            }
        }

        /// <summary>
        /// Draw the label of the rectangle
        /// </summary>
        /// <param name="aGraphics"></param>
        private void DrawLabel(Graphics aGraphics)
        {
            SolidBrush brush = new SolidBrush(Color.White);
            float fontsize = (float)Math.Log(iHeight) * iScale.Y;
            fontsize = (fontsize <= 1.0F)? 1.0F : fontsize;
            System.Drawing.Font font = new System.Drawing.Font("Arial Narrow", fontsize, FontStyle.Bold);
            aGraphics.DrawString(iLabel, font, brush, (iLowerLeft.X) * iScale.X + iBorder, (iLowerLeft.Y) * iScale.Y + iBorder);
        }

        /// <summary>
        /// Returns the leaf Rectangle that contains the location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public TreeRectangle LocationInsideRectangle(Point location)
        {
            Vector2 lowerLeft = new Vector2(iLowerLeft.X * iScale.X, iLowerLeft.Y * iScale.Y);
            Vector2 upperRight = new Vector2(iUpperRight.X * iScale.X, iUpperRight.Y * iScale.Y);

            if (lowerLeft.X <= location.X && upperRight.X >= location.X
                && lowerLeft.Y <= location.Y && upperRight.Y >= location.Y)
            {
                if (iChildRectangles.Count == 0)
                {
                    return this;
                }
                else
                {
                    foreach (TreeRectangle child in iChildRectangles)
                    {
                        TreeRectangle result = child.LocationInsideRectangle(location);
                        
                        // if found
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Adds another row in the correct orientation (horizontal / vertical), according to the longest side.
        /// </summary>
        /// <param name="aRow">Data values</param>
        /// <returns>Size of the free space on this Rectangle</returns>
        public float LayOutRow(List<TreeRectangle> aRow)
        {
            float rowArea = Sum(aRow);

            //Adding the new row horizontally
            if (iFreeWidth < iFreeHeight)
            {
                float rowHeight = rowArea / iFreeWidth;
                float rowWidth = 0;
                float xOffset = 0;

                foreach (TreeRectangle rectangle in aRow)
                {
                    rowWidth = rectangle.GetArea() / rowHeight;
                    rectangle.SetSize(
                                //lower left
                                iCurrentLowerLeft.X + xOffset
                                , iCurrentLowerLeft.Y

                                //upper right
                                , iCurrentLowerLeft.X + xOffset + rowWidth
                                , iCurrentLowerLeft.Y + rowHeight
                                );
                    xOffset += rowWidth;
                }

                //update the rectangle height
                iCurrentLowerLeft.Y += rowHeight;
            }
            //adding the new row vertically
            else
            {
                float rowWidth = rowArea / iFreeHeight;
                float rowHeight = 0;
                float yOffset = 0;

                foreach (TreeRectangle rectangle in aRow)
                {
                    rowHeight = rectangle.GetArea() / rowWidth;
                    rectangle.SetSize(
                        //lower left
                               iCurrentLowerLeft.X
                               , iCurrentLowerLeft.Y + yOffset

                               //upper right
                               , iCurrentLowerLeft.X + rowWidth
                               , iCurrentLowerLeft.Y + yOffset + rowHeight
                               );
                    yOffset += rowHeight;
                }
                //update the rectangle width
                iCurrentLowerLeft.X += rowWidth;
            }

            UpdateFreeSize();
            return MinSideLength();
        }

        /// <summary>
        /// Sum of a tree list
        /// </summary>
        /// <param name="aRow"></param>
        /// <returns></returns>
        private float Sum(List<TreeRectangle> aRow)
        {
            float sum = 0;
            foreach (TreeRectangle rectangle in aRow)
            {
                sum += rectangle.iArea;
            }
            return sum;
        }
    }
}
