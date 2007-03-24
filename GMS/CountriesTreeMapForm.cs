using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Gav.Data;
using Gav.Graphics;

namespace GMS
{
    public enum TREEMAP_TYPE{COUNTRIES_PER_STYLE, STYLES_PER_COUNTRY, GOV_TYPES};

    public partial class CountriesTreeMapForm : Form
    {
        private TreeMap iTreeMap;
        private GMSDocument iDoc;
        private bool iLoaded;
        private TREEMAP_TYPE iTreeMapType;
        ToolStripComboBox iTreeMapCombo;
        private ColorMap iSharedColorMap;

        public CountriesTreeMapForm(GMSDocument aDoc, string aTreeMapType, 
            ToolStripComboBox aTreeMapCombo, ColorMap aSharedColorMap)
        {
            InitializeComponent();

            iDoc = aDoc;
            iTreeMap = new TreeMap(mainPanel, iDoc);
            iLoaded = false;
            iTreeMapType = GetTreeMapType(aTreeMapType);
            iTreeMapCombo = aTreeMapCombo;
            iSharedColorMap = aSharedColorMap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aOption"></param>
        /// <returns></returns>
        private TREEMAP_TYPE GetTreeMapType(string aOption)
        {
            if (aOption == "Styles per Country")
            {
                return TREEMAP_TYPE.STYLES_PER_COUNTRY;
            }
            else if (aOption == "Countries per Style")
            {
                return TREEMAP_TYPE.COUNTRIES_PER_STYLE;
            }
            else
            {
                return TREEMAP_TYPE.GOV_TYPES;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeTreeMap()
        {
            int quantitativeDataIndex, ordinalDataIndex, idIndex, leafNodeIndex;
            List<GMSToolTipComponent> toolTipComponents = new List<GMSToolTipComponent>();

            object[, ,] countriesColorMapData;
            object[, ,] data;
            ColorMap map;

            switch (iTreeMapType)
            {
                case TREEMAP_TYPE.GOV_TYPES:
                    
                    data = iDoc.BuildGovernmentTypeAreasTree(out quantitativeDataIndex,
                        out ordinalDataIndex, out idIndex, out leafNodeIndex,
                        toolTipComponents, out countriesColorMapData);

                    map = CreateColorMap(countriesColorMapData);

                    break;
                case TREEMAP_TYPE.STYLES_PER_COUNTRY:
                    
                    data = iDoc.BuildCountriesAreasTree(out quantitativeDataIndex,
                        out ordinalDataIndex, out idIndex, out leafNodeIndex,
                        toolTipComponents, out countriesColorMapData);

                    map = CreateColorMap(countriesColorMapData);
                    
                    break;
                case TREEMAP_TYPE.COUNTRIES_PER_STYLE:
                default:

                    data = iDoc.BuildStylesAreasTree(out quantitativeDataIndex,
                        out ordinalDataIndex, out idIndex, out leafNodeIndex,
                        toolTipComponents);

                    map = iSharedColorMap;

                    break;
            }

            iTreeMap.SetData(data, quantitativeDataIndex, ordinalDataIndex, idIndex,
                leafNodeIndex, toolTipComponents);

            iTreeMap.UpdateScale();
            iTreeMap.ColorMap = map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aCountriesColorMapData"></param>
        /// <returns></returns>
        private ColorMap CreateColorMap(object[, ,] aCountriesColorMapData)
        {
            ColorMap map = new ColorMap();
            
            LinearColorMapPart linearMap = new LinearColorMapPart(Color.DarkRed, Color.Orange);
            map.AddColorMapPart(linearMap);
            LinearColorMapPart linearMap2 = new LinearColorMapPart(Color.Blue, Color.Yellow);
            map.AddColorMapPart(linearMap2);
            
            DataCube d = new DataCube();
            d.SetData(aCountriesColorMapData);
            map.Input = d;
            map.Invalidate();

            return map;
        }

        private void CountriesTreeMapForm_Load(object sender, EventArgs e)
        {
            if (! iLoaded)
            {
                InitializeTreeMap();
            }

            Rectangle workingArea = Screen.GetWorkingArea(this);
            int desiredHeight = workingArea.Height;
            int desiredWidth = 2 * (workingArea.Width / 3);

            this.SetClientSizeCore(desiredWidth, desiredHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountriesTreeMapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            iTreeMap.Dispose();
            iTreeMapCombo.SelectedIndex = 0;
        }

    }
}