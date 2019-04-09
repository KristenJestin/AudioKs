using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AudioKs
{
    public class Playlist
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        [ManyToMany(typeof(PlaylistAsso), "PlaylistID", "AudioID", CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<Audio> Audios { get; set; }
        public bool Random { get; set; }
        public bool Repeat { get; set; }

        public void Load()
        {
            if (Audios == null)
                Audios = new ObservableCollection<Audio>();
            else
            {
                List<Audio> tempAudios = new List<Audio>();
                foreach(Audio audio in Audios)
                {
                    if (MainWindow.instance.IsAudioFile(audio.Path))
                        audio.BuildFromPath();
                    else
                        tempAudios.Add(audio);
                }

                foreach(Audio audio in tempAudios)
                {
                    Audios.Remove(audio);
                }
            }
        }


        #region ----- Function -----

        public BitmapImage GetBitmapImage()
        {
            if (!File.Exists(ImagePath)) return null;
            return new BitmapImage(new Uri(ImagePath));
        }

        public Playlist CreateFromJSON(string jsonPath)
        {
            Playlist temp = JsonConvert.DeserializeObject<Playlist>(File.ReadAllText(jsonPath));

            ID = temp.ID;
            Name = temp.Name;
            ImagePath = temp.ImagePath;
            Random = temp.Random;
            Repeat = temp.Repeat;

            Audios = new ObservableCollection<Audio>();
            foreach (Audio audio in temp.Audios)            
                Audios.Add(audio.BuildFromPath());            

            return this;
        }

        public Audio getAudio(Audio audio)
        {
            int index = Audios.IndexOf(audio);
            return Audios[index];
        }

        public bool CheckAudio(Audio audio)
        {
            if (audio == null || !MainWindow.instance.IsAudioFile(audio.Path) || Audios == null) return false;

            bool alreadyExist = Audios.ToList().FindIndex(oldAudio => oldAudio.Path == audio.Path) >= 0;
            if (alreadyExist) return false;

            return true;
        }

        public void AddAudio(string path, bool update = true)
        {
            Audio audio = null;
            try
            {
                audio = MainWindow.instance.SQLite.FindWithQuery<Audio>("select * from Audio where Path = ?", path);
            }
            catch {}
            if(audio == null)
                audio = new Audio { Path = path };
            audio.BuildFromPath();

            if (!CheckAudio(audio)) return;

            AddAudio(audio, update);
        }

        public void AddAudio(Audio audio, bool update = true)
        {
            // Try saving audio
            if (audio.ID == 0)
            {
                try
                {
                    MainWindow.instance.SQLite.Insert(audio);
                    audio.ID = MainWindow.instance.SQLite.FindWithQuery<Audio>("select ID from Audio where Path = ?", audio.Path).ID;
                }
                catch { }
            }

            Audios.Add(audio);

            if (ID != -1)
            {
                try
                {
                    MainWindow.instance.SQLite.UpdateWithChildren(this);
                   // MessageBox.Show(ToString());
                }
                catch(Exception e) {
                    MessageBox.Show(e.Message);
                }
            }

            if (update)
                MainWindow.instance.UpdateDataContext();
        }

        public int getIndexOfKey(Audio findAudio)
        {
            int index = -1;
            foreach (Audio audio in Audios)
            {
                Console.WriteLine("MUSIC: " + audio + "\nFIND: " + findAudio);
                index++;
                if (audio == findAudio)
                {
                    Console.WriteLine("INDEX: " + index);
                    return index;
                }

            }

            return -1;
        }

        #endregion


        public override string ToString()
        {
            return "ID: " + ID + ", Name: " + Name + ", Count: " + Audios.Count;
        }

    }


    public class Audio
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [Unique]
        public string Path { get; set; }
        [Ignore]
        public string Name { get; set; }
        [Ignore]
        public bool IsPlaying { get; set; }

        [Ignore]
        public string Title { get; set; }
        [Ignore]
        public BitmapImage Image { get; set; }
        [Ignore]
        public string Artist { get; set; }
        [Ignore]
        public string Album { get; set; }
        [Ignore]
        public string Duration { get; set; }
        

        #region ----- Function -----

        public TimeSpan getDuration()
        {
            DateTime dt;
            if (!DateTime.TryParseExact(Duration, "mm:ss", CultureInfo.InvariantCulture,
                                                          DateTimeStyles.None, out dt))
            {
                return TimeSpan.FromSeconds(0);
            }
            return dt.TimeOfDay;
        }

        public Audio BuildFromPath()
        {
            if (!MainWindow.instance.IsAudioFile(Path)) return null;

            string fileName = System.IO.Path.GetFileNameWithoutExtension(Path);
            TagLib.File file = TagLib.File.Create(Path);

            string title = !String.IsNullOrWhiteSpace(file.Tag.Title) ? file.Tag.Title : fileName;
            BitmapImage image = GetPicture(file);
            string artist = !String.IsNullOrWhiteSpace(file.Tag.FirstPerformer) ? file.Tag.FirstPerformer : "Unknown";
            string album = !String.IsNullOrWhiteSpace(file.Tag.Album) ? file.Tag.Album : "Unknown";
            TimeSpan time = file.Properties.Duration;

            Name = fileName;

            Title = title;
            Image = image;
            Artist = artist;
            Album = album;
            Duration = time.ToString(@"mm\:ss");

            return this;
        }

        private BitmapImage GetPicture(TagLib.File file)
        {
            // Check if file have image
            if (file.Tag.Pictures == null) return null;

            try
            {
                // Load you image data in MemoryStream
                TagLib.IPicture pic = file.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                return bitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Insert(bool error = false)
        {
            try
            {
                MainWindow.instance.SQLite.Insert(this);
            }
            catch
            {
                if (error)
                    MessageBox.Show("Audio can't be added");
            }
        }

        #endregion
    }

    public class PlaylistAsso
    {
        [ForeignKey(typeof(Playlist))]
        public int PlaylistID { get; set; }

        [ForeignKey(typeof(Audio))]
        public int AudioID { get; set; }
    }
}
