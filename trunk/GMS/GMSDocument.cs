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
        private const string iGeoDataPath = "../../../data/geodata/";


        // private attributes
        private DB iDb;

        /// <summary>
        /// Contains Country names, that can be accessed with an index (key=int, value=string)
        /// </summary>

        private object[,]   iFilteredData;
        private DataCube    iFilteredDataCube;
        private Hashtable   iFilteredCountryNames;
        public  ColorMap    iFilteredColorMap;

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
        public Hashtable GetFilteredCountryNames() { return this.iFilteredCountryNames; }
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
            LinearColorMapPart linearMap = new LinearColorMapPart( Color.FromArgb(0x00ffb200), Color.FromArgb( 0x001919b3) );
            map.AddColorMapPart(linearMap);
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
            iFilteredCountryNames = new Hashtable();
            List<string> countryFilter = ParseCountryFilter(iGeoDataPath + aFilterFileName );

            int numOfElements = 5;
            iFilteredData = new object[numOfElements, countryFilter.Count];

            //Filter the countries
            uint counter = 0;
            foreach (string filteredCountry in countryFilter)
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
                iFilteredCountryNames.Add(counter++, countryTitleCase);
            }
            iFilteredDataCube = new DataCube();
            iFilteredDataCube.SetData( iFilteredData );

            iFilteredColorMap.Input = iFilteredDataCube;
            iFilteredColorMap.Index = 1;
            //kMeansFilter.Input = dataCube;
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


        ////////////////////////////////////////////////////////////////////////////
        //// Method:    SortStyles
        //// FullName:  GMS.GMSDocument.SortStyles
        //// Access:    public 
        //// Returns:   void
        ////////////////////////////////////////////////////////////////////////////
        //public void SortStyles()
        //{
        //    ArrayList sortedStyles = new ArrayList(iDb.styles.Values);
        //    sortedStyles.Sort(new StyleRelevanceComparer());

        //    int i = 0;
        //    foreach (Style style in sortedStyles)
        //    {
        //        Console.WriteLine(i + ": " + style.name + ": " + style.releases.Count);
        //        if( ++i > 29)
        //        {
        //            break;
        //        }
        //    }
        //}

        //////////////////////////////////////////////////////////////////////////
        // Method:    SortArtists
        // FullName:  GMS.GMSDocument.SortArtists
        // Access:    public 
        // Returns:   void
        //////////////////////////////////////////////////////////////////////////
        public void SortArtists()
        {
            ArrayList sortedArtists = new ArrayList(iDb.artists.Values);
            sortedArtists.Sort(new ArtistRelevanceComparer());

            int i = 0;
            foreach (Artist artist in sortedArtists)
            {
                Console.WriteLine(i + ": " + artist.name + ": " + artist.albums.Count);
                if (++i > 29)
                {
                    break;
                }
            }
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

            ArrayList filter = new ArrayList(iFilteredCountryNames.Values);

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
                    if (filter.Contains(release.country.name))
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
                    styleRow.Add(filter.IndexOf(countryName));  // Col 3: Id
                    styleRow.Add(governmentType);               // Col 4: Government Type

                    dataRows.Add(styleRow);                 
                }
                styleLimiter--;
            }

            // the column order
            aQuantitativeDataIndex  = 2; // Area values
            aOrdinalDataIndex       = 1; // Group Labels
            aIdIndex                = 3; // Id
            aLeafNodeLabelIndex     = 0; // Leaf Label

            // ToolTip Components
            toolTipComponents.Add(new GMSToolTipComponent("Country", aLeafNodeLabelIndex));
            toolTipComponents.Add(new GMSToolTipComponent("# Releases", aQuantitativeDataIndex, "Albums"));
            toolTipComponents.Add(new GMSToolTipComponent("Government Type", 4));

            // Create and Fill the DataCube
            object[,,] dataCube = new object[dataRows.Count, 5, 1];

            int i = 0;
            foreach (List<object> row in dataRows)
            {
                int j = 0;
                foreach (object attribute in row)
                {
                    dataCube[i, j++, 0] = attribute;
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
        public object[, ,] BuildStylesUnemploymentRateAreaTree(out int aQuantitativeDataIndex,
            out int aOrdinalDataIndex, out int aIdIndex, out int aLeafNodeLabelIndex,
            List<GMSToolTipComponent> toolTipComponents)
        {
            // Temporary structure to hold the rows of the datacube
            List<List<object>> dataRows = new List<List<object>>();

            ArrayList filter = new ArrayList(iFilteredCountryNames.Values);

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
                    if (! countries.ContainsKey(release.country.name) )
                    {
                        countries.Add(release.country.name, release.country);
                    }
                }

                // Sort the releases
                ArrayList sortedReleases = new ArrayList(countries.Values);
                sortedReleases.Sort(new CountryUnemploymentComparer());

                foreach (Country country in sortedReleases)
                {
                    List<object> styleRow = new List<object>();

                    styleRow.Add(country.name);                 // Col 0: Box Labels
                    styleRow.Add(style.name);                   // Col 1: Group Labels
                    styleRow.Add(country.unemploymentRate);     // Col 2: Area values
                    styleRow.Add(filter.IndexOf(country.name)); // Col 3: Id
                    styleRow.Add(country.govType);              // Col 4: Government Type

                    dataRows.Add(styleRow);
                }
                styleLimiter--;
            }

            // the column order
            aQuantitativeDataIndex  = 2; // Area values
            aOrdinalDataIndex       = 1; // Group Labels
            aIdIndex                = 3; // Id
            aLeafNodeLabelIndex     = 0; // Leaf Label

            // ToolTip Components
            toolTipComponents.Add(new GMSToolTipComponent("Country", aLeafNodeLabelIndex));
            toolTipComponents.Add(new GMSToolTipComponent("Unemployment Rate", aQuantitativeDataIndex, "%"));
            toolTipComponents.Add(new GMSToolTipComponent("Government Type", 4));

            // Create and Fill the DataCube
            object[, ,] dataCube = new object[dataRows.Count, 5, 1];

            int i = 0;
            foreach (List<object> row in dataRows)
            {
                int j = 0;
                foreach (object attribute in row)
                {
                    dataCube[i, j++, 0] = attribute;
                }
                ++i;
            }

            return dataCube;
        }

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

    //class StyleRelevanceComparer : IComparer
    //{
    //    // Comparator class for music styles
    //    int IComparer.Compare(object x, object y)
    //    {
    //        Style s1 = (Style)x;
    //        Style s2 = (Style)y;

    //        return s2.releases.Count - s1.releases.Count;
    //    }
    //};

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
