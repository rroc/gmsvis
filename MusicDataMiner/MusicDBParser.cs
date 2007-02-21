using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;

using CarlosAg.ExcelXmlWriter;
using musicbrainz;

namespace MusicDataminer
{
    class MusicDBParser
    {
        private Form1 iForm;
        private const string iDataPath              = "../../../data/";
        private const string iDataAlbumFileName     = iDataPath + "data_albums.bin";
        private const string iDataCountriesFileName = iDataPath + "data_countries.bin";
        private const string iCountriesInfoFileName = iDataPath + "geodata/countries_acronyms.txt";
        private const string iDBFileName            = iDataPath + "db.bin";

        //
        // Data Structures
        //

        [Serializable]
        public struct Country
        {
            public string acronym;
            public string name;
            public int gdbPerCapita;
            public string govType;
            public float unemploymentRate;
            public float medianAge;
        };

        [Serializable]
        public struct MusicBrainzAlbum
        {
            public Country country;
            public int date;
        };

        [Serializable]
        public struct Album
        {
            public string title;

            // probably store it as pointer :) ??
            public string artist;
            public string style;

            public List<MusicBrainzAlbum> releases;
        };


        [Serializable]
        public struct DB
        {
            public List<Album> albums;
            public Hashtable countries;
        };

        public struct ThreadData
        {
            public string style;
        };

        // private attributes
        private DB dataBase;
        private Hashtable iLineCount;

