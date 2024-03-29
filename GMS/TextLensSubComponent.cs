using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

using System.Windows.Forms;
using Gav.Data;
using GMS;

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

    /// <summary>
    /// 
    /// </summary>
    class TextLensSubComponent : VizSubComponent
    {
        #region Private Attributes
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        private Panel iPanel;

        /// <summary>
        /// 
        /// </summary>
        private System.Drawing.Graphics graphicsObj;

        /// <summary>
        /// The list of labels
        /// </summary>
        private List<Label> labels;

        private static float minTextSize = 1.0f;
        private static float maxTextSize = 8.0f;
        private static float minLensTextSize = 12.0f;
        private static float maxLensTextSize = 14.0f;

        // Text Hovering variables
        private int labelBottom;
        private int labelTop;
        private int labelRight;
        private int labelLeft;
        private int mouseX;
        private int mouseY;

        // Bools used in InternalRender()
        bool iInited = false;
        bool iSizeUpdated = false;

        /// <summary>
        /// The maximum width of all labels
        /// </summary>
        private float iMaxLabelWidth;

        // Mouse state variables
        private bool mouseOverText;
        private MouseButtons lastButtonPressed;

        private bool iLabelZoomed;

        GMSDocument iDoc;

        #endregion

        public TextLensSubComponent(ParallelCoordinatesPlot aPlot, Panel aPanel, GMSDocument aDoc)
        {
            plot = aPlot;
            iPanel = aPanel;
            graphicsObj = iPanel.CreateGraphics();
            iLabelZoomed = false;
            
            labels = new List<Label>();
            iMaxLabelWidth = 0.0f;
            mouseOverText = false;

            notVisibleTextColor = System.Drawing.Color.LightGray;
            selectedTextColor   = System.Drawing.Color.Red;
            visibleTextColor    = System.Drawing.Color.Black;

            lastButtonPressed = MouseButtons.None;

            iDoc = aDoc;
            iDoc.Picked += new EventHandler<IndexesPickedEventArgs>(DocumentPicked);
            
        }

        void DocumentPicked(object sender, IndexesPickedEventArgs e)
        {
            SelectionChanged(e.PickedIndexes);
        }

        /// <summary>
        /// Computes the optimal size for the Lens, given the default bounds
        /// </summary>
        /// <param name="desiredSize"></param>
        /// <returns></returns>
        private float GetBoundedLensTextSize(float desiredSize)
        {
            float size = minLensTextSize;

            if (desiredSize > maxLensTextSize)
            {
                size = maxLensTextSize;
            }
            else if (desiredSize < minLensTextSize)
            {
                size = minLensTextSize;
            }

            return size;
        }

        /// <summary>
        /// Computes the optimal size for the regular text, given the default bounds
        /// </summary>
        /// <param name="desiredSize"></param>
        /// <returns></returns>
        private float GetBoundedTextSize(float desiredSize)
        {
            float size = desiredSize;

            if (desiredSize > maxTextSize)
            {
                size = maxTextSize;
            }
            else if (desiredSize < minTextSize)
            {
                size = minTextSize;
            }

            return size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        private void InitFonts(Microsoft.DirectX.Direct3D.Device device)
        {

            ComputeLensVerticalBounds();

            // Add 10% to provide some some space between them
            float nLabels = (float)labels.Count * 1.1f;

            float height = Math.Abs(labelTop - labelBottom) / nLabels;
            
            // Limit the text size
            height = GetBoundedTextSize(height);
            float lensTextSize = GetBoundedLensTextSize(height * 4.0f);

            if (systemLensFont != null && systemFont != null)
            {
                systemLensFont.Dispose();
                systemFont.Dispose();
            }

            // The two types of font: Lens and Regular Text
            systemFont      = new System.Drawing.Font("Verdana", height);
            systemLensFont  = new System.Drawing.Font("Verdana", lensTextSize, System.Drawing.FontStyle.Bold);

            // Compute text height sizes
            System.Drawing.SizeF size = graphicsObj.MeasureString("Foobar", systemFont);
            defaultTextHeight = (int)size.Height;

            size = graphicsObj.MeasureString("Foobar", systemLensFont);
            lensTextHeight = (int)size.Height;

            // if previously created, discard them
            if (d3dFont != null && d3dLensFont != null)
            {
                d3dFont.Dispose();
                d3dLensFont.Dispose();
            }

            // Create the new D3D Fonts
            d3dFont     = new Font(device, systemFont);
            d3dLensFont = new Font(device, systemLensFont);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        protected override void InternalInit(Microsoft.DirectX.Direct3D.Device device)
        {
            //ComputeMaxLabelWidth();
            InitFonts(device);

            // NOTE: invalidate maxLabelWidth because we might be shrinking the text
            // and also, it must be done before the new LensBounds are computed
            ComputeMaxLabelWidth();

            // Compute the new bounds: given the new text sizes
            ComputeLensHorizontalBounds();

            // initialize every label's default position
            foreach (Label label in labels)
            {
                InitLabelDefaultPosition(label);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void InternalInvalidate()
        {
        }

        /// <summary>
        /// Used as a handler for an update size: MUST BE REFACTORED
        /// </summary>
        private void OnSizeUpdate(Microsoft.DirectX.Direct3D.Device device)
        {
            InitFonts(device);

            // NOTE: invalidate maxLabelWidth because we might be shrinking the text
            // and also, it must be done before the new LensBounds are computed
            ComputeMaxLabelWidth();

            // Compute the new bounds: given the new text sizes
            ComputeLensHorizontalBounds();

            // initialize every label's default position
            foreach (Label label in labels)
            {
                InitLabelDefaultPosition(label);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UpdateSize()
        {
            iSizeUpdated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="verticalPosition"></param>
        public void AddLabel(string label, float verticalPosition, int id)
        {
            labels.Add(new Label(label, verticalPosition, id, this.visibleTextColor));
        }

        /// <summary>
        /// NOTE: Must be called after an update to Label Widths
        /// </summary>
        /// <param name="label"></param>
        private void InitLabelDefaultPosition(Label label)
        {
            int textHalfHeight = defaultTextHeight / 2;

            int yPos = plot.YPositionToScreen(plot.PadPositionY(label.verticalPosition)) - textHalfHeight;
            int axisPos = 0;

            if (iInited)
            {
                List<float> axesPositions = plot.GetAxisXPositions();
                axisPos = plot.XPositionToScreen(axesPositions[0]);
            }
            else
            {
                axisPos = plot.XPositionToScreen(plot.PadPositionX(0));
            }

            int xPos = axisPos - (int)iMaxLabelWidth;

            label.defaultPosition = new System.Drawing.Point(xPos, yPos);
        }


        /// <summary>
        /// Computes, given the current window, the labels horizontal bounds (according to padding)
        /// NOTE: Depends on Max Label Width
        /// </summary>
        private void ComputeLensHorizontalBounds()
        {
            const int offset = 5;
            if (iInited)
            {
                List<float> axesPositions = plot.GetAxisXPositions();
                labelRight = plot.XPositionToScreen(axesPositions[0]) + offset;
            }
            else
            {
                labelRight = plot.XPositionToScreen(plot.PadPositionX(0)) + offset;
            }

            labelLeft = labelRight - (int)iMaxLabelWidth;
        }

        /// <summary>
        /// Computes, given the current window, the labels vertical bounds (according to padding)
        /// </summary>
        private void ComputeLensVerticalBounds()
        {
            labelBottom = plot.YPositionToScreen(plot.PadPositionY(1.0f));
            labelTop    = plot.YPositionToScreen(plot.PadPositionY(0.0f));
        }

        /// <summary>
        /// Computes the Labels max width
        /// </summary>
        private void ComputeMaxLabelWidth()
        {
            iMaxLabelWidth = 0;

            // Compute Label
            foreach (Label label in labels)
            {
                float width = graphicsObj.MeasureString(label.text, systemFont).Width;

                if (width > iMaxLabelWidth)
                {
                    iMaxLabelWidth = width;
                }
            }
            plot.PaddingLeft = (int)iMaxLabelWidth + 10;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        protected override void InternalRender(Microsoft.DirectX.Direct3D.Device device)
        {
            //// If initalized
            //if ( ! inited )
            //{
            //    OnSizeUpdate(device);
            //    inited = true;
            //}
            
            // If the panel has been resized
            if (!iInited || iSizeUpdated)
            {
                ComputeLensHorizontalBounds();
                iSizeUpdated = false;
                iInited = true;
                OnSizeUpdate(device);
            }

            bool labelMagnified = false;

            List<int> selection = new List<int>();
            foreach (Label label in labels)
            {
                // NOTE: just decreasing 30% of the width because of the text overlaps
                if (mouseOverText && (mouseY < label.defaultPosition.Y + defaultTextHeight) &&
                            (mouseY > label.defaultPosition.Y) && ! labelMagnified)
                {
                    System.Drawing.Point p = new System.Drawing.Point(label.defaultPosition.X, label.defaultPosition.Y);
                    p.Y -= (lensTextHeight - defaultTextHeight) / 2;

                    d3dLensFont.DrawText(null, label.text, p, label.colorARGB ); //System.Drawing.Color.Red);

                    labelMagnified = true;
                    iLabelZoomed = true;
                }
                else
                {
                    d3dFont.DrawText(null, label.text, label.defaultPosition, label.colorARGB);
                }
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
            else
            {
                mouseOverText = false;
                if( iLabelZoomed )
                {
                    iLabelZoomed = false;
                    plot.Invalidate();
                }
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
            lastButtonPressed = Control.MouseButtons;
            if (lastButtonPressed == MouseButtons.Left)
            {
                mouseX = e.X;
                mouseY = e.Y;

                if (mouseOverText)
                {

                    List<int> selection = new List<int>();
                    foreach (Label label in labels)
                    {
                        // NOTE: just decreasing 30% of the width because of the text overlaps
                        if (mouseOverText &&
                            (mouseY < label.defaultPosition.Y + defaultTextHeight) &&
                            (mouseY > label.defaultPosition.Y))
                        {
                            // add the selected text to the plot list of selected lines
                            if (this.lastButtonPressed == MouseButtons.Left)
                            {
                                selection.Add(label.id);
                                label.selected = true;
                            }

                            break;
                        }
                    }

                    // if the label was selected/picked
                    if (selection.Count != 0)
                    {
                        // if CTRL is pressed, add the line to the selection
                        Keys keys = Control.ModifierKeys;
                        bool add = (keys == Keys.Control);

                        iDoc.SetSelectedItems(selection, add, true);
                    }

                    return true;
                }
                
                // NOTE: NOT returning because parent (PC plot) must handle the selection as well 
            }
            else if (lastButtonPressed == MouseButtons.Right && mouseOverText)
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override bool InternalMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            List<float> axesPositions = plot.GetAxisXPositions();
            
            if (lastButtonPressed== MouseButtons.Right)
            {
                // Recompute the new bounds due to the axis displacement
                iSizeUpdated = true;
            }

            lastButtonPressed = MouseButtons.None;

            return false;
        }

    }
}