using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

using System.Windows.Forms;

namespace Gav.Graphics
{
    class Label
    {
        public string text;
        public float verticalPosition;
        public int colorARGB;


        public Label(string aText, float aVerticalPosition)
            : this(aText, aVerticalPosition, System.Drawing.Color.Blue)
        {
        }

        public Label(string aText, float aVerticalPosition, System.Drawing.Color aColor)
        {
            text = aText;
            verticalPosition = aVerticalPosition;
            colorARGB = aColor.ToArgb();
        }

       
    };

    class TextLensSubComponent : VizSubComponent
    {
        private ParallelCoordinatesPlot plot;

        private Font d3dFont;
        private System.Drawing.Font systemFont;

        private Panel panel;

        private System.Drawing.Graphics graphicsObj;

        private List<Label> labels;

        // Text Hovering variables
        bool mouseOverText;
        int labelBottom;
        int labelTop;
        int labelRight;
        int labelLeft;
        int mouseY;

        /************************************************************************/
        /* XXX                                                                  */
        /************************************************************************/
        bool inited = false;
        
        /// <summary>
        /// The maximum width of all labels
        /// </summary>
        private float maxLabelWidth;

        public TextLensSubComponent(ParallelCoordinatesPlot aPlot, Panel aPanel)
        {
            plot = aPlot;
            panel = aPanel;
            graphicsObj = panel.CreateGraphics();
            systemFont = new System.Drawing.Font("Verdana", 6);
            labels = new List<Label>();
            maxLabelWidth = 0.0f;
            mouseOverText = false;
        }

        protected override void InternalInit(Microsoft.DirectX.Direct3D.Device device)
        {
            d3dFont = new Font(device, systemFont);
        }

        protected override void InternalInvalidate()
        {
        }

        protected override void UpdateSize()
        {

        }

        public void AddLabel(string label, float verticalPosition)
        {
            float width = graphicsObj.MeasureString(label, systemFont).Width;
            
            if (width > maxLabelWidth)
            {
                maxLabelWidth = width;
            }
            
            labels.Add(new Label(label, verticalPosition));
        }

        //public void setLabels(List<string> labels)
        //{
        //    _labels.AddRange(labels);
        //}

        

        protected override void InternalRender(Microsoft.DirectX.Direct3D.Device device)
        {

            if ( ! inited )
            {
                labelBottom = plot.YPositionToScreen(plot.PadPositionY(1.0f));
                labelTop = plot.YPositionToScreen(plot.PadPositionY(0.0f));
                labelRight = plot.XPositionToScreen(plot.PadPositionX(0));
                labelLeft = labelRight - (int)maxLabelWidth;
                inited = true;
            }

            foreach (Label label in labels)
            {
                int yPos = plot.YPositionToScreen(plot.PadPositionY(label.verticalPosition));
                System.Drawing.SizeF size = graphicsObj.MeasureString(label.text, systemFont);

                int xPos = plot.XPositionToScreen(plot.PadPositionX(0)) - (int)maxLabelWidth;
                int textHalfHeight = (int)(size.Height / 2);

                System.Drawing.Point position = new System.Drawing.Point(xPos, yPos - textHalfHeight);

                if (mouseOverText && (mouseY < yPos + textHalfHeight) && (mouseY > yPos - textHalfHeight))
                {
                    d3dFont.DrawText(null, label.text, position, System.Drawing.Color.Red.ToArgb());
                }
                else
                {
                    d3dFont.DrawText(null, label.text, position, label.colorARGB);
                }
            }

            if (mouseOverText)
            {
                //plot.Invalidate();
                mouseOverText = false;
            }

            //int yPos = plot.YPositionToScreen(plot.PadPositionY(0.5f));
            //System.Drawing.SizeF size = graphicsObj.MeasureString("Hello", systemFont);

            //System.Drawing.Point position = new System.Drawing.Point(100, yPos - (int)(size.Height / 2));
            //d3dFont.DrawText(null, "Hello", position, System.Drawing.Color.Blue.ToArgb());

        }

        protected override bool InternalMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.X < labelRight) && (e.X > labelLeft) && (e.Y > labelTop) && (e.Y < labelBottom))
            {
                mouseOverText = true;
                mouseY = e.Y;
                plot.Invalidate();
                return true;
            }

            return false;
        }

        protected override bool InternalMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            return false;
        }

        protected override bool InternalMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            return false;
        }

        //private void RenderExtraText()
        //{

        //    foreach (PCText text in _pcTextList)
        //    {
        //        int yPos = plot.YPositionToScreen(plot.PadPositionY(0.5f));
        //        //int ypos = YPositionToScreen(PadPositionY(text.VerticalPosition));

        //        SizeF size = _debugGraphics.MeasureString(text.Text, text.SystemFont);

        //        ypos -= (int)(size.Height / 2.0f);

        //        if (text.Alignment == TextRelativePosition.Left)
        //        {
        //            int xpos = 0;
        //            text.D3DFont.DrawText(null, text.Text, new Rectangle(new Point(xpos, ypos), new Size(XPositionToScreen(PadPositionX(_lineXPositions[0])) - 5, (int)size.Height)), DrawTextFormat.Top | DrawTextFormat.Right, text.Color);
        //        }
        //        else
        //        {
        //            int xpos = 5 + XPositionToScreen(PadPositionX(_lineXPositions[_lineXPositions.Count - 1]));
                    
        //            text.D3DFont.DrawText(null, text.Text, 
        //                new Rectangle(new Point(xpos, ypos), new Size(PaddingRight - 5, (int)size.Height)), 
        //                DrawTextFormat.Top | DrawTextFormat.Left, text.Color);
        //        }
        //    }
        //} 
    }
}