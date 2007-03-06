using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

using System.Windows.Forms;
using Gav.Data;

namespace Gav.Graphics
{
    class Label
    {
        public string text;
        public float verticalPosition;
        public int colorARGB;
        public System.Drawing.Point defaultPosition;
        public int id;
        public bool visible;
        public bool selected;

        public Label(string aText, float aVerticalPosition, int aID)
            : this(aText, aVerticalPosition, aID, System.Drawing.Color.Blue)
        {
        }

        public Label(string aText, float aVerticalPosition, int aID, System.Drawing.Color aColor)
        {
            text = aText;
            verticalPosition = aVerticalPosition;
            colorARGB = aColor.ToArgb();
            id = aID;
            visible = true;
            selected = false;
        }

       
    };

    class TextLensSubComponent : VizSubComponent
    {
        private ParallelCoordinatesPlot plot;

        /// <summary>
        /// The default 3D Font used to draw the text on the viewport
        /// </summary>
        private Font d3dFont;

        /// <summary>
        /// The default Windows Font selected for the text
        /// </summary>
        private System.Drawing.Font systemFont;

        /// <summary>
        /// The default 3D Font used to draw the Lens text on the viewport
        /// </summary>
        private Font d3dLensFont;

        /// <summary>
        /// The default Windows Font selected for the Lens text
        /// </summary>
        private System.Drawing.Font systemLensFont;

        /// <summary>
        /// The default Font height
        /// </summary>
        private int defaultTextHeight;

        /// <summary>
        /// The Lens Font height
        /// </summary>
        private int lensTextHeight;

        /// <summary>
        /// The regular text color: without zoom/unselected
        /// </summary>
        private System.Drawing.Color visibleTextColor;

        /// <summary>
        /// The text color when the line is not selected on the PCPlot
        /// </summary>
        private System.Drawing.Color notVisibleTextColor;

        /// <summary>
        /// The text color when the line is selected on the PCPlot
        /// </summary>
        private System.Drawing.Color selectedTextColor;

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
            systemFont = new System.Drawing.Font("Verdana", 4);
            systemLensFont = new System.Drawing.Font("Verdana", 16);
            labels = new List<Label>();
            maxLabelWidth = 0.0f;
            mouseOverText = false;

            System.Drawing.SizeF size = graphicsObj.MeasureString("Foobar", systemFont);
            defaultTextHeight = (int)size.Height;

            size = graphicsObj.MeasureString("Foobar", systemLensFont);
            lensTextHeight = (int)size.Height;

            notVisibleTextColor = System.Drawing.Color.LightGray;
            selectedTextColor   = System.Drawing.Color.Blue;
            visibleTextColor    = System.Drawing.Color.Black;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        protected override void InternalInit(Microsoft.DirectX.Direct3D.Device device)
        {
            d3dFont = new Font(device, systemFont);
            d3dLensFont = new Font(device, systemLensFont);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void InternalInvalidate()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UpdateSize()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="verticalPosition"></param>
        public void AddLabel(string label, float verticalPosition, int id)
        {
            float width = graphicsObj.MeasureString(label, systemFont).Width;

            if (width > maxLabelWidth)
            {
                maxLabelWidth = width;
            }

            labels.Add(new Label(label, verticalPosition, id, this.visibleTextColor));
        }

        //public void setLabels(List<string> labels)
        //{
        //    _labels.AddRange(labels);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        private void InitLabelDefaultPosition(Label label)
        {
            //System.Drawing.SizeF size = graphicsObj.MeasureString(label.text, systemFont);
            int textHalfHeight = defaultTextHeight / 2;

            int yPos = plot.YPositionToScreen(plot.PadPositionY(label.verticalPosition)) - textHalfHeight;
            int xPos = plot.XPositionToScreen(plot.PadPositionX(0)) - (int)maxLabelWidth;

            label.defaultPosition = new System.Drawing.Point(xPos, yPos);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        protected override void InternalRender(Microsoft.DirectX.Direct3D.Device device)
        {

            if ( ! inited )
            {
                labelBottom = plot.YPositionToScreen(plot.PadPositionY(1.0f));
                labelTop = plot.YPositionToScreen(plot.PadPositionY(0.0f));
                labelRight = plot.XPositionToScreen(plot.PadPositionX(0));
                labelLeft = labelRight - (int)maxLabelWidth;

                // initialize every label's default position
                foreach (Label label in labels)
                {
                    InitLabelDefaultPosition(label);
                }

                inited = true;
            }

            List<int> selection = new List<int>();
            foreach (Label label in labels)
            {
                if (mouseOverText && (mouseY < label.defaultPosition.Y + defaultTextHeight) &&
                                    (mouseY > label.defaultPosition.Y))
                {
                    System.Drawing.Point p = new System.Drawing.Point(label.defaultPosition.X, label.defaultPosition.Y);
                    p.Y -= (lensTextHeight - defaultTextHeight) / 2;

                    d3dLensFont.DrawText(null, label.text, p,
                        System.Drawing.Color.Red.ToArgb());

                    // add the selected text to the plot list of selected lines
                    selection.Add(label.id);
                }
                else
                {
                    d3dFont.DrawText(null, label.text, label.defaultPosition, label.colorARGB);
                }
            }

            if (mouseOverText)
            {
                mouseOverText = false;
            }
        }

        /// <summary>
        /// Change the colors according to the lines selected on the PC Plot
        /// </summary>
        /// <param name="selectedIDs"></param>
        public void SelectionChanged(List<int> selectedIDs)
        {
            foreach (Label label in labels)
            {
                bool selected = selectedIDs.Contains(label.id);
                label.selected = selected;

                if (selected)
                {
                    if (label.visible)
                    {
                        label.colorARGB = selectedTextColor.ToArgb();
                    }
                }
                else
                {
                    if (label.visible)
                    {
                        label.colorARGB = visibleTextColor.ToArgb();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visibilityHandler"></param>
        public void VisibilityChanged(IndexVisibilityHandler visibilityHandler)
        {
            foreach (Label label in labels)
            {
                bool visible = visibilityHandler.GetVisibility(label.id);
                label.visible = visible;

                if (visible)
                {
                    //visible = true;

                    // if selected we don't want to override that color
                    if ( ! label.selected )
                    {
                        label.colorARGB = visibleTextColor.ToArgb();
                    }
                    else // if visible and selected, then show it selected
                    {
                        label.colorARGB = selectedTextColor.ToArgb();
                    }
                }
                else
                {
                    // NOTE: do not change the selected flag, so we can
                    // put it back to selected when it's visible again
                    //label.visible = false;
                    label.colorARGB = notVisibleTextColor.ToArgb();
                }
            }
        }

        public void RecomputeTextSize()
        {
            labelBottom = plot.YPositionToScreen(plot.PadPositionY(1.0f));
            labelTop = plot.YPositionToScreen(plot.PadPositionY(0.0f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override bool InternalMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            // if mouse if over the label area trigger the mouse boolean
            if ((e.X < labelRight) && (e.X > labelLeft) && (e.Y >= labelTop) && (e.Y <= labelBottom))
            {
                mouseOverText = true;
                mouseY = e.Y;
                plot.Invalidate();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override bool InternalMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
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