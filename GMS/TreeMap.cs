using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MusicDataminer;


using Microsoft.DirectX;

using Gav.Data;
using Gav.Graphics;
using System.Collections;
using System.Windows.Forms;

namespace GMS
{
    class TreeMap : VizComponent
    {
        #region Private Variables

        /// <summary>
        /// The original root of the TreeMap
        /// </summary>
        private TreeRectangle iRootRectangle;

        /// <summary>
        /// The current node rectangle
        /// </summary>
        private TreeRectangle iCurrentRootRectangle;

        /// <summary>
        /// 
        /// </summary>
        private IColorMap iColorMap;

        private System.Windows.Forms.Panel iPanel;

        private GavToolTip iToolTip;

        private const int TIMER_DELAY = 200;

        private const int TOOLTIP_FADE_DELAY = 200;

        private Bitmap iBackBuffer;

        private Graphics iDrawingArea;

        private List<int> iSelectedIds;

        private MouseHoverController iMouseHoverControl;

        private bool iZoomIn;

        private GMSDocument iDoc;

        private ParallelCoordinatesPlot iPcPlot;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TreeMap(System.Windows.Forms.Panel aPanel, GMSDocument aDoc, ParallelCoordinatesPlot aPcPlot )
        {
            iPanel = aPanel;
            iPanel.SizeChanged += new EventHandler(SizeChanged);
            iPanel.Paint += new System.Windows.Forms.PaintEventHandler(Paint);
            iToolTip = new GavToolTip(iPanel);

            iPcPlot = aPcPlot;

            iToolTip.FadeEnable = true;
            iToolTip.FadeTime = TOOLTIP_FADE_DELAY;
            iToolTip.Show(new Point(0,0));
            iToolTip.Hide();

            iPanel.MouseLeave           += new EventHandler(MouseHoverControlHoverEnd);
            iMouseHoverControl          = new MouseHoverController(iPanel, 5, TIMER_DELAY);
            iMouseHoverControl.Hover    += new EventHandler(MouseHoverControlHover);
            iMouseHoverControl.HoverEnd += new EventHandler(MouseHoverControlHoverEnd);

            iDoc = aDoc;

            aDoc.ColorMapChanged += new EventHandler<EventArgs>(DocumentColorMapChanged);
            iDoc.Picked += new EventHandler<IndexesPickedEventArgs>(DocumentPicked);

            iPanel.MouseDown += new MouseEventHandler(MouseDown);
            iSelectedIds = new List<int>();

            
            iZoomIn = false;

            UpdateDrawingArea();
        }

