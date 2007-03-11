using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MusicDataminer;


using Microsoft.DirectX;

using Gav.Data;
using Gav.Graphics;
using System.Collections;

namespace Treemap
{
    class TreeMap : VizComponent
    {
        TreeRectangle iRootRectangle;

        /// <summary>
        /// Constructor
        /// </summary>
        public TreeMap()
        {
            SetData();
            BuildTreeMap();
        }


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

        public void BuildStylesAreasTree(TreeRectangle aRectangle, DB db)
        {
            // Sort the styles so we can have only the N most popular ones
            ArrayList sortedStyles = new ArrayList(db.styles.Values);
            sortedStyles.Sort(new StyleComparer());

            int styleLimiter = 1;
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
                    if (countries.ContainsKey(release.country.name))
                    {
                        int i = (int)countries[release.country.name];
                        countries[release.country.name] = i + 1;
                    }
                    else
                    {
                        countries.Add(release.country.name, 1);
                    }
                }

                TreeRectangle styleRectangle = new TreeRectangle(0.0F);
                foreach (string country in countries.Keys)
                {
                    int releasesCount = (int)countries[country];
                    TreeRectangle rect = new TreeRectangle((float)releasesCount);
                    rect.SetLabel(country);
                    styleRectangle.AddRectangle(rect);
                }

                aRectangle.AddRectangle(styleRectangle);
                styleLimiter--;
            }
        }

        public void SetData()
        {
            float sum = 2549;

            int height = (int)(Math.Sqrt(sum));
            float width = sum / height;
            
            iRootRectangle = new TreeRectangle(0, 0, 0, width, height);

            //TreeRectangle grandchild1 = new TreeRectangle(0.8F);
            //TreeRectangle grandchild2 = new TreeRectangle(2.2F);

            //TreeRectangle child1 = new TreeRectangle(0.0F);
            //child1.AddRectangle(grandchild1);
            //grandchild1.SetLabel("A Very Biiiiiiiiiiiiiiiiiiiiiiiiiig Label");
            //child1.AddRectangle(grandchild2);
            //TreeRectangle child2 = new TreeRectangle(2.0F);
            //child2.SetLabel("Child2");
            //TreeRectangle child3 = new TreeRectangle(1.0F);

            //TreeRectangle parent = new TreeRectangle(0.0F);
            //parent.AddRectangle(child1);
            //parent.AddRectangle(child2);
            //parent.AddRectangle(child3);

            //iRootRectangle.AddRectangle(new TreeRectangle(6.0F));
            //iRootRectangle.AddRectangle(parent);
            //iRootRectangle.AddRectangle(new TreeRectangle(4.0F));
            //iRootRectangle.AddRectangle(new TreeRectangle(3.0F));
            //iRootRectangle.AddRectangle(new TreeRectangle(2.0F));
            //iRootRectangle.AddRectangle(new TreeRectangle(2.0F));
            //iRootRectangle.AddRectangle(new TreeRectangle(1.0F));
            string dir = System.IO.Directory.GetCurrentDirectory();
            string iDataPath = "\\..\\..\\..\\data\\";
            string iDBFileName = "db.bin";

            DB dataBase;
            MusicDBLoader.LoadDB(dir + iDataPath + iDBFileName, out dataBase);

            BuildStylesAreasTree(iRootRectangle, dataBase);
            
        }

        public void BuildTreeMap()
        {
            SquarifyTree( iRootRectangle, 0);
        }


        public void SquarifyTree( TreeRectangle aRectangle, int aLevel )
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
                    SquarifyTree(rect, 0);
                }
        }


        public void UpdateScale( int aWidth, int aHeight )
        {
            iRootRectangle.SetScale(aWidth, aHeight);
        }

        public void DrawTree( Graphics aGraphics)
        {
            iRootRectangle.Draw(aGraphics);
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
