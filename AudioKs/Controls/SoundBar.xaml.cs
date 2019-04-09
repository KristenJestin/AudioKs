using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AudioKs
{
    /// <summary>
    /// Logique d'interaction pour DisplayAudio.xaml
    /// </summary>
    public partial class SoundBar : UserControl
    {
        // Media
        public MediaPlayer mediaPlayer;
        private DispatcherTimer mediaTimer;
        public bool Playing { get; set; }

        private bool UserDraging { get; set; }

        public Audio CurrentAudio { get; set; }
        public Playlist CurrentPlaylist { get; set; }
        private double lastVolume;


        public string AudioName { get; set; }
        public string AudioArtist { get; set; }
        public BitmapImage Cover { get; set; }
        public BitmapImage PlayingImage { get; set; }


        public MainWindow Main = null;

        public SoundBar()
        {
            InitializeComponent();
            DataContext = this;

            // Media Player
            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaEnded += OnMediaEnd;
            mediaPlayer.MediaFailed += OnMediaFailed;
            mediaPlayer.MediaOpened += onMediaOpened;
            Playing = false;
            // Media Timer
            mediaTimer = new DispatcherTimer();
            mediaTimer.Interval = TimeSpan.FromMilliseconds(10);
            mediaTimer.Tick += Audio_Updater;
            mediaTimer.Start();
        }

        public void OnMainLoaded()
        {
            // Define default volume value
            VolumeSlider.Value = App.instance.settings.GetDouble("Volume", 0.5);
        }

        #region ----- Function -----

        // Audio
        public void LoadAudio(int index, bool play = false)
        {
            if (CurrentPlaylist.Audios.Count < index) return;

            PlayAudio(CurrentPlaylist.Audios[index]);
            if (!play)
                ChangeMediaState(false);
        }

        public void PlayAudio(Audio audio)
        {
            // Play in Media Player
            mediaPlayer.Open(new Uri(audio.Path));
            mediaPlayer.Play();
            // Set playing
            audio.IsPlaying = true;
            Playing = true;
            ChangeMediaState(Playing);
            // Unset Playing for last Audio
            if (CurrentAudio != null && CurrentAudio != audio)
                CurrentPlaylist.getAudio(CurrentAudio).IsPlaying = false;

            ProgressSlider.Maximum = audio.getDuration().TotalSeconds;
            CurrentAudio = audio;
            DataContext = CurrentAudio;
            Main.UpdateDataContext();

            // Check if is the same playlist
            ChangeRandom(CurrentPlaylist.Random);
            ChangeRepeat(CurrentPlaylist.Repeat);

            // Send to clients
            MainWindow.instance.Server.SendMessage("PLAYING=" + CurrentPlaylist.getIndexOfKey(audio));
        }

        public void ChangeAudio(bool next)
        {
            int index = -1;

            if (!CurrentPlaylist.Random)
            {
                int i = CurrentPlaylist.Audios.IndexOf(CurrentAudio);
                if (next)
                    index = (i + 1) % (CurrentPlaylist.Audios.Count);
                else
                    index = Main.Modulo(i -1, CurrentPlaylist.Audios.Count);
                    //index = (i - 1 == -1) ? CurrentPlaylist.Audios.Count - 1 : i - 1;
            }
            else
            {
                while(index == -1 || index == GetCurrentIndex())
                {
                    index = new Random().Next(CurrentPlaylist.Audios.Count);
                }
            }

            PlayAudio(CurrentPlaylist.Audios[index]);
        }

        public void ChangeMediaState(bool? statut = null)
        {
            if (statut != null)
            {
                if (statut == true)
                {
                    Main.PlayThumbButton.ImageSource = PlayImage.Source = FindResource("PauseImage") as BitmapImage;
                    mediaPlayer.Play();
                    Playing = true;
                    mediaPlayer.Volume = VolumeSlider.Value;
                }
                else
                {
                    Main.PlayThumbButton.ImageSource = PlayImage.Source = FindResource("PlayImage") as BitmapImage;
                    mediaPlayer.Pause();
                    Playing = false;
                }
            }
            else
            {
                if (Playing)
                {
                    Main.PlayThumbButton.ImageSource = PlayImage.Source = FindResource("PlayImage") as BitmapImage;
                    mediaPlayer.Pause();
                }
                else
                {
                    Main.PlayThumbButton.ImageSource = PlayImage.Source = FindResource("PauseImage") as BitmapImage;
                    mediaPlayer.Play();
                    mediaPlayer.Volume = VolumeSlider.Value;
                }
                Playing = !Playing;

                // Change for playlist page
                if (Main.getPlaylistPage().CurrentPlaylist == Main.SoundBar.CurrentPlaylist)
                    Main.getPlaylistPage().PlayImage.Source = Main.getPlaylistPage().FindResource(!Playing ? "PlayImage" : "PauseImage") as BitmapImage;
            }
        }


        // Playlist
        public bool LoadPlaylist(Playlist playlist, bool autoPlay = true)
        {
            // Check playlist
            if (playlist.Audios.Count == 0)
            {
                //MessageBox.Show("Empty playlist");
                return false;
            }

            // Change default current value
            CurrentPlaylist = playlist;
            if(CurrentAudio != null)
                CurrentAudio.IsPlaying = false;
            CurrentAudio = null;

            // Set random image
            ChangeRandom(CurrentPlaylist.Random);
            ChangeRepeat(CurrentPlaylist.Repeat);


            // Check if start playing when loaded
            if (CurrentPlaylist.Audios != null)
            {
                LoadAudio(0, autoPlay);
            }

            // Send to Server
            MainWindow.instance.Server.SendMessage("PLAYLIST=" + MainWindow.instance.Server.GetJson(playlist, playlist.getIndexOfKey(CurrentAudio)));

            return true;
        }

        private void FirstLoadPlaylist()
        {
            // Set random image
            ChangeRandom(CurrentPlaylist.Random);
            ChangeRepeat(CurrentPlaylist.Repeat);


            // Check if start playing when loaded
            if (CurrentPlaylist.Audios != null && CurrentPlaylist.Audios.Count > 0)
            {
                LoadAudio(0, false);
            }
        }

        public void ChangeRandom(bool random)
        {
            string normal = "RandomImage";
            string selected = "RandomSelectedImage";

            CurrentPlaylist.Random = random;
            RandomImage.Source = FindResource(random ? selected : normal) as BitmapImage;
        }

        public void ChangeRepeat(bool repeat)
        {
            string normal = "RepeatImage";
            string selected = "RepeatSelectedImage";

            CurrentPlaylist.Repeat = repeat;
            RepeatImage.Source = FindResource(repeat ? selected : normal) as BitmapImage;
        }

        // Other
        public void ChangeAudioPosition(double value)
        {
            double max = ProgressSlider.Maximum;
            double maxSec = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

            mediaPlayer.Position = TimeSpan.FromSeconds(value * maxSec / max);
        }

        private int GetCurrentIndex()
        {
            return CurrentPlaylist.Audios.IndexOf(CurrentAudio);
        }

        #endregion


        #region ----- Event -----

        // Click
        public void Play_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            ChangeMediaState();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null) return;
            
            mediaPlayer.Position = TimeSpan.FromSeconds(0);            
        }
        private void Previous_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            ChangeAudio(false);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            ChangeAudio(true);
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            ChangeRandom(!CurrentPlaylist.Random);
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            ChangeRepeat(!CurrentPlaylist.Repeat);
        }

        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeSlider.Value == 0)
            {
                VolumeSlider.Value = lastVolume == 0 ? 0.5 : lastVolume;
                lastVolume = 0;
            }
            else
            {
                lastVolume = VolumeSlider.Value;
                VolumeSlider.Value = 0;
            }
        }


        // Slider Volume
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            double value = VolumeSlider.Value;
            
            // Change the volume button image
            if (value == 0)
                VolumeImage.Source = FindResource("SpeakerXImage") as BitmapImage;
            else if (value < 33 * VolumeSlider.Maximum / 100)
                VolumeImage.Source = FindResource("Speaker1Image") as BitmapImage;
            else if (value < 66 * VolumeSlider.Maximum / 100)
                VolumeImage.Source = FindResource("Speaker2Image") as BitmapImage;
            else
                VolumeImage.Source = FindResource("Speaker3Image") as BitmapImage;


            if (mediaPlayer == null) return;
            App.instance.settings.Set("Volume", Math.Round(value, 2));

            if (!Playing || mediaPlayer.Source == null) return;
            mediaPlayer.Volume = value;
        }

        private void VolumePanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            VolumeSlider.Value += (e.Delta > 0) ? VolumeSlider.TickFrequency  : -VolumeSlider.TickFrequency;
        }

        // Slider Progress
        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer != null && mediaPlayer.Source != null)
            {
                if (mediaPlayer.NaturalDuration.HasTimeSpan && Mouse.LeftButton == MouseButtonState.Pressed && ProgressSlider.IsMouseOver && !UserDraging)
                {
                    ChangeAudioPosition(ProgressSlider.Value);

                    // Send clients
                    MainWindow.instance.Server.SendMessage("CH_TIME=" + (int)mediaPlayer.Position.TotalSeconds);
                }
            }
        }

        private void ProgressSlider_DragStarted(object sender, DragStartedEventArgs e) => UserDraging = true;

        private void ProgressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UserDraging = false;

            if (mediaPlayer.Source == null || !mediaPlayer.NaturalDuration.HasTimeSpan) return;

            ChangeAudioPosition(ProgressSlider.Value);
        }


        // Other
        private void OnMediaEnd(object sender, EventArgs e)
        {
            if (CurrentPlaylist.Repeat)
            {
                PlayAudio(CurrentAudio);
                ChangeRepeat(false);
                return;
            }

            ChangeAudio(true);
        }
        private void OnMediaFailed(object sender, EventArgs e)
        {
            MessageBox.Show("Une erreur c'est produite lors de la lecture de cette musique");
            ChangeAudio(true);
        }
        private void onMediaOpened(object sender, EventArgs e)
        {
            mediaPlayer.Volume = VolumeSlider.Value;
        }

        private void Audio_Updater(object sender, EventArgs e)
        {
            if (mediaPlayer != null && mediaPlayer.Source != null)
            {
                if (mediaPlayer.NaturalDuration.HasTimeSpan && !UserDraging)
                {
                    TimeSpan currentPos = mediaPlayer.Position;
                    TimeSpan maxPos = mediaPlayer.NaturalDuration.TimeSpan;
                    double sliderMax = ProgressSlider.Maximum;

                    double value = currentPos.TotalSeconds * sliderMax / maxPos.TotalSeconds;
                    ProgressSlider.Value = value;
                }
            }
            else
            {
                ProgressSlider.Value = 0;
            }
        }

        #endregion
    }
}