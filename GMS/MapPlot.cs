using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;

using MusicDataminer;

using Gav.Graphics;
using Gav.Data;

namespace GMS
{
    class MapPlot
    {
        private DataCube    iDataCube;
        private MapData     iMapData;
        private ColorMap    iColorMap;

        private Panel panel;
        private Renderer renderer;

        // Layers
        private MapPolygonLayer         polygonLayer;
        private MapPolygonBorderLayer   borderLayer;
        private CountryGlyphLayer       glyphLayer;
        private ChoroplethMap           choroMap;

        public MapPlot(DataCube aDataCube, Panel aDestinationPanel, Renderer aRenderer, ColorMap aColorMap )
        {
            iDataCube = aDataCube;
            panel = aDestinationPanel;
            renderer = aRenderer;
            iColorMap = aColorMap;

            SetupMapLayers();
        }

         private void SetupMapLayers()
        {
            string dir = Directory.GetCurrentDirectory();
            string dataPath = "\\..\\..\\..\\data\\geodata\\maps\\";
            string fileName = "europe_nation";


            ShapeFileReader shapeReader = new ShapeFileReader();
            iMapData = shapeReader.Read(  dir + dataPath + fileName + ".shp"
                                        ,dir + dataPath + fileName + ".dbf"
                                        ,dir + dataPath + fileName + ".shx");

            // Border Layer
            borderLayer = new MapPolygonBorderLayer();
            borderLayer.MapData = iMapData;

            // Polygon Layer
            polygonLayer = new MapPolygonLayer();
            polygonLayer.MapData = iMapData;
            polygonLayer.ColorMap = iColorMap;

            // Glyph Layer
            glyphLayer = new CountryGlyphLayer();
            glyphLayer.ActiveGlyphPositioner = new CenterGlyphPositioner();
            glyphLayer.ActiveGlyphPositioner.MapData = iMapData;
            glyphLayer.Input = iDataCube;

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
