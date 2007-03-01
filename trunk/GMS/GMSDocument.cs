using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MusicDataminer;
using Gav.Graphics;
using Gav.Data;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;

namespace GMS
{
    /*
     * 
     */
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

    /*
     * Style Comparer
     */
    class StyleRelevanceComparer : IComparer
    {
        // Comparator class for music styles
        int IComparer.Compare(object x, object y)
        {
            Style s1 = (Style)x;
            Style s2 = (Style)y;

            return s2.releases.Count -  s1.releases.Count;
        }
    };

    /*
     * Artist Comparer
     */
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

    class GMSDocument
    {
        // private attributes
        DB db;

        public GMSDocument()
        {
        }


        public void ReadDB(string filename)
        {
            bool loaded = MusicDBLoader.LoadDB(filename, out db);

            if( ! loaded )
            {
                MessageBox.Show("Database not loaded :(");
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    GetDatabase
        // FullName:  GMS.GMSDocument.GetDatabase
        // Access:    public 
        // Returns:   System.Collections.Hashtable
        //////////////////////////////////////////////////////////////////////////
        public DB GetDatabase()
        {
            return this.db;
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    SortStyles
        // FullName:  GMS.GMSDocument.SortStyles
        // Access:    public 
        // Returns:   void
        //////////////////////////////////////////////////////////////////////////
        public void SortStyles()
        {
            ArrayList sortedStyles = new ArrayList(db.styles.Values);
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
            ArrayList sortedArtists = new ArrayList(db.artists.Values);
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
}
