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

using Microsoft.DirectX;

namespace GMS
{
    class MapPlot
    {
        private DataCube iDataCube;
        private MapData iMapData;
        private ColorMap iColorMap;

        private Panel iPanel;
        private Renderer renderer;

        // Layers
        private MapPolygonLayer polygonLayer;
        private MapPolygonBorderLayer borderLayer;
        private CountryGlyphLayer glyphLayer;
        private ChoroplethMap choroMap;
        private ParallelCoordinatesPlot iPcPlot;

        private List<string> iCountryNames;

        private GMSDocument iDoc;

        //TOOLTIP STUFF
        private const int TOOLTIP_FADE_DELAY = 500;
        private GavToolTip iToolTip;

        private MouseHoverController iMouseHoverControl;

        public MapPlot(DataCube aDataCube, Panel aDestinationPanel, Renderer aRenderer, 
            ColorMap aColorMap, ParallelCoordinatesPlot aPcPlot, GMSDocument aDoc)
        {
            iDataCube = aDataCube;
            iPanel = aDestinationPanel;
            renderer = aRenderer;
            iColorMap = aColorMap;
            iPcPlot = aPcPlot;
            iDoc = aDoc;
            SetupMapLayers();

            //Get country names
            iCountryNames = new List<string>();
            ArrayList list = new ArrayList( iDoc.GetFilteredCountryNames().Values );
            foreach( string name in list)
            {
                iCountryNames.Insert( 0, name );
            }

            iToolTip = new GavToolTip(iPanel);
            iToolTip.FadeEnable = true;
            iToolTip.FadeTime = TOOLTIP_FADE_DELAY;
            iToolTip.Show(new Point(0,0));
            iToolTip.Hide();

            iMouseHoverControl = new MouseHoverController(iPanel, 5, 1000);
            iMouseHoverControl.Hover += new EventHandler(iMouseHoverControl_Hover);
            iMouseHoverControl.HoverEnd += new EventHandler(iMouseHoverControl_HoverEnd);

            iDoc.Picked += new EventHandler<IndexesPickedEventArgs>(DocumentPicked);
        }

        void iMouseHoverControl_HoverEnd(object sender, EventArgs e)
        {
            iToolTip.Hide();
        }

        void iMouseHoverControl_Hover(object sender, EventArgs e)
        {
            Vector2 v = choroMap.ConvertScreenCoordinatesToMapCoordinates(iMouseHoverControl.HoverPosition);
            int index = iMapData.GetRegionId(v);
            
            if(index != -1)
            {
            iToolTip.Hide();
            iToolTip.Text = iCountryNames[ index ]
                + "\nAge: " + iDataCube.Data[1, index, 0]
                + "\nGNP(per capita): " + iDataCube.Data[4, index, 0] + "$"
                + "\nUnemployment Rate: " + iDataCube.Data[3, index, 0] + "%"
                ;
            iToolTip.Show( iMouseHoverControl.HoverPosition );
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DocumentPicked(object sender, IndexesPickedEventArgs e)
        {
            polygonLayer.SetSelectedPolygonIndexes(e.PickedIndexes);
            choroMap.Invalidate();
        }

        private void SetupMapLayers()
        {
            string dir = Directory.GetCurrentDirectory();
            string dataPath = "\\..\\..\\..\\data\\geodata\\maps\\";
            string fileName = "europe_nation";


            ShapeFileReader shapeReader = new ShapeFileReader();
            iMapData = shapeReader.Read(dir + dataPath + fileName + ".shp"
                                        , dir + dataPath + fileName + ".dbf"
                                        , dir + dataPath + fileName + ".shx");

            // Border Layer
            borderLayer = new MapPolygonBorderLayer();
            borderLayer.MapData = iMapData;

            // Polygon Layer
            polygonLayer = new MapPolygonLayer();
            polygonLayer.MapData = iMapData;
            polygonLayer.ColorMap = iColorMap;
            polygonLayer.IndexVisibilityHandler = iPcPlot.IndexVisibilityHandler;
            iPcPlot.FilterChanged += new EventHandler(iPcPlot_FilterChanged);


            // Glyph Layer
            glyphLayer = new CountryGlyphLayer();

            glyphLayer.ActiveGlyphPositioner = new CenterGlyphPositioner();
            glyphLayer.ActiveGlyphPositioner.MapData = iMapData;
            glyphLayer.Input = iDataCube;
            glyphLayer.IndexVisibilityHandler = iPcPlot.IndexVisibilityHandler;

            //// Choropleth Map
            choroMap = new ChoroplethMap();

            choroMap.VizComponentMouseDown += new EventHandler<VizComponentMouseEventArgs>(MouseDown);
            
            

            // Add layers on the proper order
            choroMap.AddLayer(polygonLayer);
            choroMap.AddLayer(borderLayer);
            choroMap.AddLayer(glyphLayer);
            choroMap.Invalidate();

            renderer.Add(choroMap, iPanel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MouseDown(object sender, VizComponentMouseEventArgs e) 
        {
            Vector2 v = choroMap.ConvertScreenCoordinatesToMapCoordinates(e.MouseEventArgs.Location);
            int index = iMapData.GetRegionId(v);

            List<int> selectedItems = new List<int>();
            
            if (index != -1)
            {
                selectedItems.Add(index);
            }
            
            // if CTRL is pressed, add the line to the selection
            Keys keys = Control.ModifierKeys;
            bool add = (keys == Keys.Control);

            iDoc.SetSelectedItems(selectedItems, add, true);
            //polygonLayer.SetSelectedPolygonIndexes(iDoc.GetSelectedItems());
            //choroMap.Invalidate();
        }

        void iPcPlot_FilterChanged(object sender, EventArgs e)
        {
            choroMap.Invalidate();
        }

    }
}
