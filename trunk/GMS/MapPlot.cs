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
        private object[,] data;

        List<object> regions;

        private DataCube dataCube;
        private MapData mapData;

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

            //colorMap.Input = dataCube;
            //colorMap.Index = 5;
            //polygonLayer.ColorMap = colorMap;

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
    }
}
