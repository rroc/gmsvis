using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


using Microsoft.DirectX;

using Gav.Data;
using Gav.Graphics;

namespace Treemap
{
    class TreeMap : VizComponent
    {
        TreeRectangle iRectangle;

        List<float> iData;

        /// <summary>
        /// Constructor
        /// </summary>
        public TreeMap()
        {
            SetData();
            BuildTreeMap();
        }

        public void SetData()
        {
            iData = new List<float>();
            iData.Add(6.0F);
            iData.Add(6.0F);
            iData.Add(4.0F);
            iData.Add(3.0F);
            iData.Add(2.0F);
            iData.Add(2.0F);
            iData.Add(1.0F);

            float sum = Sum(iData);
            int height = (int)(Math.Sqrt(sum));
            float width = sum / height;
            iRectangle = new TreeRectangle(sum, 0, 0, width, height);
        }

        public void BuildTreeMap()
        {
            List<float> row = new List<float>();
            Squarify(iData, row, iRectangle.MinSideLength());
        }

        public void UpdateScale( int aWidth, int aHeight )
        {
            iRectangle.SetScale(aWidth, aHeight);
        }

        public void DrawTree( Graphics aGraphics)
        {
            iRectangle.Draw(aGraphics);
        }

        /// <summary>
        /// Split the input values to the rectangle
        /// </summary>
        /// <param name="aChildren">input values(the tree) </param>
        /// <param name="aRow">The current work row</param>
        /// <param name="aWidth">The length of the current rectangle's max side</param>
        public void Squarify( List<float> aChildren, List<float> aRow, float aWidth )
        {
            if( 0 == aChildren.Count )
            {
                iRectangle.LayOutRow(aRow);
                return;
            }

            float head = aChildren[0];

            if( ContinueRow( aRow, head, aWidth) )
            {
                aRow.Add( head );
                aChildren.RemoveAt(0);
                Squarify( aChildren, aRow, aWidth);
            }
            else
            {
                float size = iRectangle.LayOutRow( aRow );
                Squarify(aChildren, new List<float>(), size );
            }

        }

        /// <summary>
        /// Checks whether the row can be continued, or is it better to start a new one.
        /// </summary>
        /// <param name="aRow">List of values</param>
        /// <param name="aHead">The proposed new row member</param>
        /// <param name="aWidth">Width of the current cell</param>
        /// <returns>True if the row should be continued</returns>
        private bool ContinueRow( List<float> aRow, float aHead, float aWidth )
        {
            int count = aRow.Count;
            if( count == 0)
            {
                return true;
            }
            float widthSqr = aWidth * aWidth;
            
            //First ratio
            float sum = Sum( aRow );
            float sumSqr = sum * sum;
            float rMax = aRow[0];
            float rMin = aRow[count - 1];
            float max = (widthSqr * rMax)/sumSqr;
            float min = sumSqr/(widthSqr*rMin);
            float firstMax = Math.Max( max, min);

            //Second ratio
            sum += aHead;
            sumSqr = sum * sum;
            rMax = Math.Max( rMax, aHead );
            rMin = Math.Min( rMin, aHead);
            max = (widthSqr * rMax)/sumSqr;
            min = sumSqr/(widthSqr*rMin);
            float secondMax = Math.Max(max, min);

            //does the aspect ratio improve or not?
            return firstMax >= secondMax;
        }

        /// <summary>
        /// Sum of a list
        /// </summary>
        /// <param name="aRow"></param>
        /// <returns></returns>
        private float Sum(List<float> aRow)
        {
            float sum = 0;
            foreach (float value in aRow)
            {
                sum += value;
            }
            return sum;
        }

        protected override void InternalInit(Microsoft.DirectX.Direct3D.Device device)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void InternalInvalidate()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void InternalMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void InternalMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void InternalMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void InternalRender(Microsoft.DirectX.Direct3D.Device device)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void InternalUpdateSize()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
