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
        #region Private Attributes
        List<string> headers;

        // filters
        private DataCube iDataCube;
        private KMeansFilter kMeansFilter;
        private bool kMeansClusteringOn = false;
        private ToolStripComboBox iGroupBox;

        private Renderer renderer;

        public ParallelCoordinatesPlot iPcPlot;
        TextLensSubComponent textLens;
        Panel iPanel;

        ColorMap iColorMap;

        // Lookup table to get country names
        List<string> iCountryNames;

        GMSDocument iDoc;

        #endregion // Private Attributes

        public ParallelPlotCountries(DataCube aDataCube, List<string> aCountryNames, Panel aDestinationPanel,
            Renderer aRenderer, ColorMap aColorMap, GMSDocument aDoc, ToolStripComboBox aGroupBox )
        {
            iDataCube = aDataCube;
            iCountryNames = aCountryNames;
            iColorMap = aColorMap;
            iGroupBox = aGroupBox;

            iPanel = aDestinationPanel;
            renderer = aRenderer;
            headers = new List<string>();
            iDoc = aDoc;

            InitKMeans(aDataCube);
            SetupView();

            iDoc.Picked += new EventHandler<IndexesPickedEventArgs>(DocumentPicked);
            iDoc.ColorMapChanged += new EventHandler<EventArgs>(DocumentColorMapChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DocumentColorMapChanged(object sender, EventArgs e)
        {
            iPcPlot.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DocumentPicked(object sender, IndexesPickedEventArgs e)
        {
            iPcPlot.SetSelectedLines(e.PickedIndexes, false, false);
            iPcPlot.Invalidate();
        }

        /// <summary>
        /// Initializes the KMeans filter removing the countries from it
        /// </summary>
        /// <param name="aDataCube"></param>
        private void InitKMeans(DataCube aDataCube)
        {
            kMeansFilter = new KMeansFilter(3);

            // Copy the data, removing the countries from it
            float[, ,] data = iDataCube.Data;
            float[, ,] cleanData = new float[data.GetLength(0) - 1, data.GetLength(1), 1];

            for (int i = 0; i < data.GetLength(1); i++)
            {
                for (int j = 1; j < data.GetLength(0); j++)
                {
                    cleanData[j - 1, i, 0] = data[j, i, 0];
                }
            }

            DataCube cube = new DataCube();
            cube.Data = cleanData;
            kMeansFilter.Input = cube;
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
                iDoc.iFilteredSelectedColorMap.Index = columnIndex;
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

            iPcPlot.Invalidate();
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
                iPcPlot.SetSelectedHeaderIndexes(new List<int>());

                //if previously using normal datacube
                if( ! kMeansClusteringOn )
                {
                    iPcPlot.ColorMap.Input = kMeansFilter;
                    kMeansClusteringOn = ! kMeansClusteringOn;
                }
                iPcPlot.ColorMap.Index = columnIndex;
                kMeansFilter.CommitChanges();
            }
            //CLUSTERING WAS PREVIOUSLY ON
            else if (kMeansClusteringOn)
            {
                iPcPlot.ColorMap.Input = iDataCube;
                kMeansClusteringOn = !kMeansClusteringOn;
                //pcPlot.ColorMap.Index = iDataCube.Data.GetLength(0) - 1;
                iPcPlot.ColorMap.Index = 1;
            }
            else
            {
                //None to None
            }
            iPcPlot.GuideLineEnabled = true;
        }

        /// <summary>
        /// Initializes the view components: PC plot, its SubComponents and
        /// its properties
        /// </summary>
        private void SetupView()
        {
            this.headers.Add("Country");
            this.headers.Add("Median Age\n(years)");
            this.headers.Add("Albums\n(Log2)");
            this.headers.Add("Unemployment Rate\n(%)");
            this.headers.Add("GDP Per Capita\n($)");

            iPcPlot = InitializeParallelCoordinatesPlot(iPanel, iDataCube, -1, headers);
            
            // Padding: so the names of the countries don't be cut
            iPcPlot.PaddingLeft += 60;
            iPcPlot.PaddingTop += 15;
            iPcPlot.PickSensitivity = 3;

            iPcPlot.Picked += new EventHandler<IndexesPickedEventArgs>(pcPlot_Picked);
            iPcPlot.FilterChanged += new EventHandler(pcPlot_FilterChanged);

            iPcPlot.LineTransparency = 235;
            iPcPlot.LineTransparencySelected = 200;
            iPcPlot.SelectedLineColor = Color.Black;
            iPcPlot.SelectedLineThickness = 3;
            iPcPlot.OutfilteredLineColor = Color.LightGray;
            iPcPlot.ShowOutFilteredLines = true;

            textLens = new TextLensSubComponent(iPcPlot, iPanel, iDoc);
            iPcPlot.AddSubComponent(textLens);

            iPcPlot.HeaderClicked += new EventHandler(PCPlotHeaderClicked);

            /************************************************************************/
            /* XXX: FONTS NOT WORKING                                               */
            /************************************************************************/
            iPcPlot.FontSelectedHeaders = new Font("Verdana", 14, FontStyle.Bold);
            
            iPcPlot.SelectedHeaderTextColor = Color.Red;
            List<int> list = new List<int>();
            list.Add(1);
            iPcPlot.SetSelectedHeaderIndexes(list);


            int countriesCount = iCountryNames.Count;

            // iterate through all the countries and 
            for (int i = 0; i < countriesCount; i++)
            {
                float verticalPosition = 1.0f - (float)i / (float)(countriesCount - 1);
                string country = iCountryNames[i];

                textLens.AddLabel(country, verticalPosition, (int)i);
            }

        }

        void PCPlotHeaderClicked(object sender, EventArgs e)
        {
            if (kMeansClusteringOn)
            {
                iGroupBox.SelectedIndex = 5;
                iGroupBox.Invalidate();
            }
            iPcPlot.SetSelectedHeader(iPcPlot.ClickedHeader);
            iColorMap.Index = iPcPlot.ClickedHeader;
            iDoc.iFilteredSelectedColorMap.Index = iPcPlot.ClickedHeader;
            iDoc.OnColorMapChanged();
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
            //ParallelCoordinatesPlot plot = (ParallelCoordinatesPlot)sender;
            List<int> selectedLines = e.PickedIndexes;

            // if CTRL is pressed, add the line to the selection
            Keys keys = Control.ModifierKeys;
            bool add = (keys == Keys.Control);
            iDoc.SetSelectedItems(selectedLines, add, true);
        }
    }
}