        void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IncreaseLevel(e.Location);
            }
            else
            {
                DecreaseLevel();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MouseHoverControlHoverEnd(object sender, EventArgs e)
        {
            iToolTip.Hide();
            Invalidate();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MouseHoverControlHover(object sender, EventArgs e)
        {
            object label = OnHoverRectangle(iMouseHoverControl.HoverPosition);
            iToolTip.Text = (string)label;

            iToolTip.Hide();
            iToolTip.Show(iPanel.PointToScreen(iMouseHoverControl.HoverPosition));
        }

        /// <summary>
        /// Handles the event of Picking an object 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DocumentPicked(object sender, IndexesPickedEventArgs e)
        {
            iSelectedIds = e.PickedIndexes;
            Graphics g = iPanel.CreateGraphics();
            DrawTree(g);
        }

        /// <summary>
        /// Updates the Backbuffer drawing area variables
        /// </summary>
        private void UpdateDrawingArea()
        {
            if (iBackBuffer != null && iDrawingArea != null)
            {
                iBackBuffer.Dispose();
                iDrawingArea.Dispose();
            }

            iBackBuffer = new Bitmap(iPanel.ClientSize.Width, iPanel.ClientSize.Height);
            iDrawingArea = Graphics.FromImage(iBackBuffer);
        }

        void DocumentColorMapChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTree(e.Graphics);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SizeChanged(object sender, EventArgs e)
        {
            // if minimized, don't update
            if (this.iPanel.ClientSize.IsEmpty && ! this.iPanel.Disposing)
            {
                return;
            }

            UpdateData();
            UpdateScale();
            UpdateDrawingArea();
            
            this.Invalidate();
        }

        /// <summary>
        /// Sets/Gets the Color Map associated with last level
        /// </summary>
        public IColorMap ColorMap
        {
            get
            {
                return this.iColorMap;
            }
            set
            {
                this.iColorMap = value;
            }
        }

        /// <summary>
        /// Initializes the TreeMap from a descending order object[,,] cube
        /// </summary>
        /// <param name="aDataCube">[columns, rows, 1]</param>
        /// <param name="aQuantitativeDataIndex"></param>
        /// <param name="aOrdinalDataIndex"></param>
        /// <param name="aIdIndex"></param>
        private void InitTreeMapFromDataCube(object[, ,] aDataCube, int aQuantitativeDataIndex,
            int aOrdinalDataIndex, int aIdIndex, int aLeafNodeLabelIndex, 
            List<GMSToolTipComponent> aToolTipComponents)
        {
            iRootRectangle  = new TreeRectangle(0.0F);
            iCurrentRootRectangle    = iRootRectangle;

            string currentGroup = (string)aDataCube[aOrdinalDataIndex, 0, 0];
            TreeRectangle currentNode = new TreeRectangle(0.0F);
            currentNode.Label = currentGroup;

            // iterate through the rows
            for (int i = 0; i < aDataCube.GetLength(1); i++)
            {
                // if changing to a different group
                if (currentGroup != (string)aDataCube[aOrdinalDataIndex, i, 0])
                {
                    // only add the node to the root if any children have been created
                    if (currentNode.GetChildren().Count != 0)
                    {
                        iRootRectangle.AddRectangle(currentNode);
                    }
                    currentNode = new TreeRectangle(0.0F);
                    currentGroup = (string)aDataCube[aOrdinalDataIndex, i, 0];
                    currentNode.Label = currentGroup;
                }

                float area = Convert.ToSingle(aDataCube[aQuantitativeDataIndex, i, 0]);

                // only add the node if the area is bigger or equal to one
                if (area >= 1.0F)
                {
                    TreeRectangle childRectangle = new TreeRectangle(area);
                    childRectangle.Id = Convert.ToInt32(aDataCube[aIdIndex, i, 0]);

                    // Tooltip Data and Label
                    BuildToolTipData(childRectangle, aDataCube, aToolTipComponents, i);
                    childRectangle.Label = (string)aDataCube[aLeafNodeLabelIndex, i, 0];

                    currentNode.AddRectangle(childRectangle);
                }
            }

            iRootRectangle.AddRectangle(currentNode);
        }

        /// <summary>
        /// Creates the Tooltip Data
        /// </summary>
        /// <param name="aRectangle"></param>
        /// <param name="aDataCube"></param>
        /// <param name="aToolTipComponents"></param>
        private void BuildToolTipData(TreeRectangle aRectangle, object[, ,] aDataCube,
            List<GMSToolTipComponent> aToolTipComponents, int aRow)
        {
            string toolTip = "";
            foreach (GMSToolTipComponent toolTipComponent in aToolTipComponents)
            {
                toolTip += toolTipComponent.iPrefix + ": "
                    + aDataCube[toolTipComponent.iColumnIndex, aRow, 0]
                    + " " + toolTipComponent.iSuffix
                    + "\n";
            }

            aRectangle.Data = toolTip;
        }
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private object OnHoverRectangle(Point location)
        {
            TreeRectangle rectangle = iCurrentRootRectangle.LocationInsideLeafRectangle(location);
            
            if (rectangle != null)
            {
                return rectangle.Data;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        public void IncreaseLevel(Point location)
        {
            // If already zoomed, quit
            if (iZoomIn)
            {
                return;
            }

            TreeRectangle selectedRectangle = null;

            foreach (TreeRectangle rectangle in iCurrentRootRectangle.GetChildren())
            {
                selectedRectangle = rectangle.LocationInsideRectangle(location);
                if (selectedRectangle != null)
                {
                    break;
                }
            }

            if (selectedRectangle != null)
            {
                this.iCurrentRootRectangle = selectedRectangle;
            }

            /************************************************************************/
            /* THE SAME AS IN SIZECHANGED!!! CREATE INVALIDATEDATA / INVALIDATEVIEW */
            /************************************************************************/
            UpdateData();
            UpdateScale();
            UpdateDrawingArea();

            this.Invalidate();

            iZoomIn = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DecreaseLevel()
        {
            if (iZoomIn)
            {
                iZoomIn = false;

                // switch to the original root
                iCurrentRootRectangle = iRootRectangle;
                
                UpdateData();
                UpdateScale();
                UpdateDrawingArea();

                this.Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aPanelWidth"></param>
        /// <param name="aPanelHeight"></param>
        public void SetData(object[, ,] aDataCube, int aQuantitativeDataIndex,
            int aOrdinalDataIndex, int aIdIndex, int aLeafNodeLabelIndex, 
            List<GMSToolTipComponent> aToolTipComponents)
        {
            InitTreeMapFromDataCube(aDataCube, aQuantitativeDataIndex,
                aOrdinalDataIndex, aIdIndex, aLeafNodeLabelIndex, aToolTipComponents);

            // Calculates the TreeMap Visual Appearance
            UpdateData();
        }

        /// <summary>
        /// Update the data according to the available screenspace
        /// </summary>
        public void UpdateData()
        {
            float sum = iCurrentRootRectangle.GetArea();

            float ratio = iPanel.Width / (float)iPanel.Height;
            float height = (float)Math.Sqrt(sum / ratio);
            float width = height * ratio;

            iCurrentRootRectangle.SetSize(0, 0, width, height);
            BuildTreeMap();
        }


        /// <summary>
        /// Executes the treemap calculation
        /// </summary>
        public void BuildTreeMap()
        {
            SquarifyTree(iCurrentRootRectangle);
        }

        /// <summary>
        /// Parse the whole tree squerifying all the nodes
        /// </summary>
        /// <param name="aRectangle">current rectangle</param>
        public void SquarifyTree(TreeRectangle aRectangle)
        {
            if ( 0 == aRectangle.GetChildren().Count) 
            {
                return;
            }

            List<TreeRectangle> row = new List<TreeRectangle>();
            List<TreeRectangle> copyOfChildren = new List<TreeRectangle>(aRectangle.GetChildren());
            SquarifyLevel(copyOfChildren, row, aRectangle);

            foreach (TreeRectangle rect in aRectangle.GetChildren())
                {
                    SquarifyTree(rect);
                }
        }

        /// <summary>
        /// Update the scale for the new screensize
        /// </summary>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public void UpdateScale()
        {
            iCurrentRootRectangle.SetScale(iPanel.Width, iPanel.Height);
        }

        /// <summary>
        /// Draw the whole tree
        /// </summary>
        public void DrawTree(Graphics g)
        {
            if (iCurrentRootRectangle == null)
            {
                return;
            }


            //iCurrentRootRectangle.GetChildren()[0].Draw(iDrawingArea, iColorMap);

            iCurrentRootRectangle.Draw(iDrawingArea, iColorMap, iSelectedIds, iPcPlot.IndexVisibilityHandler );
            g.DrawImageUnscaled(iBackBuffer, 0, 0);
        }

        /// <summary>
        /// Split the input values to the rectangle
        /// </summary>
        /// <param name="aChildren">input values(the tree) </param>
        /// <param name="aRow">The current work row</param>
        /// <param name="aWidth">The length of the current rectangle's max side</param>
        public void SquarifyLevel(List<TreeRectangle> aChildren, List<TreeRectangle> aRow, TreeRectangle aRectangle)
        {

            if( 0 == aChildren.Count )
            {
                aRectangle.LayOutRow(aRow);
                return;
            }

            TreeRectangle head = aChildren[0];

            if (ContinueRow(aRow, head, aRectangle))
            {
                aRow.Add( head );
                aChildren.RemoveAt(0);
                SquarifyLevel( aChildren, aRow, aRectangle );
            }
            else
            {
                aRectangle.LayOutRow(aRow);
                SquarifyLevel(aChildren, new List<TreeRectangle>(), aRectangle );
            }

        }

        /// <summary>
        /// Checks whether the row can be continued, or is it better to start a new one.
        /// </summary>
        /// <param name="aRow">List of values</param>
        /// <param name="aHead">The proposed new row member</param>
        /// <param name="aWidth">Width of the current cell</param>
        /// <returns>True if the row should be continued</returns>
        private bool ContinueRow(List<TreeRectangle> aRow, TreeRectangle aChild, TreeRectangle aParent )
        {
            float width = aParent.MinSideLength();
            float area = aChild.GetArea();

            int count = aRow.Count;
            if( count == 0)
            {
                return true;
            }
            float widthSqr = width * width;
            
            //First ratio
            float sum = Sum( aRow );
            float sumSqr = sum * sum;
            float rMax = aRow[0].GetArea();
            float rMin = aRow[count - 1].GetArea();
            float max = (widthSqr * rMax)/sumSqr;
            float min = sumSqr/(widthSqr*rMin);
            float firstMax = Math.Max( max, min);

            //Second ratio
            sum += area;
            sumSqr = sum * sum;
            rMax = Math.Max( rMax, area );
            rMin = Math.Min( rMin, area);
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
        private float Sum(List<TreeRectangle> aRow)
        {
            float sum = 0;
            foreach (TreeRectangle rectangle in aRow)
            {
                sum += rectangle.GetArea();
            }
            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        public new void Invalidate()
        {
            Graphics g = iPanel.CreateGraphics();
            DrawTree(g);
            g.Dispose();
        }

        public void Dispose()
        {
            iDoc.ColorMapChanged    -= new EventHandler<EventArgs>(DocumentColorMapChanged);
            iDoc.Picked             -= new EventHandler<IndexesPickedEventArgs>(DocumentPicked);
        }

        protected override void InternalMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            //iToolTip.Hide();
            //iToolTip.Text = "I'm over the (" + e.X + ", " + e.Y + ") position";
            //iToolTip.Show(e.Location);
        }

        protected override void InternalMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
        }

        protected override void InternalInit(Microsoft.DirectX.Direct3D.Device device)
        {
            // TODO: something maybe :)
        }

        protected override void InternalInvalidate()
        {
            //DrawTree();
        }

        protected override void InternalMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        protected override void InternalRender(Microsoft.DirectX.Direct3D.Device device)
        {
            //DrawTree();
        }

        protected override void InternalUpdateSize()
        {
            //UpdateData();
            //UpdateScale();
            //iPanel.Invalidate();
        }
    }


    // HELPER CLASSES
    public class GMSToolTipComponent
    {
        public string iPrefix;
        public int iColumnIndex;
        public string iSuffix;

        public GMSToolTipComponent(string aPrefix, int aColumnIndex, string aSuffix)
        {
            iPrefix         = aPrefix;
            iColumnIndex    = aColumnIndex;
            iSuffix         = aSuffix;
        }

        public GMSToolTipComponent(string aPrefix, int aColumnIndex)
            : this(aPrefix, aColumnIndex, "")
        {

        }
    }
}
