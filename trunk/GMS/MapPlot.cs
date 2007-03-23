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
        private MapPolygonLayer polygonSelectionLayer;
        private MapPolygonBorderLayer borderLayer;
        private CountryGlyphLayer glyphLayer;
        private ChoroplethMap choroMap;
        private ParallelCoordinatesPlot iPcPlot;
        private IndexVisibilityHandler iSelectedVisibility;
        private IndexVisibilityList iVisibilityList;


        private List<string> iCountryNames;

        private GMSDocument iDoc;

        private InteractiveColorLegend iColorLegend;

        //TOOLTIP STUFF
        private const int TIMER_DELAY = 200;
        private const int TOOLTIP_FADE_DELAY = 200;
        private GavToolTip iToolTip;

        private MouseHoverController iMouseHoverControl;
        private Point iMouseClickPoint;

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
            iCountryNames = iDoc.GetFilteredCountryNames();

            iToolTip = new GavToolTip(iPanel);
            iToolTip.FadeEnable = true;
            iToolTip.FadeTime = TOOLTIP_FADE_DELAY;
            iToolTip.Show(new Point(0,0));
            iToolTip.Hide();

            iMouseHoverControl = new MouseHoverController(iPanel, 5, TIMER_DELAY);
            iMouseHoverControl.Hover += new EventHandler(iMouseHoverControl_Hover);
            iMouseHoverControl.HoverEnd += new EventHandler(iMouseHoverControl_HoverEnd);

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
            Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            Country country = (Country)iDoc.GetDatabase().countries[ iDoc.GetFilteredAcronyms()[index] ];
            iToolTip.Text = iCountryNames[index]
                + "\nMedian Age: " + country.medianAge
                + "\nAlbum Releases: " + country.releases.Count
                + "\nGDP(per capita): " + country.gdbPerCapita + "$"
                + "\nUnemployment Rate: " + country.unemploymentRate + "%"
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
            for (int i = 0, endI = iMapData.RegionList.Count; i < endI; i++)
            {
                iVisibilityList.SetVisibility(i, 0, true);
            }
            //polygonLayer.SetSelectedPolygonIndexes(e.PickedIndexes);
            //polygonSelectionLayer.SetSelectedPolygonIndexes(e.PickedIndexes);
            foreach (int index in e.PickedIndexes)
            {
                iVisibilityList.SetVisibility(index, 0, false ); //!iVisibilityList.GetVisibility(index));
            }
            iVisibilityList.CommitChanges();
            Invalidate();
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

            polygonSelectionLayer = new MapPolygonLayer();
            polygonSelectionLayer.MapData = iMapData;
            polygonSelectionLayer.PolygonColor = Color.FromArgb(220,220,220);
            polygonSelectionLayer.Alpha = 150;

            iSelectedVisibility = new IndexVisibilityHandler(iMapData.RegionList.Count);
            iSelectedVisibility.Clear();
            iVisibilityList = iSelectedVisibility.CreateVisibilityList();

            polygonSelectionLayer.IndexVisibilityHandler = iSelectedVisibility;

//            polygonSelectionLayer.SelectedPolygonColor = Color.Transparent;

            // Glyph Layer
            glyphLayer = new CountryGlyphLayer( iPanel );
            glyphLayer.ActiveGlyphPositioner = new CenterGlyphPositioner();
            glyphLayer.ActiveGlyphPositioner.MapData = iMapData;
            glyphLayer.Input = iDataCube;
            glyphLayer.IndexVisibilityHandler = iPcPlot.IndexVisibilityHandler;

            // Choropleth Map
            choroMap = new ChoroplethMap();
            choroMap.VizComponentMouseDown += new EventHandler<VizComponentMouseEventArgs>(MouseDown);
            choroMap.VizComponentMouseUp += new EventHandler<VizComponentMouseEventArgs>(MouseUp);
            
            // Add layers on the proper order
            choroMap.AddLayer(polygonLayer);
            choroMap.AddLayer(polygonSelectionLayer);
            choroMap.AddLayer(borderLayer);
            choroMap.AddLayer(glyphLayer);
            Invalidate();

            iColorLegend = new InteractiveColorLegend();
            iColorLegend.ColorMap = iColorMap;
            iColorLegend.BorderColor = Color.Black;
            iColorLegend.SliderTextColor = Color.Black;
            iColorLegend.ShowMinMaxValues = true;
            
            iColorLegend.SetPosition(0.03F, 0.01F);
            iColorLegend.SetLegendSize(0.01f, 0.2f);

            iColorLegend.ShowColorEdgeSliders = true;
            iColorLegend.ShowColorEdgeSliderValue = true;
            iColorLegend.ColorEdgeValuesChanged += new EventHandler(ColorLegendChanged);

            choroMap.AddSubComponent( iColorLegend );
            renderer.Add(choroMap, iPanel);
        }


        void ColorLegendChanged(object sender, EventArgs e)
        {
            iDoc.OnColorMapChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MouseDown(object sender, VizComponentMouseEventArgs e) 
        {
            //Store the clik start position
            if (e.MouseEventArgs.Button == MouseButtons.Left)
            {
                iMouseClickPoint = e.MouseEventArgs.Location;

            }
        }

        void MouseUp(object sender, VizComponentMouseEventArgs e)
        {
            const int treshold = 10;

            //Trying to see if mouse was dragged or was it clicked
            if (e.MouseEventArgs.Button == MouseButtons.Left
                && (iMouseClickPoint.X - treshold < e.MouseEventArgs.Location.X && iMouseClickPoint.X + treshold > e.MouseEventArgs.Location.X)
                && (iMouseClickPoint.Y - treshold < e.MouseEventArgs.Location.Y && iMouseClickPoint.Y + treshold > e.MouseEventArgs.Location.Y)                
                )
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
            }
        }



        void iPcPlot_FilterChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Invalidate()
        {
            choroMap.Invalidate();
        }
    }
}
