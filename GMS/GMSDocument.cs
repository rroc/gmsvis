using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;

using MusicDataminer;

using Gav.Graphics;
using Gav.Data;
using System.Runtime.CompilerServices;

namespace GMS
{
    class GMSDocument
    {
        // CONST DATA
        private const string KGeoDataPath = "../../../data/geodata/";
        private const string KFlagDataPath = "../../../data/flags/";


        // private attributes
        private DB iDb;

        /// <summary>
        /// Contains Country names, that can be accessed with an index (key=int, value=string)
        /// </summary>

        private object[,]       iFilteredData;
        private DataCube        iFilteredDataCube;
        private List<string>    iFilteredCountryNames;
        //private List<string>    iFilteredFlagFiles;
        List<string>            iFilteredAcronyms;
        public  ColorMap        iFilteredColorMap;

        private object[,]   iSortedData;
        private DataCube    iSortedDataCube;
        private Hashtable   iSortedCountryNames;
        public  ColorMap    iSortedColorMap;

        // Selection List
        private Hashtable   iSelectedIndexes;
        
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<IndexesPickedEventArgs> Picked;

        public event EventHandler<EventArgs> ColorMapChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public GMSDocument()
        {
            iSelectedIndexes = new Hashtable();
        }

        //GETTERS:
        public DB GetDatabase() { return this.iDb; }
        public DataCube GetFilteredDataCube() { return this.iFilteredDataCube; }
        public DataCube GetSortedDataCube() { return this.iSortedDataCube; }
        
        public List<string> GetFilteredCountryNames() { return this.iFilteredCountryNames; }
        public List<string> GetFilteredAcronyms() { return this.iFilteredAcronyms; }
        //public List<string> GetFilteredFlagNames() { return this.iFilteredFlagFiles; }

        public Hashtable GetSortedCountryNames() { return this.iSortedCountryNames; }

        public void ReadDB(string filename)
        {
            bool loaded = MusicDBLoader.LoadDB(filename, out iDb);

            if (!loaded)
            {
                MessageBox.Show("Database not loaded :(");
            }
        }

        /// <summary>
        /// Creates a HSV(0.0, 180.0) Color Map
        /// </summary>
        /// <returns></returns>
        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            //LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 270.0f);
            //map.AddColorMapPart(hsvMap);
            //hsvMap.Invalidate();
            LinearColorMapPart linearMap = new LinearColorMapPart(Color.FromArgb(0x0051a87b), Color.FromArgb(0x00bad97a));
            map.AddColorMapPart(linearMap);
            LinearColorMapPart linearMap2 = new LinearColorMapPart(Color.FromArgb(0x00f0e978), Color.FromArgb(0x00d07c59));
            map.AddColorMapPart(linearMap2);

            linearMap.Invalidate();
            map.Invalidate();
            return map;
        }

        /// <summary>
        /// Create a Datacube accoring to the Filter provided as input (filename).
        /// NOTE: The data will be sorted according to the way it is described
        /// in the Europe Map
        /// </summary>
        /// <param name="aFilterFileName">filter filename</param>
        public void SetupFilteredData( string aFilterFileName )
        {
            iFilteredColorMap = CreateColorMap();
            iFilteredCountryNames = new List<string>();
            iFilteredAcronyms = ParseCountryFilter(KGeoDataPath + aFilterFileName);
            
            //Hashtable flagfiles = ParseFlagNames(KFlagDataPath + "flags.txt");
            //iFilteredFlagFiles = new List<string>();

            int numOfElements = 5;
            iFilteredData = new object[numOfElements, iFilteredAcronyms.Count];

            //Filter the countries
            uint counter = 0;
            foreach (string filteredCountry in iFilteredAcronyms)
            {
                Country country = (Country)iDb.countries[filteredCountry];

                //NOTE: numOfElements
                iFilteredData[0, counter] = counter;
                iFilteredData[1, counter] = country.medianAge;
                iFilteredData[2, counter] = (Math.Log(country.releases.Count, 2) > 0)? Math.Log(country.releases.Count, 2):0;
                iFilteredData[3, counter] = country.unemploymentRate;
                iFilteredData[4, counter] = country.gdbPerCapita;
                
                string countryTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country.name);
                if (countryTitleCase.Length > 14)
                {
                    countryTitleCase = countryTitleCase.Substring(0, 14);
                }

                iFilteredCountryNames.Add(countryTitleCase);
//                iFilteredFlagFiles.Add( (string)flagfiles[countryTitleCase]);
                counter++;
            }
            iFilteredDataCube = new DataCube();
            iFilteredDataCube.SetData( iFilteredData );

