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

        /// <summary>
        /// Constructor
        /// </summary>
        public GMSDocument()
        {
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
            LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 180.0f);
            map.AddColorMapPart(hsvMap);
            hsvMap.Invalidate();
            map.Invalidate();
            return map;
        }

        /// <summary>
        /// Create a Datacube accoring to the Filter provided as input (filename).
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
                iFilteredData[2, counter] = country.releases.Count;
                iFilteredData[3, counter] = country.unemploymentRate;
                iFilteredData[4, counter] = country.gdbPerCapita;
                
                string countryTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country.name);
                iFilteredCountryNames.Add(counter++, countryTitleCase);
            }
            iFilteredDataCube = new DataCube();
            iFilteredDataCube.SetData( iFilteredData );

            iFilteredColorMap.Input = iFilteredDataCube;
            iFilteredColorMap.Index = 0;
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
            iSortedColorMap.Index = 0;
            //kMeansFilter.Input = dataCube;
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


        //////////////////////////////////////////////////////////////////////////
        // Method:    SortStyles
        // FullName:  GMS.GMSDocument.SortStyles
        // Access:    public 
        // Returns:   void
        //////////////////////////////////////////////////////////////////////////
        public void SortStyles()
        {
            ArrayList sortedStyles = new ArrayList(iDb.styles.Values);
            sortedStyles.Sort(new StyleRelevanceComparer());

            int i = 0;
            foreach (Style style in sortedStyles)
	        {
                Console.WriteLine(i + ": " + style.name + ": " + style.releases.Count);
                if( ++i > 29)
                {
                    break;
                }
	        }
        }

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

      
        //void pcPlot_LinePicked(object sender, EventArgs e)
        //{
        //    List<int> selectedLines =((ParallelCoordinatesPlot)sender).GetSelectedLineIndexes();
        //    Font font = new Font("Verdana", 10);
        //    Color color = Color.DarkKhaki;
        //    int countriesCount = countries.Count;

        //    foreach (int countryId in selectedLines)
        //    {
        //        string country = (string)countries[(uint)countryId];
        //        float verticalPosition = countryId / countriesCount;
                
        //        pcPlot.AddText(country, ParallelCoordinatesPlot.TextRelativePosition.Left,
        //        color, font, verticalPosition);
        //    }

        //}


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

    class StyleRelevanceComparer : IComparer
    {
        // Comparator class for music styles
        int IComparer.Compare(object x, object y)
        {
            Style s1 = (Style)x;
            Style s2 = (Style)y;

            return s2.releases.Count - s1.releases.Count;
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

}
