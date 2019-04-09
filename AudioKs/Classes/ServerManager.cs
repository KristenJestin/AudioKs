using Newtonsoft.Json;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioKs
{
    public class ServerManager : SimpleTcpServer
    {
        public bool IsEnabled { get; set; }

        public List<TcpClient> Clients { get; set; }


        public ServerManager(string password, int port)
        {
            Clients = new List<TcpClient>();

            try
            {
                Delimiter = 0x13;
                AutoTrimStrings = true;
                StringEncoder = Encoding.UTF8;

                Start(GetLocalIPAddress(), port);
                IsEnabled = true;
            }
            catch (Exception)
            {
                IsEnabled = false;
            }
        }



        public void SendMessage(string message)
        {
            Broadcast(message + "\n");
        }



        public string GetJson(Playlist playlist, int current)
        {
            List<AudioView> audios = new List<AudioView>();
            foreach (Audio audio in playlist.Audios)
            {

                audios.Add(CreateAudioView(audio));
            }

            PlaylistView pl = new PlaylistView();
            pl.NAME = playlist.Name;
            pl.RANDOM = playlist.Random;
            pl.CURRENT = current;
            pl.MUSIC = audios;

            return JsonConvert.SerializeObject(pl);
        }

        public PlaylistView GetPlView(Playlist playlist)
        {
            PlaylistView pl = new PlaylistView();
            pl.NAME = playlist.Name;
            pl.RANDOM = playlist.Random;
            pl.CURRENT = 0;
            pl.MUSIC = null;

            return pl;
        }

        public string GetJson(Audio audio)
        {
            return JsonConvert.SerializeObject(CreateAudioView(audio));
        }


        private AudioView CreateAudioView(Audio audio)
        {
            return new AudioView
            {
                TITLE = audio.Title,
                ARTIST = audio.Artist,
                DURATION = (int)audio.getDuration().TotalSeconds
            };
        }


        public IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return IPAddress.Parse(ip.ToString());
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        
    }

    public class PlaylistView
    {
        public string NAME { get; set; }
        public bool RANDOM { get; set; }
        public int CURRENT { get; set; }
        public List<AudioView> MUSIC { get; set; }
    }
    public class AudioView
    {
        public string TITLE { get; set; }
        public string ARTIST { get; set; }
        public int DURATION { get; set; }
    }


    public class Version
    {
        public float min { get; set; }
        public float max { get; set; }

        public Version(float max, float min = 0)
        {
            this.max = max;
            this.min = min;
        }


        public bool IsValid(float version)
        {
            return version >= min && version <= max;
        }
    }
}