            iFilteredColorMap.Input = iFilteredDataCube;
            iFilteredColorMap.Index = 1;
        }



        /// <summary>
        /// Sorts the data by Country name and adds the data to the K-Means filter
        /// and the regular Data Cube
        /// </summary>
        public void SetupSortedData()
        {
            iSortedColorMap = CreateColorMap();
            iSortedCountryNames = new Hashtable();
            List<object[]> filteredCountries = new List<object[]>();
            ArrayList sortedCountries = new ArrayList( iDb.countries.Values);
            sortedCountries.Sort(new CountryComparer());

            //Dynamically allocate memory for the data
            uint i = 0;
            iSortedCountryNames.Clear();
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
                    iSortedCountryNames.Add(i++, countryTitleCase);
                }
            }

            //Allocate memory for the datacube data
            iSortedData = new object[5, filteredCountries.Count];
            i = 0;
            foreach (object[] obj in filteredCountries)
            {
                // copy every attribute
                for (int j = 0; j < 5; j++)
                {
                    iSortedData[j, i] = obj[j];
                }
                ++i;
            }

            iSortedDataCube = new DataCube();
            iSortedDataCube.SetData(iSortedData);
            iSortedColorMap.Input = iSortedDataCube;
            iSortedColorMap.Index = 1;
            //kMeansFilter.Input = dataCube;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedLinesIndexes"></param>
        /// <param name="add"></param>
        /// <param name="removeIfSelected"></param>
        public void SetSelectedItems(List<int> aSelectedIndices, bool aAdd, bool aRemoveIfSelected)
        {
            // removing or adding an empty set
            if ( ! aAdd || (aSelectedIndices.Count == 0))
            {
                iSelectedIndexes.Clear();
            }

            if (aRemoveIfSelected)
            {
                foreach (int index in aSelectedIndices)
                {
                    if (iSelectedIndexes.ContainsKey(index))
                    {
                    	iSelectedIndexes.Remove(index);
                    }
                    else
                    {
                        iSelectedIndexes.Add(index, index);
                    }
                }
            }
            else
            {
                foreach (int index in aSelectedIndices)
                {
                    iSelectedIndexes.Add(index, index);
                }
            }

            // Generate the event
            OnPicked(GetSelectedItems());
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnColorMapChanged()
        {
            iSortedColorMap.Invalidate();
            iFilteredColorMap.Invalidate();

            if (this.ColorMapChanged != null)
            {
                this.ColorMapChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<int> GetSelectedItems()
        {
            List<int> items = new List<int>();

            foreach (int index in iSelectedIndexes.Values)
            {
                items.Add(index);
            }

            return items;
        }

        /// <summary>
        /// Read a filter file of format: acronym\tcountry name\n
        /// </summary>
        /// <param name="filename">name of the filter file</param>
        /// <returns>list of acronyms</returns>
        private List<string> ParseCountryFilter(string filename)
        {
            List<string> countryFilter = new List<string>();

            // Open the file and read it back.
            StreamReader sr = File.OpenText(filename);
            string text = "";
            countryFilter.Clear();

            while ((text = sr.ReadLine()) != null)
            {
                char[] delimiterChars = { '\t' };
                string[] words = text.Split(delimiterChars);

                // Acronym, Country Name
                if (2 == words.Length)
                {
                    countryFilter.Add(words[0].ToUpperInvariant());
                }
            }
            return countryFilter;
        }

        /// <summary>
        /// Read a flag file of format: country name\tfilename\n
        /// </summary>
        /// <param name="filename">name of the filter file</param>
        /// <returns>Hashtable of filenames</returns>
        private Hashtable ParseFlagNames(string filename)
        {
            Hashtable flagNames = new Hashtable();

            // Open the file and read it back.
            StreamReader sr = File.OpenText(filename);
            string text = "";
            flagNames.Clear();

            while ((text = sr.ReadLine()) != null)
            {
                char[] delimiterChars = { '\t' };
                string[] words = text.Split(delimiterChars);

                string countryTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[0]);
                if (countryTitleCase.Length > 14)
                {
                    countryTitleCase = countryTitleCase.Substring(0, 14);
                }
                // Acronym, Country Name
                if (2 == words.Length)
                {
                    flagNames.Add(countryTitleCase, words[1]);
                }
            }

            ////FLAGS Stuff
            ////LOAD FLAGS
            //string dir = Directory.GetCurrentDirectory();
            //string dataPath = "\\..\\..\\..\\data\\flags\\";
            //iTexture = new List<Bitmap>();
            //foreach (string flagFile in iDoc.GetFilteredFlagNames())
            //{
            //    try
            //    {
            //        iTexture.Insert(0, new Bitmap(dir + dataPath + flagFile));
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("File not found(" + e + "): " + flagFile);
            //    }
            //}
            ////DRAW FLAGS
            //int countryIndex = iDoc.GetFilteredCountryNames().IndexOf(country.name);
            //Size size = new Size(40, 30);
            //System.Drawing.Rectangle flagRectangle = new System.Drawing.Rectangle(iMouseHoverControl.HoverPosition, size);
            //TextureBrush textureBrush = new TextureBrush(iTexture[countryIndex]);
            //System.Drawing.Drawing2D.Matrix mx = new System.Drawing.Drawing2D.Matrix(((float)size.Width / iTexture[index].Width), 0, 0, ((float)size.Height / iTexture[index].Height), 0, 0);
            //textureBrush.Transform = mx;
            //Graphics g = iPanel.CreateGraphics();
            //g.FillRectangle(textureBrush, flagRectangle);

            return flagNames;
        }


        /// <summary>
        /// Creates the TreeMap for the countries grouped per Style
        /// </summary>
        /// <param name="aRectangle"></param>
        /// <param name="db"></param>
        public object[, ,] BuildCountriesAreasTree(out int aQuantitativeDataIndex,
            out int aOrdinalDataIndex, out int aIdIndex, out int aLeafNodeLabelIndex,
            List<GMSToolTipComponent> toolTipComponents, out object[,,] aColorMapInput)
        {
            List<List<object>> dataRows = new List<List<object>>();
            
            // Sort the countries so we can have only the N most popular ones
            ArrayList sortedCountries = new ArrayList(iDb.countries.Values);
            sortedCountries.Sort(new CountryComparer(CountryComparer.CRITERION.RELEASES));

            Hashtable styles = new Hashtable();

            //int countryLimiter = 20;
            int idCounter = 0;
            foreach (Country country in sortedCountries)
            {
                //if (countryLimiter == 0)
                //{
                //    break;
                //}

                foreach (StyleTreeMapInfo styleInfo in styles.Values)
                {
                    styleInfo.iReleaseCount = 0;
                }

                // iterate the releases and increment the counter for each style
                foreach (MusicBrainzRelease release in country.releases)
                {
                    string styleName = release.freeDBAlbum.style.name;
                    if (styles.ContainsKey(styleName))
                    {
                        StyleTreeMapInfo info = (StyleTreeMapInfo)styles[styleName];
                        info.iReleaseCount++;
                    }
                    else
                    {
                        StyleTreeMapInfo info = new StyleTreeMapInfo();
                        info.iReleaseCount = 1;
                        info.id = idCounter++;
                        styles.Add(styleName, info);
                    }
                }

                // Sort the styles in a decreasing way
                ArrayList sortedStyles = new ArrayList(styles);
                sortedStyles.Sort(new StyleTreeMapInfoComparer());

                int styleLimiter = 30;
                foreach (DictionaryEntry entry in sortedStyles)
                {
                    StyleTreeMapInfo styleInfo = (StyleTreeMapInfo)entry.Value;
                    int releasesCount = styleInfo.iReleaseCount;

                    if (releasesCount == 0) { continue; }
                    if (styleLimiter == 0) { break; }

                    string styleName = (string)entry.Key;
                    string countryName = country.name;
                    
                    List<object> countryRow = new List<object>();

                    countryRow.Add(styleName);      // Col 0: Leaf Labels
                    countryRow.Add(countryName);    // Col 1: Group Labels
                    countryRow.Add(releasesCount);  // Col 2: Area values
                    countryRow.Add(styleInfo.id);   // Col 3: Id

                    dataRows.Add(countryRow);
                    styleLimiter--;
                }
                //countryLimiter--;
            }

            // Removing the id's gaps by creating increasing ids
            Hashtable idHash = new Hashtable();

            int idCount = 0;
            foreach (List<object> row in dataRows)
            {
                int id = (int)row[3];
                if (! idHash.ContainsKey(id))
                {
                    idHash.Add(id, idCount++);
                }
            }

            // the column order
            aQuantitativeDataIndex = 2; // Area values
            aOrdinalDataIndex = 1;      // Group Labels
            aIdIndex = 3;               // Id
            aLeafNodeLabelIndex = 0;    // Leaf Label

            // ToolTip Components
            toolTipComponents.Add(new GMSToolTipComponent("Country", aOrdinalDataIndex));
            toolTipComponents.Add(new GMSToolTipComponent("Music Style", aLeafNodeLabelIndex));
            toolTipComponents.Add(new GMSToolTipComponent("Nr. Albums", aQuantitativeDataIndex));

            // Create and Fill the DataCube
            object[, ,] dataCube    = new object[4, dataRows.Count, 1];
            aColorMapInput          = new object[1, idHash.Count, 1];

            // Fill the color map input
            int j = 0;
            foreach (int id in idHash.Values)
            {
                aColorMapInput[0, j++, 0] = id;
            }

            int i = 0;
            foreach (List<object> row in dataRows)
            {
                j = 0;
                foreach (object attribute in row)
                {
                    if (j == 3)
                    {
                        dataCube[j++, i, 0] = idHash[attribute];
                    }
                    else
                    {
                        dataCube[j++, i, 0] = attribute;
                    }
                }
                ++i;
            }

            return dataCube;
        }

        /// <summary>
        /// Creates the TreeMap for the countries grouped per Style
        /// </summary>
        /// <param name="aRectangle"></param>
        /// <param name="db"></param>
        public object[, ,] BuildStylesAreasTree(out int aQuantitativeDataIndex,
            out int aOrdinalDataIndex, out int aIdIndex, out int aLeafNodeLabelIndex,
            List<GMSToolTipComponent> toolTipComponents)
        {
            List<List<object>> dataRows = new List<List<object>>();

            // Sort the styles so we can have only the N most popular ones
            ArrayList sortedStyles = new ArrayList(iDb.styles.Values);
            sortedStyles.Sort(new StyleComparer());

            int styleLimiter = 20;
            foreach (Style style in sortedStyles)
            {
                if (styleLimiter == 0)
                {
                    break;
                }

                Hashtable countries = new Hashtable();

                // count all the occurrences in the countries
                foreach (MusicBrainzRelease release in style.releases)
                {
                    //according to the filter used on pcplot
                    if (iFilteredCountryNames.Contains(release.country.name))
                    {
                        if (countries.ContainsKey(release.country.acronym))
                        {
                            int oldvalue = (int)countries[release.country.acronym];
                            countries[release.country.acronym] = oldvalue + 1;
                        }
                        else
                        {
                            countries.Add(release.country.acronym, 1);
                        }
                    }
                }

                // Sort the releases
                ArrayList sortedReleases = new ArrayList(countries);
                sortedReleases.Sort(new DictionaryValueComparer());

                foreach (DictionaryEntry entry in sortedReleases)
                {
                    Country country = (Country)iDb.countries[(string)entry.Key];
                    string countryName = country.name;
                    int releasesCount = (int)countries[(string)entry.Key];
                    string governmentType = country.govType;
                    List<object> styleRow = new List<object>();

                    styleRow.Add(countryName);                  // Col 0: Box Labels
                    styleRow.Add(style.name);                   // Col 1: Group Labels
                    styleRow.Add(releasesCount);                // Col 2: Area values
                    styleRow.Add(iFilteredCountryNames.IndexOf(countryName));  // Col 3: Id
                    styleRow.Add(governmentType);               // Col 4: Government Type

                    dataRows.Add(styleRow);
                }
                styleLimiter--;
            }

            // the column order
            aQuantitativeDataIndex = 2; // Area values
            aOrdinalDataIndex = 1; // Group Labels
            aIdIndex = 3; // Id
            aLeafNodeLabelIndex = 0; // Leaf Label

            // ToolTip Components
            toolTipComponents.Add(new GMSToolTipComponent("Country", aLeafNodeLabelIndex));
            toolTipComponents.Add(new GMSToolTipComponent("Nr. Albums", aQuantitativeDataIndex));
            toolTipComponents.Add(new GMSToolTipComponent("Government Type", 4));

            // Create and Fill the DataCube
            object[, ,] dataCube = new object[5, dataRows.Count, 1];

            int i = 0;
            foreach (List<object> row in dataRows)
            {
                int j = 0;
                foreach (object attribute in row)
                {
                    dataCube[j++, i, 0] = attribute;
                }
                ++i;
            }

            return dataCube;
        }

        ///// <summary>
        ///// Creates the TreeMap for the countries grouped per Style. The grouping
        ///// criterion is either Unemployment Rate or Median Age
        ///// </summary>
        ///// <param name="aQuantitativeDataIndex"></param>
        ///// <param name="aOrdinalDataIndex"></param>
        ///// <param name="aIdIndex"></param>
        ///// <param name="aLeafNodeLabelIndex"></param>
        ///// <param name="toolTipComponents"></param>
        ///// <param name="aMedianAge">if true uses Median Age as the criterion, otherwise
        ///// it uses Unemployment Rate</param>
        ///// <returns></returns>
        //public object[, ,] BuildStylesCriterionAreaTree(out int aQuantitativeDataIndex,
        //    out int aOrdinalDataIndex, out int aIdIndex, out int aLeafNodeLabelIndex,
        //    List<GMSToolTipComponent> toolTipComponents, bool aMedianAge)
        //{
        //    CountryComparer.CRITERION compareCriterion = aMedianAge ? 
        //        CountryComparer.CRITERION.MEDIANAGE : CountryComparer.CRITERION.UNEMPLOYMENTRATE;

        //    // Temporary structure to hold the rows of the datacube
        //    List<List<object>> dataRows = new List<List<object>>();

        //    // Sort the styles so we can have only the N most popular ones
        //    ArrayList sortedStyles = new ArrayList(iDb.styles.Values);
        //    sortedStyles.Sort(new StyleComparer());

        //    int styleLimiter = 20;
        //    foreach (Style style in sortedStyles)
        //    {
        //        if (styleLimiter == 0)
        //        {
        //            break;
        //        }

        //        Hashtable countries = new Hashtable();

        //        // count all the occurrences in the countries
        //        foreach (MusicBrainzRelease release in style.releases)
        //        {
        //            if (! countries.ContainsKey(release.country.name) )
        //            {
        //                countries.Add(release.country.name, release.country);
        //            }
        //        }

        //        // Sort the releases
        //        ArrayList sortedReleases = new ArrayList(countries.Values);
        //        sortedReleases.Sort(new CountryComparer(compareCriterion));

        //        foreach (Country country in sortedReleases)
        //        {
        //            List<object> styleRow = new List<object>();

        //            styleRow.Add(country.name);                 // Col 0: Box Labels
        //            styleRow.Add(style.name);                   // Col 1: Group Labels

        //            if (compareCriterion == CountryComparer.CRITERION.UNEMPLOYMENTRATE)
        //            {
        //                styleRow.Add(country.unemploymentRate); // Col 2: Area values
        //            }
        //            else
        //            {
        //                styleRow.Add(country.medianAge);        // Col 2: Area values
        //            }

        //            styleRow.Add(iFilteredCountryNames.IndexOf(country.name)); // Col 3: Id
        //            styleRow.Add(country.govType);              // Col 4: Government Type

        //            // switch the roles, so we can have both info on the tooltip
        //            if (compareCriterion == CountryComparer.CRITERION.UNEMPLOYMENTRATE)
        //            {
        //                styleRow.Add(country.medianAge);        // Col 5: unchosen criterion
        //            }
        //            else
        //            {
        //                styleRow.Add(country.unemploymentRate); // Col 5: unchosen criterion
        //            }

        //            dataRows.Add(styleRow);
        //        }
        //        styleLimiter--;
        //    }

        //    // the column order
        //    aQuantitativeDataIndex  = 2; // Area values
        //    aOrdinalDataIndex       = 1; // Group Labels
        //    aIdIndex                = 3; // Id
        //    aLeafNodeLabelIndex     = 0; // Leaf Label

        //    // ToolTip Components
        //    toolTipComponents.Add(new GMSToolTipComponent("Country", aLeafNodeLabelIndex));
            
        //    // Add the tool tip according to the Criterion
        //    if (compareCriterion == CountryComparer.CRITERION.UNEMPLOYMENTRATE)
        //    {
        //        toolTipComponents.Add(new GMSToolTipComponent("Unemployment Rate", aQuantitativeDataIndex, "%"));
        //        toolTipComponents.Add(new GMSToolTipComponent("Median Age", 5, "years"));
        //    }
        //    else
        //    {
        //        toolTipComponents.Add(new GMSToolTipComponent("Median Age", aQuantitativeDataIndex, "years"));
        //        toolTipComponents.Add(new GMSToolTipComponent("Unemployment Rate", 5, "%"));
        //    }
            
        //    toolTipComponents.Add(new GMSToolTipComponent("Government Type", 4));

        //    // Create and Fill the DataCube
        //    object[, ,] dataCube = new object[6, dataRows.Count, 1];

        //    int i = 0;
        //    foreach (List<object> row in dataRows)
        //    {
        //        int j = 0;
        //        foreach (object attribute in row)
        //        {
        //            dataCube[j++, i, 0] = attribute;
        //        }
        //        ++i;
        //    }

        //    return dataCube;
        //}

        ///// <summary>
        ///// Creates the TreeMap for the countries grouped per Style
        ///// </summary>
        ///// <param name="aRectangle"></param>
        ///// <param name="db"></param>
        //public object[, ,] BuildStylesUnemploymentRateAreaTree(out int aQuantitativeDataIndex,
        //    out int aOrdinalDataIndex, out int aIdIndex, out int aLeafNodeLabelIndex,
        //    List<GMSToolTipComponent> toolTipComponents)
        //{
        //    // Temporary structure to hold the rows of the datacube
        //    List<List<object>> dataRows = new List<List<object>>();

        //    // Sort the styles so we can have only the N most popular ones
        //    ArrayList sortedStyles = new ArrayList(iDb.styles.Values);
        //    sortedStyles.Sort(new StyleComparer());

        //    int styleLimiter = 20;
        //    foreach (Style style in sortedStyles)
        //    {
        //        if (styleLimiter == 0)
        //        {
        //            break;
        //        }

        //        Hashtable countries = new Hashtable();

        //        // count all the occurrences in the countries
        //        foreach (MusicBrainzRelease release in style.releases)
        //        {
        //            if (! countries.ContainsKey(release.country.name) )
        //            {
        //                countries.Add(release.country.name, release.country);
        //            }
        //        }

        //        // Sort the releases
        //        ArrayList sortedReleases = new ArrayList(countries.Values);
        //        sortedReleases.Sort(new CountryUnemploymentComparer());

        //        foreach (Country country in sortedReleases)
        //        {
        //            List<object> styleRow = new List<object>();

        //            styleRow.Add(country.name);                 // Col 0: Box Labels
        //            styleRow.Add(style.name);                   // Col 1: Group Labels
        //            styleRow.Add(country.unemploymentRate);     // Col 2: Area values
        //            styleRow.Add(iFilteredCountryNames.IndexOf(country.name)); // Col 3: Id
        //            styleRow.Add(country.govType);              // Col 4: Government Type

        //            dataRows.Add(styleRow);
        //        }
        //        styleLimiter--;
        //    }

        //    // the column order
        //    aQuantitativeDataIndex  = 2; // Area values
        //    aOrdinalDataIndex       = 1; // Group Labels
        //    aIdIndex                = 3; // Id
        //    aLeafNodeLabelIndex     = 0; // Leaf Label

        //    // ToolTip Components
        //    toolTipComponents.Add(new GMSToolTipComponent("Country", aLeafNodeLabelIndex));
        //    toolTipComponents.Add(new GMSToolTipComponent("Unemployment Rate", aQuantitativeDataIndex, "%"));
        //    toolTipComponents.Add(new GMSToolTipComponent("Government Type", 4));

        //    // Create and Fill the DataCube
        //    object[, ,] dataCube = new object[5, dataRows.Count, 1];

        //    int i = 0;
        //    foreach (List<object> row in dataRows)
        //    {
        //        int j = 0;
        //        foreach (object attribute in row)
        //        {
        //            dataCube[j++, i, 0] = attribute;
        //        }
        //        ++i;
        //    }

        //    return dataCube;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexes"></param>
        private void OnPicked(List<int> indexes)
        {
            if (this.Picked != null)
            {
                this.Picked(this, new IndexesPickedEventArgs(indexes));
            }
        }

    }
    //HELPER CLASSES:

    public class CountryComparer : IComparer
    {
        public enum CRITERION{NAME, RELEASES, UNEMPLOYMENTRATE, MEDIANAGE};

        private CRITERION iCompareCriterion;

        public CountryComparer()
            :this(CRITERION.NAME)
        {
        }

        public CountryComparer(CRITERION criterion)
        {
            iCompareCriterion = criterion;
        }

        private int CompareName(Country c1, Country c2)
        {
            return c1.name.CompareTo(c2.name);
        }

        private int CompareUnemploymentRate(Country c1, Country c2)
        {
            return c1.unemploymentRate.CompareTo(c2.unemploymentRate);
        }

        private int CompareMedianAge(Country c1, Country c2)
        {
            return c1.medianAge.CompareTo(c2.medianAge);
        }

        private int CompareReleases(Country c1, Country c2)
        {
            return c2.releases.Count.CompareTo(c1.releases.Count);
        }

        // Comparator class for countries
        int IComparer.Compare(object x, object y)
        {
            Country c1 = (Country)x;
            Country c2 = (Country)y;

            switch (iCompareCriterion)
            {
                case CRITERION.RELEASES:
                    return CompareReleases(c1, c2);
                case CRITERION.UNEMPLOYMENTRATE:
                    return CompareUnemploymentRate(c1, c2);
                case CRITERION.MEDIANAGE:
                    return CompareMedianAge(c1, c2);
                default:
                    return CompareName(c1, c2);
            }
        }
    };

    class CountryUnemploymentComparer : IComparer
    {
        // Comparator class for countries
        int IComparer.Compare(object x, object y)
        {
            Country c1 = (Country)x;
            Country c2 = (Country)y;

            // descendent order
            return c2.unemploymentRate.CompareTo(c1.unemploymentRate);
        }
    };

    class ArtistRelevanceComparer : IComparer
    {
        // Comparator class for music styles
        int IComparer.Compare(object x, object y)
        {
            Artist s1 = (Artist)x;
            Artist s2 = (Artist)y;

            return s2.albums.Count - s1.albums.Count;
        }
    };

    /// <summary>
    /// 
    /// </summary>
    class StyleComparer : IComparer
    {
        // Comparator class for countries
        int IComparer.Compare(object x, object y)
        {
            Style c1 = (Style)x;
            Style c2 = (Style)y;

            // descendent order
            return c2.releases.Count.CompareTo(c1.releases.Count);
        }
    };

    /// <summary>
    /// 
    /// </summary>
    class StyleTreeMapInfo
    {
        public int iReleaseCount;
        public int id;
    };

    /// <summary>
    /// 
    /// </summary>
    class StyleTreeMapInfoComparer : IComparer
    {
        // Comparator class for countries
        int IComparer.Compare(object x, object y)
        {
            DictionaryEntry e1 = (DictionaryEntry)x;
            DictionaryEntry e2 = (DictionaryEntry)y;

            StyleTreeMapInfo st1 = (StyleTreeMapInfo)e1.Value;
            StyleTreeMapInfo st2 = (StyleTreeMapInfo)e2.Value;

            // descendent order
            return st2.iReleaseCount.CompareTo(st1.iReleaseCount);
        }
    };

    class DictionaryValueComparer : IComparer
    {
        // Comparator class for hashtable entries
        // with integer as its value
        int IComparer.Compare(object x, object y)
        {
            DictionaryEntry c1 = (DictionaryEntry)x;
            DictionaryEntry c2 = (DictionaryEntry)y;

            return Convert.ToInt32(Convert.ToSingle(c2.Value) - Convert.ToSingle(c1.Value));
        }
    };

}
