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
        private object[,] data;
        List<string> headers;

        // filters
        private DataCube dataCube;
        private KMeansFilter kMeansFilter;
        private bool kMeansClusteringOn = false;

        private Renderer renderer;

        ParallelCoordinatesPlot pcPlot;
        TextLensSubComponent textLens;
        Panel panel;

        DB database;

        // Lookup table to get country names
        Hashtable countries;

        public ParallelPlotCountries(DB aDatabase, Panel aDestinationPanel, 
            Renderer aRenderer)
        {
            database = aDatabase;
            panel = aDestinationPanel;
            countries = new Hashtable();
            renderer = aRenderer;
            headers = new List<string>();
            kMeansFilter = new KMeansFilter(3);

            SetupData();
            SetupView();
        }

        /// <summary>
        /// Creates a HSV(0.0, 180.0) Color Map
        /// </summary>
        /// <returns></returns>
        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 180.0f);
            map.AddColorMapPart(hsvMap);
            hsvMap.Invalidate();
            map.Invalidate();

            return map;
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

            ColorMap colorMap = CreateColorMap();
            colorMap.Input = filter;

            // to color according to clusters
            if (columnIndex != -1)
            {
                colorMap.Index = columnIndex;
            }

            filterPlot.ColorMap = colorMap;

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
            // to color according to clusters
            if (columnIndex != -1)
            {
                if( ! kMeansClusteringOn )
                {
                    pcPlot.Input = kMeansFilter;
                    pcPlot.ColorMap.Input = kMeansFilter;
                    kMeansClusteringOn = ! kMeansClusteringOn;
                }
                
                pcPlot.ColorMap.Index = columnIndex;
                kMeansFilter.CommitChanges();
            }
            else if (kMeansClusteringOn)
            {
                pcPlot.Input = dataCube;
                pcPlot.ColorMap.Input = dataCube;
                kMeansClusteringOn = !kMeansClusteringOn;
                pcPlot.ColorMap.Index = dataCube.Data.GetLength(0) - 1;
            }
            pcPlot.GuideLineEnabled = true;
        }


        /// <summary>
        /// Sorts the data by Country name and adds the data to the K-Means filter
        /// and the regular Data Cube
        /// </summary>
        private void SetupData()
        {
            this.headers.Add("Country");
            this.headers.Add("Median Age");
            this.headers.Add("Number of Releases");
            this.headers.Add("Unemployment Rate");
            this.headers.Add("GDB Per Capita");

            List<object[]> filteredCountries = new List<object[]>();
            ArrayList sortedCountries = new ArrayList( database.countries.Values );
            sortedCountries.Sort(new CountryComparer());

            //Dynamically allocate memory for the data
            uint i = 0;
            foreach (Country country in sortedCountries)
            {
                // if any albums were release in that country
                if (country.releases.Count != 0)
                {
                    filteredCountries.Add(new object[5]{
                        i, 
                        country.medianAge, 
                        Math.Log(country.releases.Count, 2), 
                        //country.releases.Count, 
                        country.unemploymentRate, 
                        country.gdbPerCapita});

                    string countryTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country.name);

                    countries.Add(i++, countryTitleCase);
                }
            }

            //Allocate memory for the datacube data
            data = new object[5, filteredCountries.Count];
            i = 0;
            foreach (object[] obj in filteredCountries)
            {
                // copy every attribute
                for (int j = 0; j < 5; j++)
                {
                    data[j, i] = obj[j];
                }
                ++i;
            }

            dataCube = new DataCube();
            dataCube.SetData(data);
            kMeansFilter.Input = dataCube;
        }

        /// <summary>
        /// Initializes the view components: PC plot, its SubComponents and
        /// its properties
        /// </summary>
        private void SetupView()
        {
            pcPlot = InitializeParallelCoordinatesPlot(panel, dataCube, -1, headers);
            
            // Padding: so the names of the countries don't be cut
            pcPlot.PaddingLeft += 60;
            pcPlot.PickSensitivity = 3;

            pcPlot.Picked += new EventHandler<IndexesPickedEventArgs>(pcPlot_Picked);
            pcPlot.FilterChanged += new EventHandler(pcPlot_FilterChanged);

            textLens = new TextLensSubComponent(pcPlot, panel);
            pcPlot.AddSubComponent(textLens);

            //Font font = new Font("Verdana", 6);
            //Color color = Color.DodgerBlue;

            int countriesCount = countries.Count;

            // iterate through all the countries and 
            for (uint i = 0; i < countriesCount; i++)
            {
                float verticalPosition = 1.0f - (float)i / (float)(countriesCount - 1);
                string country = (string)countries[i];

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
