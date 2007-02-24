using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;

namespace MusicDataminer
{
    //
    // Data Structures
    //

    [Serializable]
    public class Country
    {
        public string acronym;
        public string name;
        public int gdbPerCapita;
        public string govType;
        public float unemploymentRate;
        public float medianAge;

        public List<MusicBrainzRelease> releases;
    };

    [Serializable]
    public class MusicBrainzRelease
    {
        public Country country;
        public int date;

        public Album freeDBAlbum;
    };

    [Serializable]
    public class Style
    {
        public List<MusicBrainzRelease> releases;
        public string name;
    };

    [Serializable]
    public class Album
    {
        public string title;

        public Artist artist;
        public Style style;

        public List<MusicBrainzRelease> releases;
    };

    [Serializable]
    public class Artist
    {
        public string name;
        
        public List<Album> albums;
    };


    [Serializable]
    public class DB
    {
        //public List<Album> albums;
        public Hashtable albums;
        public Hashtable countries;
        public Hashtable styles;
        public Hashtable artists;
    };


    public class MusicDBLoader
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveDB(string filename, DB db)
        {
            // file stream states the saved binary
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);

            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, db);
            }
            finally
            {
                fs.Close();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    LoadDB
        // FullName:  MusicDataminer.MusicDBParser.LoadDB
        // Access:    public 
        // Returns:   bool
        //////////////////////////////////////////////////////////////////////////
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool LoadDB(string filename, out DB db)
        {
            // file stream states the saved binary
            FileStream fs = null;
            db = new DB();
            bool loaded = false;

            if (File.Exists(filename))
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    db = (DB)bf.Deserialize(fs);
                    loaded = true;
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("DB not loaded: " + e.Message);
                    return loaded;
                }
                finally
                {
                    fs.Close();
                }
            }

            return loaded;
        }
    }
        
}
