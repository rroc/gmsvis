using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Gav.Graphics;
using MusicDataminer;
using Gav.Data;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;

namespace GMS
{
    class ParallelPlotCountries
    {
        // Private Attributes
        List<string> headers;

        // filters
        private DataCube iDataCube;
        private KMeansFilter kMeansFilter;
        private bool kMeansClusteringOn = false;

        private Renderer renderer;

        ParallelCoordinatesPlot pcPlot;
        TextLensSubComponent textLens;
        Panel panel;

        ColorMap iColorMap;

        // Lookup table to get country names
        Hashtable iCountryNames;

        public ParallelPlotCountries(DataCube aDataCube, Hashtable aCountryNames, Panel aDestinationPanel,
            Renderer aRenderer, ColorMap aColorMap)
        {
            iDataCube = aDataCube;
            iCountryNames = aCountryNames;
            iColorMap = aColorMap;

            panel = aDestinationPanel;
//            iCountryNames = new Hashtable();
            renderer = aRenderer;
            headers = new List<string>();
            kMeansFilter = new KMeansFilter(3);
            kMeansFilter.Input = iDataCube;

            SetupView();
        }

        /// <summary>
        /// Creates the PC plot given
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="filter"></param>
        /// <param name="columnIndex">the column index for which we're clustering the data
        /// in case we're using K-Means</param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private ParallelCoordinatesPlot InitializeParallelCoordinatesPlot(Panel panel,
            IDataCubeProvider<float> filter, int columnIndex, List<string> headers)
        {
            ParallelCoordinatesPlot filterPlot = new ParallelCoordinatesPlot();

            filterPlot.Input = filter;
            filterPlot.Headers = headers;

            // to color according to clusters
            if (columnIndex != -1)
            {
                iColorMap.Index = columnIndex;
            }

            filterPlot.ColorMap = iColorMap;

            filterPlot.Enabled = true;

            renderer.Add(filterPlot, panel);
            return filterPlot;
        }

      
        /// <summary>
        /// Toggles the fiter: K-Means / Regular Data
        /// </summary>
        /// <param name="aClusters">the number of clusters or -1, if regular data is to be used</param>
        public void ToggleFilter(string aClusters)
        {
            Console.WriteLine( aClusters );
            if (aClusters.Equals("None"))
            {
                ChangePC(-1);
            }
            else
            {
                this.kMeansFilter.SetNumberOfClusters(int.Parse(aClusters));
                ChangePC(kMeansFilter.GetData().Data.GetLength(0) - 1);
            }

            pcPlot.Invalidate();
            GC.Collect();
        }

        /// <summary>
        /// Changes the PC to used K-Means with the specified column for the clusters
        /// or uses the regular data
        /// </summary>
        /// <param name="columnIndex"></param>
        private void ChangePC(int columnIndex)
        {
            //COLOR according to clusters
            if (columnIndex != -1)
            {
                //if previously using normal datacube
                if( ! kMeansClusteringOn )
                {
                    pcPlot.ColorMap.Input = kMeansFilter;
                    kMeansClusteringOn = ! kMeansClusteringOn;
                }
                pcPlot.ColorMap.Index = columnIndex;
                kMeansFilter.CommitChanges();
            }
            //CLUSTERING WAS PREVIOUSLY ON
            else if (kMeansClusteringOn)
            {
                pcPlot.ColorMap.Input = iDataCube;
                kMeansClusteringOn = !kMeansClusteringOn;
                pcPlot.ColorMap.Index = iDataCube.Data.GetLength(0) - 1;
            }
            else
            {
                //None to None
            }
            pcPlot.GuideLineEnabled = true;
        }

        /// <summary>
        /// Initializes the view components: PC plot, its SubComponents and
        /// its properties
        /// </summary>
        private void SetupView()
        {
            this.headers.Add("Country");
            this.headers.Add("Median Age");
            this.headers.Add("Number of Releases");
            this.headers.Add("Unemployment Rate");
            this.headers.Add("GDB Per Capita");

            pcPlot = InitializeParallelCoordinatesPlot(panel, iDataCube, -1, headers);
            
            // Padding: so the names of the countries don't be cut
            pcPlot.PaddingLeft += 60;
            pcPlot.PickSensitivity = 3;

            pcPlot.Picked += new EventHandler<IndexesPickedEventArgs>(pcPlot_Picked);
            pcPlot.FilterChanged += new EventHandler(pcPlot_FilterChanged);

            textLens = new TextLensSubComponent(pcPlot, panel);
            pcPlot.AddSubComponent(textLens);

            //Font font = new Font("Verdana", 6);
            //Color color = Color.DodgerBlue;

            int countriesCount = iCountryNames.Count;

            // iterate through all the countries and 
            for (uint i = 0; i < countriesCount; i++)
            {
                float verticalPosition = 1.0f - (float)i / (float)(countriesCount - 1);
                string country = (string)iCountryNames[i];

                //pcPlot.AddText(country, ParallelCoordinatesPlot.TextRelativePosition.Left,
                //color, font, verticalPosition);
                textLens.AddLabel(country, verticalPosition, (int)i);
            }
        }

        void pcPlot_FilterChanged(object sender, EventArgs e)
        {
            ParallelCoordinatesPlot plot = (ParallelCoordinatesPlot)sender;

            textLens.VisibilityChanged(plot.IndexVisibilityHandler);
        }

        /// <summary>
        /// Handles the event of mouse pressing over the PC plot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pcPlot_Picked(object sender, IndexesPickedEventArgs e)
        {
            ParallelCoordinatesPlot plot = (ParallelCoordinatesPlot)sender;
            List<int> selectedLines = e.PickedIndexes;
            //Font font = new Font("Verdana", 10);
            //Color color = Color.DodgerBlue;
            //int countriesCount = countries.Count;

            //foreach (int countryId in selectedLines)
            //{
            //    string country = (string)countries[(uint)countryId];
            //    float verticalPosition = (float)countryId / (float)countriesCount;

            //plot.AddText("Very nice country", ParallelCoordinatesPlot.TextRelativePosition.Left,
            //    color, new Font("Verdana", 10), 0.3f);
            //}

            // if CTRL is pressed, add the line to the selectio
            Keys keys = Control.ModifierKeys;
            bool add = (keys == Keys.Control);

            pcPlot.SetSelectedLines(selectedLines, add, true);

            List<int> overallSelection = pcPlot.GetSelectedLineIndexes();

            textLens.SelectionChanged(overallSelection);
        }
    }
}
