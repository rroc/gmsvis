using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MusicDataminer;
using Gav.Graphics;
using Gav.Data;
using System.Windows.Forms;

namespace GMS
{
    class GMSDocument
    {
        // private attributes
        private string[] titles;
        private object[,] data;
        List<string> stringList;

        private DataCube dataCube;
        private Renderer renderer;

        Panel panel;

        DB db;

        public GMSDocument(Panel aPanel, Form form)
        {
            this.panel = aPanel;
            this.renderer = new Renderer(form);
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
            headers.Add("Median Age");
            headers.Add("Number of Releases");
            headers.Add("Unemployment Rate");
            headers.Add("GDB Per Capita");

            List<object[]> filteredCountries = new List<object[]>();
            
            int i = 0;
            foreach (Country country in db.countries.Values)
            {
                // if any albums were release in that country
                if (country.releases.Count != 0)
                {
                    filteredCountries.Add(new object[4]{country.medianAge, 
                        country.releases.Count, 
                        country.unemploymentRate, 
                        country.gdbPerCapita});

                    //data[0, i] = country.medianAge;
                    //data[1, i] = country.releases.Count;
                    //data[2, i] = country.unemploymentRate;
                    //data[3, i] = country.gdbPerCapita;
                }
            }

            data = new object[4, filteredCountries.Count];

            foreach (object[] obj in filteredCountries)
            {
                data[0, i] = obj[0];
                data[1, i] = obj[1];
                data[2, i] = obj[2];
                data[3, i] = obj[3];
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
            InitializeParallelCoordinatesPlot(panel, dataCube, -1, headers);

            //List<float> max, min;
            //dataCube.GetData().GetAllColumnMaxMin(out max, out min);
            //int columns = dataCube.GetData().Data.GetLength(0);
            //for (int i = 0; i < columns; i++)
            //{
            //    plot.SetMin(i, min[i]);
            //    plot.SetMax(i, max[i]);
            //}


        }


    }
}
