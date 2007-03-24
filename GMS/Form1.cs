using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Gav.Data;
using Gav.Graphics;
using System.Collections;
using MusicDataminer;

namespace GMS
{
    public partial class Form1 : Form
    {
        GMSDocument doc;
        Renderer renderer;
        ParallelPlotCountries pcCountries;
        MapPlot mapPlot;
        TreeMap iTreeMap;

        public Form1()
        {
            InitializeComponent();

            this.glyphCheckBoxes.SetItemChecked(1, true);
            this.glyphCheckBoxes.SetItemChecked(3, true);
            this.glyphPanel.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string dir = Directory.GetCurrentDirectory();
            string iDataPath = "\\..\\..\\..\\data\\";
            string iDBFileName = "dbEurope.bin";

            renderer = new Renderer(this);
            doc = new GMSDocument();
            doc.ReadDB(dir + iDataPath + iDBFileName);
            doc.SetupFilteredData("countries_acronyms_europe.txt");
            doc.SetupSortedData();
            
            pcCountries = new ParallelPlotCountries(  doc.GetFilteredDataCube()
                                                    , doc.GetFilteredCountryNames()
                                                    , panel1
                                                    , renderer
                                                    , doc.iFilteredColorMap
                                                    , doc
                                                    );

            mapPlot = new MapPlot(doc.GetFilteredDataCube()
                                      , PCMapSplitContainer.Panel1
                                      , renderer
                                      , doc.iFilteredColorMap
                                      , pcCountries.pcPlot
                                      , doc
                                      );


            // Initialize Tree Map
            iTreeMap = new TreeMap(mainSplitContainer.Panel2, doc);

            int quantitativeDataIndex, ordinalDataIndex, idIndex, leafNodeIndex;
            List<GMSToolTipComponent> toolTipComponents = new List<GMSToolTipComponent>();

            //// Build the TreeMap Data << Styles releases per Countries >>
            //object[, ,] countriesColorMapData;
            //object[, ,] data = doc.BuildCountriesAreasTree(out quantitativeDataIndex,
            //    out ordinalDataIndex, out idIndex, out leafNodeIndex, 
            //    toolTipComponents, out countriesColorMapData);

            // Build the TreeMap Data << Countries releases per Styles >>
            object[, ,] data = doc.BuildStylesAreasTree(out quantitativeDataIndex,
                out ordinalDataIndex, out idIndex, out leafNodeIndex, toolTipComponents);

            //// Build the TreeMap Data << UnemploymentRates per Styles >>
            //object[, ,] data = doc.BuildStylesUnemploymentRateAreaTree(out quantitativeDataIndex,
            //    out ordinalDataIndex, out idIndex, out leafNodeIndex, toolTipComponents);

            //// Build the TreeMap Data << UnemploymentRates per Styles >>
            //object[, ,] data = doc.BuildStylesCriterionAreaTree(out quantitativeDataIndex,
            //    out ordinalDataIndex, out idIndex, out leafNodeIndex, toolTipComponents, true);

            iTreeMap.SetData(data, quantitativeDataIndex, ordinalDataIndex, idIndex, 
                leafNodeIndex, toolTipComponents);
            
            /************************************************************************/
            /* !!!!!!!!!!!!! TEMPORARY COLORMAP !!!!!!!!                            */
            /************************************************************************/
            //ColorMap map = new ColorMap();
            //LinearColorMapPart linearMap = new LinearColorMapPart(Color.FromArgb(0x0051a87b), Color.FromArgb(0x00bad97a));
            //map.AddColorMapPart(linearMap);
            //LinearColorMapPart linearMap2 = new LinearColorMapPart(Color.FromArgb(0x00f0e978), Color.FromArgb(0x00d07c59));
            //map.AddColorMapPart(linearMap2);
            //DataCube d = new DataCube();
            //d.SetData(countriesColorMapData);
            //map.Input = d;
            //map.Invalidate();

            iTreeMap.UpdateScale();
            iTreeMap.ColorMap = doc.iFilteredColorMap;
            //iTreeMap.ColorMap = map;
            pcCountries.pcPlot.FilterChanged += new EventHandler(pcPlot_FilterChanged);

            this.glyphPanel.Show();
            this.ShowInformationBox();
        }

        void ShowInformationBox()
        {
            MessageBox.Show(
                  "This program visualizes data gathered from three different sources: \n"
                + "- freedb.org:\tAlbum name, Artist, Style\n"
                + "- musicbrainz.org:\tAlbum release year, Release country\n"
                + "- CIA World book:\tGovernment type, Median age, GDP per capita, Unemployment\n"
                + "\n"
                + "The same data is presented in three views:\n"
                + "- MapPlot: 4 types of selectable glyphs\n"
                + "- TreeMap: Select which items you want to compare from drop down menu.\n"
                + "- Parallel Coordinates: Filter and select items.\n"
                + "\n"
                + "Coloring and selection are synchronized with all the views when possible."
                , "Welcome to the GMS Visualizer"
                , MessageBoxButtons.OK
                , MessageBoxIcon.Information
                );
        }

        void pcPlot_FilterChanged(object sender, EventArgs e)
        {
            iTreeMap.Invalidate();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox combo = (ToolStripComboBox)sender;
            string clusters = (string)combo.SelectedItem;
            pcCountries.ToggleFilter(clusters);
            iTreeMap.Invalidate();
            mapPlot.Invalidate();
        }

        private void glyphCheckBoxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            mapPlot.Invalidate(); //PCMapSplitContainer.Panel1.Invalidate();
        }

        private void helpButtonClicked(object sender, EventArgs e)
        {
            ShowInformationBox();
        }
    }
}
