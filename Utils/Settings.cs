﻿using DC_SB.Utils.Players;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace DC_SB.Utils
{
    public class Settings : INotifyPropertyChanged
    {
        public const int WMP = 0;
        public const int NAUDIO = 1;

        public IPlayer Player { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Bindable properties
        private ObservableCollection<Counter> counters;
        public ObservableCollection<Counter> Counters
        {
            get { return counters; }
            private set
            {
                counters = value;
                OnPropertyChanged("Counters");
            }
        }
        private ObservableCollection<Sound> sounds;
        public ObservableCollection<Sound> Sounds
        {
            get { return sounds; }
            private set
            {
                sounds = value;
                OnPropertyChanged("Sounds");
            }
        }
        private List<KeyPrompt> keyBindingsCounters;
        public List<KeyPrompt> KeyBindingsCounters
        {
            get { return keyBindingsCounters; }
            set
            {
                keyBindingsCounters = value;
                OnPropertyChanged("KeyBindingsCounters");
            }
        }
        private List<KeyPrompt> keyBindingsSounds;
        public List<KeyPrompt> KeyBindingsSounds
        {
            get { return keyBindingsSounds; }
            set
            {
                keyBindingsSounds = value;
                OnPropertyChanged("KeyBindingsSounds");
            }
        }
        private int playerLib { get; set; }
        public int PlayerLib
        {
            get { return playerLib; }
            set
            {
                playerLib = value;
                if (value == WMP)
                {
                    Player = new WMPlayer();
                }
                else
                {
                    Player = new NAudioPlayer();
                    Player.SetDevice(Device);
                }
                Player.Volume = Volume;
                OnPropertyChanged("PlayerLib");
            }
        }
        private bool wMPLibrary { get; set; }
        public bool WMPLibrary
        {
            get { return wMPLibrary; }
            set
            {
                wMPLibrary = value;
                OnPropertyChanged("WMPLibrary");
            }
        }
        private int device;
        public int Device
        {
            get { return device; }
            set
            {
                device = value;
                Player.SetDevice(value);
                OnPropertyChanged("Device");
            }
        }
        private bool disableCounters { get; set; }
        public bool DisableCounters
        {
            get { return disableCounters; }
            set
            {
                disableCounters = value;
                OnPropertyChanged("DisableCounters");
            }
        }
        private bool disableSounds { get; set; }
        public bool DisableSounds
        {
            get { return disableSounds; }
            set
            {
                disableSounds = value;
                OnPropertyChanged("DisableSounds");
            }
        }
        private List<OutputDevice> devicesList;
        public List<OutputDevice> DevicesList
        {
            get { return devicesList; }
            set
            {
                devicesList = value;
                OnPropertyChanged("DevicesList");
            }
        }
        private double windowHeight = 200;
        public double WindowHeight
        {
            get { return windowHeight; }
            set
            {
                windowHeight = value;
                OnPropertyChanged("WindowHeight");
            }
        }
        private double windowWidth = 216;
        public double WindowWidth
        {
            get { return windowWidth; }
            set
            {
                windowWidth = value;
                OnPropertyChanged("WindowWidth");
            }
        }
        private double splitterPosition;
        public double SplitterPosition
        {
            get { return splitterPosition; }
            set
            {
                splitterPosition = value;
                FirstColumnWidth = new GridLength(value, GridUnitType.Star);
                SecondColumnWidth = new GridLength(WindowWidth - 21 - value, GridUnitType.Star);
                OnPropertyChanged("FirstColumnWidth");
                OnPropertyChanged("SecondColumnWidth");
                OnPropertyChanged("SplitterPosition");
            }
        }
        public GridLength firstColumnWidth = new GridLength();
        public GridLength FirstColumnWidth
        {
            get { return firstColumnWidth; }
            set
            {
                firstColumnWidth = value;
                splitterPosition = value.Value;
                OnPropertyChanged("FirstColumnWidth");
                OnPropertyChanged("SplitterPosition");
            }
        }
        public GridLength SecondColumnWidth { get; set; } = new GridLength();
        private Counter selectedCounter;
        public Counter SelectedCounter
        {
            get { return selectedCounter; }
            set
            {
                selectedCounter = value;
                OnPropertyChanged("SelectedCounter");
            }
        }
        private Sound selectedSound;
        public Sound SelectedSound
        {
            get { return selectedSound; }
            set
            {
                selectedSound = value;
                OnPropertyChanged("SelectedSound");
            }
        }
        private int volume;
        public int Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                Player.Volume = value;
                OnPropertyChanged("Volume");
                Save();
            }
        }
        private bool initialized;
        public bool Initialized
        {
            get { return initialized; }
            set
            {
                initialized = value;
                OnPropertyChanged("Initialized");
            }
        }
        #endregion

        #region Constructors
        public Settings()
        {
            LoadSettings();
            LoadItems();

            if (IniFile.FilePath == IniFile.OLD_CONFIG_PATH) IniFile.FilePath = IniFile.DEFAULT_CONFIG_PATH;
            Initialized = true;
        }

        public Settings(Settings settings)
        {
            Counters = settings.Counters;
            Sounds = settings.Sounds;
            volume = settings.Volume;
            device = settings.Device;
            playerLib = settings.PlayerLib;
            Player = settings.Player;
            WMPLibrary = settings.WMPLibrary;
            DevicesList = OutputDevice.GetDevices();
            DisableCounters = settings.DisableCounters;
            DisableSounds = settings.DisableSounds;
            WindowHeight = settings.WindowHeight;
            WindowWidth = settings.WindowWidth;
            SplitterPosition = settings.SplitterPosition;

            keyBindingsCounters = new List<KeyPrompt>();
            foreach (KeyPrompt keyPrompt in settings.keyBindingsCounters)
            {
                keyBindingsCounters.Add(new KeyPrompt(keyPrompt.Name, keyPrompt.Keys));
            }

            keyBindingsSounds = new List<KeyPrompt>();
            foreach (KeyPrompt keyPrompt in settings.keyBindingsSounds)
            {
                keyBindingsSounds.Add(new KeyPrompt(keyPrompt.Name, keyPrompt.Keys));
            }
            Initialized = true;
        }
        #endregion

        #region Loading
        private void LoadSettings()
        {
            string tmp;
            Sounds = new ObservableCollection<Sound>();
            Counters = new ObservableCollection<Counter>();

            tmp = IniFile.IniReadValue("Size", "form");
            if (tmp != null)
            {
                try
                {
                    string[] sizeStrings = tmp.Split('\t');
                    WindowWidth = double.Parse(sizeStrings[0]);
                    WindowHeight = double.Parse(sizeStrings[1]);
                }
                catch
                {
                    WindowWidth = 500;
                    WindowHeight = 350;
                }
            }

            tmp = IniFile.IniReadValue("Size", "split");
            if (tmp != null) SplitterPosition = double.Parse(tmp);

            try
            {
                var player = new WMPLib.WindowsMediaPlayer();
                WMPLibrary = true;
            }
            catch
            {
                WMPLibrary = false;
            }

            DevicesList = OutputDevice.GetDevices();

            tmp = IniFile.IniReadValue("Sounds", "device");
            device = OutputDevice.GetDeviceNumber(tmp);

            tmp = IniFile.IniReadValue("Sounds", "volume");
            try { volume = int.Parse(tmp); }
            catch { volume = 30; }

            tmp = IniFile.IniReadValue("Sounds", "player");
            if (WMPLibrary && tmp == WMP.ToString()) PlayerLib = WMP;
            else PlayerLib = NAUDIO;

            tmp = IniFile.IniReadValue("Settings", "counters_disable");
            try { DisableCounters = bool.Parse(tmp); }
            catch { }

            tmp = IniFile.IniReadValue("Settings", "sounds_disable");
            try { DisableSounds = bool.Parse(tmp); }
            catch { }

            keyBindingsCounters = new List<KeyPrompt>();
            keyBindingsSounds = new List<KeyPrompt>();

            keyBindingsCounters.Add(new KeyPrompt("Next", IniFile.IniReadValue("Settings", "Next")));
            keyBindingsCounters.Add(new KeyPrompt("Previous", IniFile.IniReadValue("Settings", "Previous")));
            keyBindingsCounters.Add(new KeyPrompt("Up", IniFile.IniReadValue("Settings", "Up")));
            keyBindingsCounters.Add(new KeyPrompt("Down", IniFile.IniReadValue("Settings", "Down")));
            keyBindingsCounters.Add(new KeyPrompt("Reset", IniFile.IniReadValue("Settings", "Reset")));
            keyBindingsSounds.Add(new KeyPrompt("Pause", IniFile.IniReadValue("Settings", "Pause")));
            keyBindingsSounds.Add(new KeyPrompt("Continue", IniFile.IniReadValue("Settings", "Continue")));
        }

        private void LoadItems()
        {
            int index = 1;
            string tmp = IniFile.IniReadValue("Counters", "Log" + index);
            while (tmp != null && tmp.Trim() != "")
            {
                var splitted = tmp.Split('\t');
                var name = splitted[0];
                string filePath = splitted[1];
                Counters.Add(new Counter(name, filePath));
                index++;
                tmp = IniFile.IniReadValue("Counters", "Log" + index);
            }

            index = 1;
            tmp = IniFile.IniReadValue("Sounds", "Log" + index);
            while (tmp != null && tmp.Trim() != "")
            {
                var splitted = tmp.Split('\t');

                var name = splitted[0];

                var filePaths = new List<string>();
                var files = splitted[1].Split(new string[] { " |" }, StringSplitOptions.RemoveEmptyEntries);

                string dirName = Path.GetDirectoryName(files[0]);
                filePaths.Add(files[0]);               
                for (int i = 1; i < files.Length; i++)
                {
                    filePaths.Add(Path.Combine(dirName, files[i]));
                }

                var keys = new ObservableCollection<Input.VKeys>();
                var keyNames = splitted[2].Replace("Choose another file | ", "").Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string key in keyNames)
                {
                    Input.VKeys vKey;
                    if (Enum.TryParse(key, out vKey)) keys.Add(vKey);
                }

                Sounds.Add(new Sound(name, filePaths, keys));
                index++;
                tmp = IniFile.IniReadValue("Sounds", "Log" + index);
            }
        }
        #endregion

        #region Saving
        public void Save()
        {
            if (Initialized)
            {
                IniFile.IniWriteValue("Size", "form", string.Format("{0}\t{1}", WindowWidth, WindowHeight));
                IniFile.IniWriteValue("Size", "split", SplitterPosition.ToString());
                IniFile.IniWriteValue("Sounds", "device", DevicesList[Device].Name);
                IniFile.IniWriteValue("Sounds", "player", PlayerLib.ToString());
                IniFile.IniWriteValue("Sounds", "volume", Volume.ToString());
                IniFile.IniWriteValue("Settings", "counters_disable", DisableCounters.ToString());
                IniFile.IniWriteValue("Settings", "sounds_disable", DisableSounds.ToString());

                foreach (KeyPrompt keyPrompt in keyBindingsCounters)
                {
                    IniFile.IniWriteValue("Settings", keyPrompt.Name, (string)new VKeysToString().Convert(keyPrompt.Keys, null, null, null));
                }

                foreach (KeyPrompt keyPrompt in keyBindingsSounds)
                {
                    IniFile.IniWriteValue("Settings", keyPrompt.Name, (string)new VKeysToString().Convert(keyPrompt.Keys, null, null, null));
                }

                int index = 1;
                string tmp = IniFile.IniReadValue("Counters", "Log" + index);
                while (tmp != null)
                {
                    IniFile.IniWriteValue("Counters", "Log" + index, "");
                    index++;
                    tmp = IniFile.IniReadValue("Counters", "Log" + index);
                }

                index = 1;
                tmp = IniFile.IniReadValue("Sounds", "Log" + index);
                while (tmp != null)
                {
                    IniFile.IniWriteValue("Sounds", "Log" + index, "");
                    index++;
                    tmp = IniFile.IniReadValue("Sounds", "Log" + index);
                }

                for (int i = 0; i < Counters.Count; i++)
                {
                    var counter = Counters[i];
                    string filePath = counter.FilePath;
                    if (IniFile.Portable) filePath = counter.FileName;
                    IniFile.IniWriteValue("Counters", "Log" + (i + 1), string.Format("{0}\t{1}", counter.Name, filePath));
                }

                for (int i = 0; i < Sounds.Count; i++)
                {
                    var sound = Sounds[i];

                    int startIndex;
                    string filePaths;
                    if (IniFile.Portable)
                    {
                        filePaths = "";
                        startIndex = 0;
                    }
                    else
                    {
                        filePaths = sound.FilePaths[0];
                        startIndex = 1;
                    }

                    for (int j = startIndex; j < sound.FilePaths.Count; j++)
                    {
                        filePaths += " |" + Path.GetFileName(sound.FilePaths[j]);
                    }

                    string keys = sound.Keys[0].ToString();
                    for (int j = 1; j < sound.Keys.Count; j++)
                    {
                        keys += " + " + sound.Keys[j];
                    }

                    IniFile.IniWriteValue("Sounds", "Log" + (i + 1), string.Format("{0}\t{1}\t{2}", sound.Name, filePaths, keys));
                }
            }
        }
        #endregion

        #region Override
        public static bool operator ==(Settings x, Settings y)
        {
            if (ReferenceEquals(null, x) || ReferenceEquals(null, y))
            {
                return !(ReferenceEquals(null, x) ^ ReferenceEquals(null, y));
            }
            if (x.PlayerLib != y.PlayerLib) return false;
            if (x.Device != y.Device) return false;
            if (x.DisableCounters != y.DisableCounters) return false;
            if (x.DisableSounds != y.DisableSounds) return false;
            for (int i = 0; i < x.keyBindingsCounters.Count; i++)
            {
                if (x.keyBindingsCounters[i].Keys.Count != y.keyBindingsCounters[i].Keys.Count) return false;
                for (int j = 0; j < x.keyBindingsCounters[i].Keys.Count; j++)
                {
                    if (x.keyBindingsCounters[i].Keys[j] != y.keyBindingsCounters[i].Keys[j]) return false;
                }
            }
            for (int i = 0; i < x.keyBindingsSounds.Count; i++)
            {
                if (x.keyBindingsSounds[i].Keys.Count != y.keyBindingsSounds[i].Keys.Count) return false;
                for (int j = 0; j < x.keyBindingsSounds[i].Keys.Count; j++)
                {
                    if (x.keyBindingsSounds[i].Keys[j] != y.keyBindingsSounds[i].Keys[j]) return false;
                }
            }
            return true;
        }

        public static bool operator !=(Settings x, Settings y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            Settings item = obj as Settings;
            if (item == null) return false;
            return this == item;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    public class KeyPrompt : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value == name) return;
                name = value;
                OnPropertyChanged("Name");
            }
        }
        private ObservableCollection<Input.VKeys> keys;
        public ObservableCollection<Input.VKeys> Keys
        {
            get { return keys; }
            set
            {
                if (value == keys) return;
                keys = value;
                OnPropertyChanged("Keys");
            }
        }

        public KeyPrompt(string name)
        {
            this.name = name;
            this.keys = new ObservableCollection<Input.VKeys>();
        }

        public KeyPrompt(string name, string value)
        {
            this.name = name;
            this.keys = new ObservableCollection<Input.VKeys>();
            Load(value);
        }

        public KeyPrompt(string name, ObservableCollection<Input.VKeys> keys)
        {
            this.name = name;
            this.keys = keys;
        }

        public void Load(string value)
        {
            try
            {
                foreach (string keyName in value.Split('+'))
                {
                    string trimmedKeyName = keyName.Trim();
                    keys.Add((Input.VKeys)Enum.Parse(typeof(Input.VKeys), trimmedKeyName));
                }
            }
            catch
            {
                keys.Clear();
                switch (Name)
                {
                    case "Next":
                        keys.Add(Input.VKeys.MULTIPLY);
                        break;
                    case "Up":
                        keys.Add(Input.VKeys.ADD);
                        break;
                    case "Down":
                        keys.Add(Input.VKeys.SUBTRACT);
                        break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OutputDevice
    {
        public string Name { get; private set; }

        public OutputDevice(string name)
        {
            this.Name = name.Split(new string[] { " (" }, StringSplitOptions.None)[0];
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<OutputDevice> GetDevices()
        {
            var devicesList = new List<OutputDevice>();
            for (int i = 0; i < NAudio.Wave.WaveOut.DeviceCount; i++)
            {
                devicesList.Add(new OutputDevice(NAudio.Wave.WaveOut.GetCapabilities(i).ProductName));
            }
            return devicesList;
        }

        public static int GetDeviceNumber(string name)
        {
            if (name == null || name == "") return 0;
            var devicesList = GetDevices();
            for (int i = 0; i < devicesList.Count; i++)
            {
                if (devicesList[i].Name == name)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
