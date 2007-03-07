using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;

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

        public struct ThreadData
        {
            public string style;
        };

        // private attributes
        private volatile DB dataBase;
        private Hashtable iLineCount;

        //Constructor
        public MusicDBParser(Form1 aForm)
        {
            iForm = aForm;
            iLineCount = new Hashtable();

            // try to load DataBase
            bool loaded = MusicDBLoader.LoadDB(iDBFileName, out dataBase);
            
            if ( ! loaded )
            {
                dataBase            = new DB();
                dataBase.albums     = new Hashtable();
                dataBase.countries  = new Hashtable();
                dataBase.styles     = new Hashtable();
                dataBase.artists    = new Hashtable();
                dataBase.dates      = new Hashtable();

                this.ParseCountries( iCountriesInfoFileName );
                MusicDBLoader.SaveDB(iDBFileName, this.dataBase);
            }

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
            List<MusicBrainzRelease> releasesList, out string retrievedName, string aStyle)
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
            bool found = o.Query(MusicBrainz.MBQ_FileInfoLookup, new String[] { "", artist, albumName, "", "", "" });

            if( ! found )
            {
                return found;
            }

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

                        if (dataBase.countries.ContainsKey(country.ToUpper()))
                        {
                            // add it to the list
                            MusicBrainzRelease release = new MusicBrainzRelease();

                            release.country = (Country)dataBase.countries[country];
                            release.date = int.Parse(date.Substring(0, 4));

                            // Add the release both to the Album and Country
                            releasesList.Add(release);
                            release.country.releases.Add(release);
                        }

                    }

                    o.Select(MusicBrainz.MBS_Back);
                }
            }
            return foundRelevantRelease;
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
                    Country country = new Country();

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
                    // Empty List of Releases
                    country.releases = new List<MusicBrainzRelease>();

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
                MusicDBLoader.SaveDB(iDBFileName, this.dataBase);
            }
            catch (ThreadAbortException)
            {
                iForm.PrintLine("(" + td.style + ")\t!ABORTED the queries ( at " + iLineCount[td.style] + " lines )");

                //update log

                SaveLog((int)this.iLineCount[td.style], td.style);
                MusicDBLoader.SaveDB(iDBFileName, this.dataBase);

            }
            catch(Exception e)
            {
                iForm.PrintLine(e.Message);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    ParseMusicStyle
        // FullName:  MusicDataminer.MusicDBParser.ParseMusicStyle
        // Access:    public 
        // Returns:   void
        // Parameter: string style
        // Parameter: Thread aCurrentThread
        //////////////////////////////////////////////////////////////////////////
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

            char[] delimiterChars = { '\t' };
            string[] tokens;
            string retrievedName;

            Style styleObj = GetStyle(style);

            //Start querying
            while ((text = sr.ReadLine()) != null )
            {
                Thread.Sleep(2);
                tokens = text.Split(delimiterChars);

                // Lowercase all the tokens
                string artistName = tokens[0].ToLower();
                string title = tokens[1].ToLower();

                List<MusicBrainzRelease> releases = new List<MusicBrainzRelease>();

                // search for info in the MusicBrainz DB
                bool foundSomething = GetMusicBrainzReleases(artistName, title, releases, out retrievedName, style);

                if (foundSomething)
                {
                    Album album = new Album();

                    if (tokens.Length < 3 || tokens[2].Trim().Length == 0)
                    {
                        album.style = styleObj;
                    }
                    else
                    {
                        album.style = GetStyle(tokens[2]);
                    }

                    album.title     = retrievedName;

                    if (AddAlbum(album, artistName))
                    {
                        // Add the album to all its releases
                        foreach (MusicBrainzRelease release in releases)
                        {
                            release.freeDBAlbum = album;
                            AddReleaseDate( release );
                        }

                        // set the Artist object
                        album.artist = GetArtist(artistName);

                        // add the releases to the Album
                        album.releases = releases;

                        // add the releases to their Style
                        album.style.releases.AddRange(releases);

                        // add the album to its artist
                        album.artist.albums.Add(album);

                        // Some output
                        iForm.PrintLine("(" + style + ")\t-> ADDED: " + album.title + " Artist: " + album.artist.name);
                        foreach (MusicBrainzRelease release in album.releases)
                        {
                            iForm.PrintLine("\tCountry: " + release.country.name + "  Date: " + release.date);
                        }
                    }
                }
                lineNumber++;
                iLineCount[style] = lineNumber;
            }

            iForm.PrintLine("(" + style + ")\t!!! FINISHED !!! ( " + lineNumber + " lines )");
  

            //update log
            SaveLog(lineNumber, style);
            MusicDBLoader.SaveDB(iDBFileName, this.dataBase);
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    ClearString
        // FullName:  MusicDataminer.MusicDBParser.ClearString
        // Access:    private 
        // Returns:   string
        // Parameter: string input
        //////////////////////////////////////////////////////////////////////////
        private string ClearString(string input)
        {
            input = input.Replace(" and ", "");
            Regex regex = new Regex("[^a-z0-9€]");

            return regex.Replace(input, "");
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    AddReleaseDate
        // FullName:  MusicDataminer.MusicDBParser.AddReleaseDate
        // Access:    private 
        // Returns:   void
        // Parameter: MusicBrainzRelease release
        //////////////////////////////////////////////////////////////////////////
        private void AddReleaseDate(MusicBrainzRelease release)
        {
            int key = release.date;
            List<MusicBrainzRelease> dates;

            if ( ! this.dataBase.dates.ContainsKey(key) )
            {
                dates = new List<MusicBrainzRelease>();
                this.dataBase.dates[key] = dates;
            }
            else
            {
                dates = (List<MusicBrainzRelease>)this.dataBase.dates[key];
            }

            dates.Add(release);
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    AddAlbum
        // FullName:  MusicDataminer.MusicDBParser.AddAlbum
        // Access:    private 
        // Returns:   bool
        // Parameter: Album album
        //////////////////////////////////////////////////////////////////////////
        private bool AddAlbum(Album album, string artistName)
        {
            string key = artistName + "€" + album.title;
            key = key.ToLowerInvariant();

            key = ClearString(key);

            if (!this.dataBase.albums.ContainsKey(key))
            {
                this.dataBase.albums[key] = album;
                return true;
            }

            return false;
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    GetArtist
        // FullName:  MusicDataminer.MusicDBParser.GetArtist
        // Access:    private 
        // Returns:   MusicDataminer.Artist
        // Parameter: string name
        //////////////////////////////////////////////////////////////////////////
        private Artist GetArtist(string name)
        {
            name = name.ToLowerInvariant();
            string key = ClearString(name);

            if (this.dataBase.artists.ContainsKey(key))
            {
                return (Artist)this.dataBase.artists[key];
            }
            else
            {
                Artist artist   = new Artist();
                artist.name     = name;
                artist.albums   = new List<Album>();
                
                this.dataBase.artists[key] = artist;

                return artist;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Method:    GetStyle
        // FullName:  MusicDataminer.MusicDBParser.GetStyle
        // Access:    private 
        // Returns:   MusicDataminer.Style
        // Parameter: string style
        //////////////////////////////////////////////////////////////////////////
        private Style GetStyle(string style)
        {
            string key = style.ToLowerInvariant();

            key = ClearString(key);

            if( this.dataBase.styles.ContainsKey( key ) )
            {
                return (Style)this.dataBase.styles[key];
            }
            else
            {
                Style styleObj = new Style();
                styleObj.name = style.ToLowerInvariant();
                styleObj.releases = new List<MusicBrainzRelease>();
                this.dataBase.styles.Add(key, styleObj);

                return styleObj;
            }
        }

        public static bool SynchronizeDBs(string srcDBFilename1,
            string srcDBFilename2, string outputFilename)
        {
            DB destination;
            DB source;

            bool loaded = MusicDBLoader.LoadDB(srcDBFilename1, out destination);
            if( loaded )
            {
                loaded = MusicDBLoader.LoadDB(srcDBFilename2, out source);

                if (loaded)
                {
                    // TODO: recode this
                    //destination.albums.AddRange(source.albums);

                    MusicDBLoader.SaveDB(outputFilename, destination);

                    return true;
                }
            }

            return false;
        }

    }
}