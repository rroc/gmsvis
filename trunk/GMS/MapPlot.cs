using System;
using System.IO;
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
    class MapPlot
    {
        private const string iGeoDataPath = "../../../data/geodata/";

        private object[,] data;

        List<object> regions;


        private DataCube dataCube;
        private MapData mapData;
        private ColorMap colorMap;

        private Renderer renderer;

        // Layers
        MapPolygonLayer polygonLayer;
        MapPolygonBorderLayer borderLayer;
        CountryGlyphLayer glyphLayer;
        ChoroplethMap choroMap;

        Panel panel;
        DB database;

        public MapPlot(DB aDatabase, Panel aDestinationPanel, Renderer aRenderer)
        {
            database = aDatabase;
            panel = aDestinationPanel;
            renderer = aRenderer;
            colorMap = CreateColorMap();
            
            SetupFilteredData();
            SetupMapLayers();
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
        /// Sorts the data by Country name and adds the data to the K-Means filter
        /// and the regular Data Cube
        /// </summary>
        private void SetupFilteredData()
        {
            List<string> countryFilter = ParseCountryFilter(iGeoDataPath + "countries_acronyms_europe.txt");

            int numOfElements = 5;
            data = new object[numOfElements, countryFilter.Count];

            //Filter the countries
            int counter=0;
            foreach (string filteredCountry in countryFilter) 
            {
                Country country = (Country)database.countries[filteredCountry];

                //NOTE: numOfElements
                data[0, counter] = counter;
                data[1, counter] = country.medianAge;
                data[2, counter] = country.releases.Count;
                data[3, counter] = country.unemploymentRate;
                data[4, counter] = country.gdbPerCapita;
                counter++;
            }
            dataCube = new DataCube();
            dataCube.SetData(data);
            //kMeansFilter.Input = dataCube;
        }


        private void SetupMapLayers()
        {
            string dir = Directory.GetCurrentDirectory();
            string dataPath = "\\..\\..\\..\\data\\geodata\\maps\\";
            string fileName = "europe_nation";


            ShapeFileReader shapeReader = new ShapeFileReader();
            mapData = shapeReader.Read(  dir + dataPath + fileName + ".shp"
                                        ,dir + dataPath + fileName + ".dbf"
                                        ,dir + dataPath + fileName + ".shx");

            // Border Layer
            borderLayer = new MapPolygonBorderLayer();
            borderLayer.MapData = mapData;

            // Polygon Layer
            polygonLayer = new MapPolygonLayer();
            polygonLayer.MapData = mapData;
            colorMap.Input = dataCube;
            colorMap.Index = 0;
            polygonLayer.ColorMap = colorMap;


            // Glyph Layer
            glyphLayer = new CountryGlyphLayer();
            glyphLayer.ActiveGlyphPositioner = new CenterGlyphPositioner();
            glyphLayer.ActiveGlyphPositioner.MapData = mapData;
            glyphLayer.Input = dataCube;

            //// Choropleth Map
            choroMap = new ChoroplethMap();

            // Add layers on the proper order
            choroMap.AddLayer(polygonLayer);
            choroMap.AddLayer(borderLayer);
            //choroMap.AddLayer(glyphLayer);
            choroMap.Invalidate();

            renderer.Add(choroMap, panel);
        }



        public List<string> ParseCountryFilter(string filename)
        {
            List<string>  countryFilter = new List<string>();

            // Open the file and read it back.
            StreamReader sr = File.OpenText(filename);
            string text = "";
            countryFilter.Clear();

            while ((text = sr.ReadLine()) != null)
            {
                char[] delimiterChars = { '\t' };
                string[] words = text.Split(delimiterChars);

                // Acronym
                if ( 2 == words.Length )
                {
                    countryFilter.Add(words[0].ToUpperInvariant());
                }
            }

            return countryFilter;
        }







    }
}
