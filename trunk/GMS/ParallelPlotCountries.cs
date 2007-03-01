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
        //private string[] titles;
        private object[,] data;
        List<string> headers;

        // filters
        private DataCube dataCube;
        private KMeansFilter kMeansFilter;

        private Renderer renderer;

        ParallelCoordinatesPlot pcPlot;
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

        //////////////////////////////////////////////////////////////////////////
        // Method:    CreateColorMap
        // FullName:  GMS.GMSDocument.CreateColorMap
        // Access:    private 
        // Returns:   Gav.Data.ColorMap
        //////////////////////////////////////////////////////////////////////////
        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 180.0f);
            map.AddColorMapPart(hsvMap);
            hsvMap.Invalidate();
            map.Invalidate();

            return map;
        }


        //////////////////////////////////////////////////////////////////////////
        // Method:    InitializeParallelCoordinatesPlot
        // FullName:  GMS.GMSDocument.InitializeParallelCoordinatesPlot
        // Access:    private 
        // Returns:   Gav.Graphics.ParallelCoordinatesPlot
        // Parameter: Panel panel
        // Parameter: IDataCubeProvider<float> filter
        // Parameter: int columnIndex
        //////////////////////////////////////////////////////////////////////////
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

      
        //////////////////////////////////////////////////////////////////////////
        // Method:    ToggleFilter
        // FullName:  GMS.ParallelPlotCountries.ToggleFilter
        // Access:    public 
        // Returns:   void
        //////////////////////////////////////////////////////////////////////////
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
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    ChangePC
        // FullName:  GMS.ParallelPlotCountries.ChangePC
        // Access:    private 
        // Returns:   void
        // Parameter: int columnIndex
        //////////////////////////////////////////////////////////////////////////
        private void ChangePC(int columnIndex)
        {
            // to color according to clusters
            if (columnIndex != -1)
            {
                pcPlot.Input = kMeansFilter;
                pcPlot.ColorMap.Input = kMeansFilter;
                pcPlot.ColorMap.Index = columnIndex;
            }
            else
            {
                pcPlot.Input = dataCube;
                pcPlot.ColorMap.Input = dataCube;
                /************************************************************************/
                /* TODO: COLOR ACCORDING TO COUNTRY NAMES                             */
                /************************************************************************/
                pcPlot.ColorMap.Index = dataCube.Data.GetLength(0) - 1;
            }

            pcPlot.Enabled = true;
        }


        //////////////////////////////////////////////////////////////////////////
        // Method:    SetupData
        // FullName:  GMS.ParallelPlotCountries.SetupData
        // Access:    private 
        // Returns:   void
        //////////////////////////////////////////////////////////////////////////
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


        private void SetupView()
        {
            pcPlot = InitializeParallelCoordinatesPlot(panel, dataCube, -1, headers);

            Font font = new Font("Verdana", 6);
            Color color = Color.DodgerBlue;

            int countriesCount = countries.Count;

            // iterate through all the countries and 
            for (uint i = 0; i < countriesCount; i++)
            {
                float verticalPosition = 1.0f - (float)i / (float)(countriesCount - 1);
                string country = (string)countries[i];

                pcPlot.AddText(country, ParallelCoordinatesPlot.TextRelativePosition.Left,
                color, font, verticalPosition);
            }

            // Padding: so the names of the countries don't be cut
            pcPlot.PaddingLeft += 60;

            //pcPlot.LinePicked += new EventHandler(pcPlot_LinePicked);
            pcPlot.Picked += new EventHandler<IndexesPickedEventArgs>(pcPlot_Picked);
            pcPlot.PickSensitivity = 3;
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    pcPlot_Picked
        // FullName:  GMS.GMSDocument.pcPlot_Picked
        // Access:    public 
        // Returns:   void
        // Parameter: object sender
        // Parameter: IndexesPickedEventArgs e
        //////////////////////////////////////////////////////////////////////////
        void pcPlot_Picked(object sender, IndexesPickedEventArgs e)
        {
            ParallelCoordinatesPlot plot = (ParallelCoordinatesPlot)sender;
            List<int> selectedLines = e.PickedIndexes;
            Font font = new Font("Verdana", 10);
            Color color = Color.DodgerBlue;
            int countriesCount = countries.Count;

            //foreach (int countryId in selectedLines)
            //{
            //    string country = (string)countries[(uint)countryId];
            //    float verticalPosition = (float)countryId / (float)countriesCount;

            //plot.AddText("Very nice country", ParallelCoordinatesPlot.TextRelativePosition.Left,
            //    color, new Font("Verdana", 10), 0.3f);
            //}

            pcPlot.SetSelectedLines(selectedLines, true, true);
        }
    }
}