        //Constructor
        public MusicDBParser(Form1 aForm)
        {
            iForm = aForm;
            iLineCount = new Hashtable();

            // try to load DataBase
            bool loaded = LoadDB( iDBFileName, out this.dataBase );
            
            if ( ! loaded )
            {
                dataBase = new DB();
                dataBase.albums = new List<Album>();
                dataBase.countries = new Hashtable();
                this.ParseCountries( iCountriesInfoFileName );
            }

        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    SaveDB
        // FullName:  MusicDataminer.MusicDBParser.SaveDB
        // Access:    public 
        // Returns:   void
        //////////////////////////////////////////////////////////////////////////
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

        //////////////////////////////////////////////////////////////////////////
        // Method:    GetMusicBrainzReleases
        // FullName:  MusicDataminer.MusicDBParser.GetMusicBrainzReleases
        // Access:    public 
        // Returns:   bool
        // Parameter: string artist
        // Parameter: string albumName
        // Parameter: MusicBrainz o
        // Parameter: List<MusicBrainzAlbum> releasesList
        // Parameter: out string retrievedName
        //////////////////////////////////////////////////////////////////////////
        public bool GetMusicBrainzReleases(string artist, string albumName,
            List<MusicBrainzAlbum> releasesList, out string retrievedName, string aStyle)
        {
            MusicBrainz o;
            // Create the musicbrainz object, which will be needed for subsequent calls
            o = new MusicBrainz();
            // Set the proper server to use. Defaults to mm.musicbrainz.org:80
            if (Environment.GetEnvironmentVariable("MB_SERVER") != null)
                o.SetServer(Environment.GetEnvironmentVariable("MB_SERVER"), 80);
            // Check to see if the debug env var has been set 
            if (Environment.GetEnvironmentVariable("MB_DEBUG") != null)
                o.SetDebug(Environment.GetEnvironmentVariable("MB_DEBUG") != "0");
            // Tell the server to only return 4 levels of data, unless the MB_DEPTH env var is set
            if (Environment.GetEnvironmentVariable("MB_DEPTH") != null)
                o.SetDepth(int.Parse(Environment.GetEnvironmentVariable("MB_DEPTH")));
            else
                o.SetDepth(4);

            retrievedName = albumName;
            bool foundRelevantRelease = false;

            //Console.WriteLine("Searching for occurrences for: " + artist + " / " + albumName);
            iForm.PrintLine("(" + aStyle + ") LOOK: " + artist + " / " + albumName);
            bool ret = o.Query(MusicBrainz.MBQ_FileInfoLookup, new String[] { "", artist, albumName, "", "", "" });

            // Select the first album
            o.Select(MusicBrainz.MBS_SelectLookupResult, 1);

            string type;
            o.GetResultData(MusicBrainz.MBE_LookupGetType, out type);
            string fragment;
            o.GetFragmentFromURL(type, out fragment);

            // iterate through all the results
            o.Select(MusicBrainz.MBS_Rewind);

            if (!o.Select(MusicBrainz.MBS_SelectLookupResult, 1))
            {
                return foundRelevantRelease;
            }

            // NOTE: must be done before the next Select
            int relevance = o.GetResultInt(MusicBrainz.MBE_LookupGetRelevance);

            // if not sure about it, quit
            if (relevance < 80)
            {
                return foundRelevantRelease;
            }

            // select the album
            o.Select(MusicBrainz.MBS_SelectLookupResultAlbum);

            o.GetResultData(MusicBrainz.MBE_AlbumGetAlbumName, out retrievedName);

            int nReleases = o.GetResultInt(MusicBrainz.MBE_AlbumGetNumReleaseDates);

            if (nReleases != 0)
            {
                albumName = retrievedName;

                for (int i = 1; i <= nReleases; i++)
                {
                    if (o.Select(MusicBrainz.MBS_SelectReleaseDate, i))
                    {
                        foundRelevantRelease = true;

                        string country;
                        o.GetResultData(MusicBrainz.MBE_ReleaseGetCountry, out country);

                        string date;
                        o.GetResultData(MusicBrainz.MBE_ReleaseGetDate, out date);

                        // add it to the list
                        MusicBrainzAlbum release = new MusicBrainzAlbum();

                        if (dataBase.countries.ContainsKey(country.ToUpper()))
                        {
                            release.country = (Country)dataBase.countries[country];
                            release.date = int.Parse(date.Substring(0, 4));
                            releasesList.Add(release);
                        }

                    }

                    o.Select(MusicBrainz.MBS_Back);
                }
            }

            return foundRelevantRelease;
        }

        private static void WriteSomeStuff()
        {
            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets.Add("Sample");
            WorksheetRow row = sheet.Table.Rows.Add();

            // Header
            row.Cells.Add("Artist");
            row.Cells.Add("Album/Release");
            row.Cells.Add("Similar Artist");
            row.Cells.Add("Date");
            row.Cells.Add("Country");

            // Rows
            row = sheet.Table.Rows.Add();
            row.Cells.Add("Pink Floyd");
            row.Cells.Add("The Division Bell");
            row.Cells.Add("Someone :)");
            row.Cells.Add("2000-10-05");
            row.Cells.Add("SE");

            book.Save(@"../../../test.xls");
        }

        private void SaveData() 
        {
 
        }

        private string GetLogFilename( string aStyle )
        {
            return "../../../log_" + aStyle + ".txt";
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    ClearLog
        // FullName:  MusicDataminer.MusicDBParser.ClearLog
        // Access:    public 
        // Returns:   void
        // Parameter: string aStyle
        //////////////////////////////////////////////////////////////////////////
        public void ClearLog(string aStyle) 
        {
            string fileName = GetLogFilename(aStyle);

            // Delete the file if it exists.
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                iForm.PrintLine("(" + aStyle + ")\tErased a log file.");
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    SaveLog
        // FullName:  MusicDataminer.MusicDBParser.SaveLog
        // Access:    private 
        // Returns:   void
        // Parameter: int aLineNumber
        // Parameter: string aStyle
        //////////////////////////////////////////////////////////////////////////
        private void SaveLog(int aLineNumber, string aStyle)
        {

            string fileName = GetLogFilename(aStyle);

            // Delete the file if it exists.
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            // Create the file.
            FileStream fs = File.Create(fileName, 1024);

            // Add some information to the file.
            byte[] info = new System.Text.UTF8Encoding(true).GetBytes( ""+ aLineNumber );
            fs.Write(info, 0, info.Length);
            fs.Close();
        }

        private int ReadLog(string aStyle)
        {
            string fileName = GetLogFilename(aStyle);

            // Open the file and read it back.
            try
            {
                StreamReader sr = File.OpenText(fileName);
                string s = "";
                s = sr.ReadLine();
                sr.Close();

                iForm.PrintLine("(" + aStyle + ")\tContinuing from line number: " + s + ".");
                return int.Parse(s);
            }
            catch
            {
                iForm.PrintLine("(" + aStyle + ")\tStarting New Query Event.");
                return 0;
            }
        }



        public void ParseCountries(string filename)
        {
            // Open the file and read it back.
            StreamReader sr = File.OpenText(filename);
            string text = "";
            while ((text = sr.ReadLine()) != null)
            {
                char[] delimiterChars = { '\t' };

                string[] words = text.Split(delimiterChars);

                // Acronym	Country	GDB Per capita	Government Type	Unemployment Rate	Median Age
                if (words.Length > 5)
                {
                    Country country;

                    // Acronym
                    country.acronym = words[0].ToUpperInvariant();
                    // Country Name
                    country.name = words[1];
                    // GDB Per Capita
                    country.gdbPerCapita = HandleNAValuesInt(words[2]);
                    // Government Type
                    country.govType = words[3].Trim('"').ToLowerInvariant();
                    // Unemployment Rate
                    country.unemploymentRate = HandleNAValuesFloat(words[4]);
                    // Median Age
                    country.medianAge = HandleNAValuesFloat(words[5]);

                    if (!dataBase.countries.ContainsKey(country.acronym))
                    {
                        dataBase.countries.Add(country.acronym, country);
                    }
                }
            }
        }

        private float HandleNAValuesFloat(string s)
        {
            return s.ToUpperInvariant() == "NA" ? -1 : float.Parse(s);
        }

        private int HandleNAValuesInt(string s)
        {
            return s.ToUpperInvariant() == "NA" ? -1 : int.Parse(s);
        }

        public Thread AsyncParseByStyle(string style)
        {
            ThreadData td = new ThreadData() ;
            td.style = style;
//            td.query = queryObject;

            Thread thr = new Thread(new ParameterizedThreadStart(this.ThreadParse));
            thr.Start( td );
            return thr;
        }



        void ThreadParse(object data)
        {
            ThreadData td = (ThreadData)data;

            Thread thr = Thread.CurrentThread;
            try
            {
                ParseMusicStyle(td.style, thr);
            }
            catch (ThreadInterruptedException)
            {
                iForm.PrintLine("(" + td.style + ")\t!INTERRUPTED the queries ( at " + iLineCount[td.style] + " lines )");

                //update log

                SaveLog((int)this.iLineCount[td.style], td.style);
                MusicDBParser.SaveDB(iDBFileName, this.dataBase);
            }
            catch (ThreadAbortException)
            {
                iForm.PrintLine("(" + td.style + ")\t!ABORTED the queries ( at " + iLineCount[td.style] + " lines )");

                //update log

                SaveLog((int)this.iLineCount[td.style], td.style);
                MusicDBParser.SaveDB(iDBFileName, this.dataBase);

            }
        }

        public void ParseMusicStyle(string style, Thread aCurrentThread )
        {
            // Open the file and read it back.
            StreamReader sr = File.OpenText("../../../data/freedb/" + style + ".txt");
            string text = "";

            //Skip previously read
            int lineNumber = ReadLog(style);
            for (int i = 0; i < lineNumber; i++)
            {
                sr.ReadLine();
            }

            iLineCount[style] = lineNumber;
            //Start querying
            while ((text = sr.ReadLine()) != null )
            {
                Thread.Sleep(2);
                char[] delimiterChars = { '\t' };

                string[] tokens = text.Split(delimiterChars);

                Album album = new Album();

                // Lowercase all the tokens
                album.artist = tokens[0].ToLower();
                album.title = tokens[1].ToLower();

                if (tokens.Length < 3 || tokens[2].Trim().Length == 0)
                {
                    album.style = style;
                }
                else
                {
                    album.style = tokens[2].ToLower();
                }

                // search for info in the MusicBrainz DB
                List<MusicBrainzAlbum> releases = new List<MusicBrainzAlbum>();
                string retrievedName;
                bool foundSomething = GetMusicBrainzReleases(album.artist, album.title, releases, out retrievedName, style);

                if (foundSomething)
                {
                    album.title = retrievedName;
                    album.releases = releases;
                    dataBase.albums.Add(album);

                    //Console.WriteLine("Added Album: " + album.title + " Artist: " + album.artist + ": ");
                    iForm.PrintLine("(" + style + ")\t-> ADDED: " + album.title + " Artist: " + album.artist);
                    foreach (MusicBrainzAlbum release in album.releases)
                    {
                        //Console.WriteLine("\tCountry: " + release.country.name + "  Date: " + release.date);
                        iForm.PrintLine("\tCountry: " + release.country.name + "  Date: " + release.date);
                    }
                }
                lineNumber++;
                iLineCount[style] = lineNumber;
            }

            iForm.PrintLine("(" + style + ")\t!!! FINISHED !!! ( " + lineNumber + " lines )");
  

            //update log
            SaveLog(lineNumber, style);
            MusicDBParser.SaveDB( iDBFileName, this.dataBase );
        }

        public static bool SynchronizeDBs(string srcDBFilename1,
            string srcDBFilename2, string outputFilename)
        {
            DB destination;
            DB source;
            
            bool loaded = MusicDBParser.LoadDB(srcDBFilename1, out destination);
            if( loaded )
            {
                loaded = MusicDBParser.LoadDB(srcDBFilename2, out source);

                if (loaded)
                {
                    destination.albums.AddRange(source.albums);

                    SaveDB(outputFilename, destination);

                    return true;
                }
            }

            return false;
        }

    }
}