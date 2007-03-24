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

        public ParallelCoordinatesPlot pcPlot;
        TextLensSubComponent textLens;
        Panel panel;

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

            panel = aDestinationPanel;
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
            pcPlot.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DocumentPicked(object sender, IndexesPickedEventArgs e)
        {
            pcPlot.SetSelectedLines(e.PickedIndexes, false, false);
            pcPlot.Invalidate();
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
                pcPlot.SetSelectedHeaderIndexes(new List<int>());

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
                //pcPlot.ColorMap.Index = iDataCube.Data.GetLength(0) - 1;
                pcPlot.ColorMap.Index = 1;
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
            this.headers.Add("Median Age\n(years)");
            this.headers.Add("Albums\n(Log2)");
            this.headers.Add("Unemployment Rate\n(%)");
            this.headers.Add("GDB Per Capita\n($)");

            pcPlot = InitializeParallelCoordinatesPlot(panel, iDataCube, -1, headers);
            
            // Padding: so the names of the countries don't be cut
            pcPlot.PaddingLeft += 60;
            pcPlot.PaddingTop += 15;
            pcPlot.PickSensitivity = 3;

            pcPlot.Picked += new EventHandler<IndexesPickedEventArgs>(pcPlot_Picked);
            pcPlot.FilterChanged += new EventHandler(pcPlot_FilterChanged);

            pcPlot.LineTransparency = 235;
            pcPlot.LineTransparencySelected = 200;
            pcPlot.SelectedLineColor = Color.Black;
            pcPlot.SelectedLineThickness = 3;
            pcPlot.OutfilteredLineColor = Color.LightGray;
            pcPlot.ShowOutFilteredLines = true;

            textLens = new TextLensSubComponent(pcPlot, panel, iDoc);
            pcPlot.AddSubComponent(textLens);

            pcPlot.HeaderClicked += new EventHandler(PCPlotHeaderClicked);

            /************************************************************************/
            /* XXX: FONTS NOT WORKING                                               */
            /************************************************************************/
            pcPlot.FontSelectedHeaders = new Font("Verdana", 14, FontStyle.Bold);
            
            pcPlot.SelectedHeaderTextColor = Color.Red;
            List<int> list = new List<int>();
            list.Add(1);
            pcPlot.SetSelectedHeaderIndexes(list);


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
            pcPlot.SetSelectedHeader(pcPlot.ClickedHeader);
            iColorMap.Index = pcPlot.ClickedHeader;
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

            //pcPlot.SetSelectedLines(selectedLines, add, true);

            iDoc.SetSelectedItems(selectedLines, add, true);

            //List<int> overallSelection = pcPlot.GetSelectedLineIndexes();

            //textLens.SelectionChanged(overallSelection);
        }
    }
}
