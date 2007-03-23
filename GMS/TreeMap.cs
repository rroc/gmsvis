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
        /// 
        /// </summary>
        private TreeRectangle iRootRectangle;

        /// <summary>
        /// 
        /// </summary>
        private IColorMap iColorMap;

        private System.Windows.Forms.Panel iPanel;

        private GavToolTip iToolTip;

        private Point iMouseLocation;

        private Timer iToolTipTimer;

        private const int TIMER_DELAY = 200;

        private const int TOOLTIP_FADE_DELAY = 500;

        private Bitmap iBackBuffer;

        private Graphics iDrawingArea;

        private List<int> iSelectedIds;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TreeMap(System.Windows.Forms.Panel aPanel, GMSDocument aDoc)
        {
            iPanel = aPanel;
            iPanel.SizeChanged += new EventHandler(SizeChanged);
            iPanel.Paint += new System.Windows.Forms.PaintEventHandler(Paint);
            iToolTip = new GavToolTip(iPanel);
            
            iToolTip.FadeEnable = true;
            iToolTip.FadeTime = TOOLTIP_FADE_DELAY;
            iToolTip.Show(iMouseLocation);
            iToolTip.Hide();

            iToolTipTimer = new Timer();
            iToolTipTimer.Interval = TIMER_DELAY;
            iToolTipTimer.Tick += new EventHandler(ToolTipTimerTick);

            iPanel.MouseMove    += new System.Windows.Forms.MouseEventHandler(MouseMove);
            iPanel.MouseLeave   += new EventHandler(MouseLeave);

            aDoc.ColorMapChanged    += new EventHandler<EventArgs>(DocumentColorMapChanged);
            aDoc.Picked             += new EventHandler<IndexesPickedEventArgs>(DocumentPicked);

            iSelectedIds = new List<int>();

            UpdateDrawingArea();
        }

        /// <summary>
        /// Handles the event of Picking an object 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DocumentPicked(object sender, IndexesPickedEventArgs e)
        {
            iSelectedIds = e.PickedIndexes;
            //Graphics g = iPanel.CreateGraphics();
            //DrawTree(g);
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
        void MouseLeave(object sender, EventArgs e)
        {
            iToolTipTimer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToolTipTimerTick(object sender, EventArgs e)
        {
            iToolTipTimer.Stop();
            iToolTip.Hide();
            object label = OnHoverRectangle(iMouseLocation);
            iToolTip.Text = (string)label;

            /************************************************************************/
            /* XXX: HACK :P - BAR                                                   */
            /************************************************************************/
            Point mousePos = iMouseLocation;
            //mousePos.X += iPanel.Location.X + iToolTip.Size.Width;
            //mousePos.Y += 21;
            
            iToolTip.FadeEnable = false;
            iToolTip.Show(iPanel.PointToScreen(mousePos));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            iToolTipTimer.Stop();

            // XXX: The ToolTip Show() method is invalidating the form which
            // causes the MouseMove event to be called repeatedly.
            // The > 1 condition avoids the tooltip being stuck
            // when the mouse leaves the panel
            if (iMouseLocation != e.Location && e.Location.X > 1)
            {
                iToolTip.Hide();
                iToolTipTimer.Start();
            }
            
            iMouseLocation = e.Location;
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
        /// 
        /// </summary>
        /// <param name="aDataCube"></param>
        /// <param name="aQuantitativeDataIndex"></param>
        /// <param name="aOrdinalDataIndex"></param>
        /// <param name="aIdIndex"></param>
        private void InitTreeMapFromDataCube(object[, ,] aDataCube, int aQuantitativeDataIndex,
            int aOrdinalDataIndex, int aIdIndex, int aLeafNodeLabelIndex, 
            List<GMSToolTipComponent> aToolTipComponents)
        {
            iRootRectangle = new TreeRectangle(0.0F);

            string currentGroup = (string)aDataCube[0, aOrdinalDataIndex, 0];
            TreeRectangle currentNode = new TreeRectangle(0.0F);
            currentNode.Label = currentGroup;

            // iterate through the rows
            for (int i = 0; i < aDataCube.GetLength(0); i++)
            {
                // if changing to a different group
                if (currentGroup != (string)aDataCube[i, aOrdinalDataIndex, 0])
                {
                    iRootRectangle.AddRectangle(currentNode);
                    currentNode = new TreeRectangle(0.0F);
                    currentGroup = (string)aDataCube[i, aOrdinalDataIndex, 0];
                    currentNode.Label = currentGroup;
                }

                float area = Convert.ToSingle(aDataCube[i, aQuantitativeDataIndex, 0]);

                // only add the node if the area is bigger than one
                if (area > 1.0F)
                {
                    TreeRectangle childRectangle = new TreeRectangle(area);
                    childRectangle.Id = Convert.ToInt32(aDataCube[i, aIdIndex, 0]);

                    // Tooltip Data and Label
                    BuildToolTipData(childRectangle, aDataCube, aToolTipComponents, i);
                    childRectangle.Label = (string)aDataCube[i, aLeafNodeLabelIndex, 0];

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
                    + aDataCube[aRow, toolTipComponent.iColumnIndex, 0]
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
            TreeRectangle rectangle = iRootRectangle.LocationInsideRectangle(location);
            
            if (rectangle != null)
            {
                return rectangle.Data;
            }

            return null;
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

            //Calculate data size according to the sreen
            float sum = iRootRectangle.GetArea();
            float ratio = iPanel.Width / (float)iPanel.Height;
            float height = (float)Math.Sqrt(sum / ratio);
            float width = height * ratio;

            iRootRectangle.SetSize(0, 0, width, height);

            BuildTreeMap();
        }

        /// <summary>
        /// Update the data according to the available screenspace
        /// </summary>
        public void UpdateData()
        {
            float sum = iRootRectangle.GetArea();

            float ratio = iPanel.Width / (float)iPanel.Height;
            float height = (float)Math.Sqrt(sum / ratio);
            float width = height * ratio;

            iRootRectangle.SetSize(0, 0, width, height);
            BuildTreeMap();
        }


        /// <summary>
        /// Executes the treemap calculation
        /// </summary>
        public void BuildTreeMap()
        {
            SquarifyTree(iRootRectangle);
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
            iRootRectangle.SetScale(iPanel.Width, iPanel.Height);
        }

        /// <summary>
        /// Draw the whole tree
        /// </summary>
        public void DrawTree(Graphics g)
        {
            if (iRootRectangle == null)
            {
                return;
            }

            iRootRectangle.GetChildren()[0].Draw(iDrawingArea, iColorMap);
            //iRootRectangle.Draw(iDrawingArea, iColorMap);
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
