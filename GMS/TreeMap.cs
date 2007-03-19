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
        //VARIABLES

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


        /************************************************************************/
        /*                                                                      */
        /************************************************************************/

        class StyleComparer : IComparer
        {
            // Comparator class for countries
            int IComparer.Compare(object x, object y)
            {
                Style c1 = (Style)x;
                Style c2 = (Style)y;

                return c2.releases.Count.CompareTo(c1.releases.Count);
            }
        };

        class DictionaryValueComparer : IComparer
        {
            // Comparator class for hashtable entries
            // with integer as its value
            int IComparer.Compare(object x, object y)
            {
                DictionaryEntry c1 = (DictionaryEntry)x;
                DictionaryEntry c2 = (DictionaryEntry)y;

                return (int)((int)c2.Value - (int)c1.Value);
            }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aRectangle"></param>
        /// <param name="db"></param>
        public void BuildStylesAreasTree(TreeRectangle aRectangle, DB db, Hashtable aFilter)
        {
            ArrayList filter = new ArrayList(aFilter.Values);
            // Sort the styles so we can have only the N most popular ones
            ArrayList sortedStyles = new ArrayList(db.styles.Values);
            sortedStyles.Sort(new StyleComparer());

            int styleLimiter = 10;
            foreach (Style style in sortedStyles)
            {
                if (styleLimiter == 0)
                {
                    break;
                }

                Hashtable countries = new Hashtable();

                // count all the occurrences in the countries
                foreach (MusicBrainzRelease release in style.releases)
                {
                    //according to the filter used on pcplot
                    if (filter.Contains(release.country.name) )
                    {
                        if (countries.ContainsKey(release.country.name))
                        {
                            int oldvalue = (int)countries[release.country.name];
                            countries[release.country.name] = oldvalue + 1;
                        }
                        else
                        {
                            countries.Add(release.country.name, 1);
                        }
                    }
                }

                /************************************************************************/
                /* QUICK FIX !!!                                                        */
                /************************************************************************/
                ArrayList sortedReleases = new ArrayList(countries);
                sortedReleases.Sort(new DictionaryValueComparer());

                TreeRectangle styleRectangle = new TreeRectangle(0.0F);
                
                //Style name
                styleRectangle.Label = style.name;
                
                foreach (DictionaryEntry entry in sortedReleases)
                {
                    string country = (string)entry.Key;
                    int releasesCount = (int)countries[country];
                    TreeRectangle rect = new TreeRectangle((float)releasesCount);
                    rect.Label = country;
                    rect.id = filter.IndexOf(country);
                    styleRectangle.AddRectangle(rect);
                }

                aRectangle.AddRectangle(styleRectangle);
                styleLimiter--;
            }
        }

        /************************************************************************/
        /*                                                                      */
        /************************************************************************/

        /// <summary>
        /// Constructor
        /// </summary>
        public TreeMap(DB aDatabase, Hashtable aFilter, System.Windows.Forms.Panel aPanel ) //int aPanelWidth, int aPanelHeight)
        {
            iPanel = aPanel;
            iPanel.SizeChanged += new EventHandler(iPanel_SizeChanged);
            iPanel.Paint += new System.Windows.Forms.PaintEventHandler(iPanel_Paint);
            iToolTip = new GavToolTip(iPanel);
            SetData( aDatabase, aFilter );
            BuildTreeMap();
            iToolTip.FadeEnable = true;
            iToolTip.FadeTime = TOOLTIP_FADE_DELAY;
            iToolTip.Show(iMouseLocation);
            iToolTip.Hide();

            iToolTipTimer = new Timer();
            iToolTipTimer.Interval = TIMER_DELAY;
            iToolTipTimer.Tick += new EventHandler(iToolTipTimer_Tick);

            iPanel.MouseHover   += new EventHandler(iPanel_MouseHover);
            iPanel.MouseMove    += new System.Windows.Forms.MouseEventHandler(iPanel_MouseMove);
            iPanel.MouseLeave   += new EventHandler(iPanel_MouseLeave);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void iPanel_MouseLeave(object sender, EventArgs e)
        {
            iToolTip.Hide();
            iToolTipTimer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void iToolTipTimer_Tick(object sender, EventArgs e)
        {
            iToolTip.Hide();
            object label = OnHoverRectangle(iMouseLocation);
            //iToolTip.Text = "I'm over the (" + iMouseLocation.X + ", " + iMouseLocation.Y + ") position";
            iToolTip.Text = (string)label;

            /************************************************************************/
            /* XXX: HACK :P                                                        */
            /************************************************************************/
            Point mousePos = iMouseLocation;
            mousePos.X += iPanel.Location.X + iToolTip.Size.Width;
            mousePos.Y += 46;

            iToolTip.Show(mousePos);
            iToolTipTimer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void iPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            iToolTipTimer.Stop();

            // XXX: The ToolTip Show() method is invalidating the form which
            // causes the MouseMove event to be called repeatedly
            if (iMouseLocation != e.Location)
            {
                iToolTipTimer.Start();
            }
            
            iMouseLocation = e.Location;
        }

        void iPanel_MouseHover(object sender, EventArgs e)
        {
            //iToolTip.Hide();
            //iToolTip.Text = "I'm over the (" + iMouseLocation.X + ", " + iMouseLocation.Y + ") position";
            //iToolTip.Show(iMouseLocation);
            //Console.WriteLine("I'm over the TreeMap");
        }

        void iPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTree();
        }


        void iPanel_SizeChanged(object sender, EventArgs e)
        {
            UpdateData();
            UpdateScale();
            iPanel.Invalidate();
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
 
        private object OnHoverRectangle(Point location)
        {
            TreeRectangle rectangle = iRootRectangle.LocationInsideRectangle(location);
            
            if (rectangle != null)
            {
                return rectangle.Label;
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aPanelWidth"></param>
        /// <param name="aPanelHeight"></param>
        public void SetData(DB aDatabase, Hashtable aFilter)
        {
            iRootRectangle = new TreeRectangle(0.0f);
            BuildStylesAreasTree(iRootRectangle, aDatabase, aFilter);


            //Calculate data size according to the sreen
            float sum = iRootRectangle.GetArea();
            float ratio = iPanel.Width / (float)iPanel.Height;
            float height = (float)Math.Sqrt(sum / ratio);
            float width = height * ratio;

            iRootRectangle.SetSize(0, 0, width, height);
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
        public void DrawTree()
        {
            Graphics g = iPanel.CreateGraphics();
            iRootRectangle.Draw(g, iColorMap);
            g.Dispose();
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
}
