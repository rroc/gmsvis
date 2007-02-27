using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MusicDataminer;
using Gav.Graphics;
using Gav.Data;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;

namespace GMS
{

    class CountryComparer : IComparer
    {
        // Comparator class for countries
        int IComparer.Compare(object x, object y)
        {
            Country c1 = (Country)x;
            Country c2 = (Country)y;

            return c1.name.CompareTo(c2.name);
        }
    };

    class GMSDocument
    {
        // private attributes
        private string[] titles;
        private object[,] data;
        List<string> stringList;

        Hashtable countries;

        private DataCube dataCube;
        private Renderer renderer;

        Panel panel;
        ParallelCoordinatesPlot pcPlot;

        DB db;

        public GMSDocument(Panel aPanel, Form form)
        {
            this.panel = aPanel;
            this.renderer = new Renderer(form);
            countries = new Hashtable();
        }


        public void ReadDB(string filename)
        {
            bool loaded = MusicDBLoader.LoadDB(filename, out db);

            if( ! loaded )
            {
                MessageBox.Show("Database not loaded :(");
            }
        }

        public void FillDummieCube(List<string> headers)
        {
            headers.Add("Country");
            headers.Add("Median Age");
            headers.Add("Number of Releases");
            headers.Add("Unemployment Rate");
            headers.Add("GDB Per Capita");

            List<object[]> filteredCountries = new List<object[]>();
            ArrayList sortedCountries = new ArrayList(db.countries.Values);
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
                        country.releases.Count, 
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

            if (columnIndex != -1)
            {
                colorMap.Index = columnIndex;
            }

            filterPlot.ColorMap = colorMap;

            filterPlot.Enabled = true;

            renderer.Add(filterPlot, panel);
            return filterPlot;
        }

        public void ShowData(List<string> headers)
        {
            // Write your code here.
            pcPlot = InitializeParallelCoordinatesPlot(panel, dataCube, -1, headers);
            string country0 = (string)countries[(uint)0];
            string country1 = (string)countries[(uint)10];
            
            Font font = new Font("Verdana", 6);
            Color color = Color.DodgerBlue;

            int countriesCount = countries.Count;
            uint stride = (uint)countriesCount / 10;

            for (uint i = 0; i < countriesCount; i += 1)
            {
                float verticalPosition = 1.0f - (float)i / (float)countriesCount;
                string country = (string)countries[i];

                pcPlot.AddText(country, ParallelCoordinatesPlot.TextRelativePosition.Left,
                color, font, verticalPosition);
            }

            pcPlot.PaddingLeft += 60;

            pcPlot.LinePicked += new EventHandler(pcPlot_LinePicked);
            pcPlot.Picked += new EventHandler<IndexesPickedEventArgs>(pcPlot_Picked);
            pcPlot.PickSensitivity = 3;
        }

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

        void pcPlot_LinePicked(object sender, EventArgs e)
        {
            List<int> selectedLines =((ParallelCoordinatesPlot)sender).GetSelectedLineIndexes();
            Font font = new Font("Verdana", 10);
            Color color = Color.DarkKhaki;
            int countriesCount = countries.Count;

            foreach (int countryId in selectedLines)
            {
                string country = (string)countries[(uint)countryId];
                float verticalPosition = countryId / countriesCount;
                
                pcPlot.AddText(country, ParallelCoordinatesPlot.TextRelativePosition.Left,
                color, font, verticalPosition);
            }

        }


    }
}
