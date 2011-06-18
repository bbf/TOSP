using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using TOSP.Util;
using System.ComponentModel;
using Microsoft.Win32;

namespace TOSP
{
    public class TOSLConfig
    {
        protected static XmlSerializer configSerializer;
        protected const string CONFIG_NAME = "TOSLConfig.xml";

        public delegate void ConfigUpdatedDelegate();
        public static event PropertyChangedEventHandler PropertyChanged;

        private static TOSLConfig instance;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public static TOSLConfig GetConfig()
        {
            if (instance != null)
            {
                return instance;
            }

            if (configSerializer == null)
            {
                configSerializer = new XmlSerializer(typeof(TOSLConfig));
            }

            TOSLConfig config = (TOSLConfig)Serializers.DeserializeXML(CONFIG_NAME, configSerializer);
            if (config == null)
            {
                config = new TOSLConfig();
                config.ScanDefaults();
            }
            else
            {
                config.Validate();
            }

            instance = config;

            return config;
        }

        private void ScanDefaults()
        {
            if (TerrariaPath == null)
            {
                string terrariaPath = null;
                terrariaPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Re-Logic\Terraria", "Install_Path", null);
                if (terrariaPath == null)
                {
                    terrariaPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Re-Logic\Terraria", "Install_Path", null);
                }

                if (terrariaPath != null && Directory.Exists(terrariaPath))
                {
                    string terrariaServer = terrariaPath + Path.DirectorySeparatorChar + "TerrariaServer.exe";
                    if (File.Exists(terrariaServer))
                    {
                        TerrariaPath = terrariaServer;
                    }
                }

            }
        }

        private void Validate()
        {
            if (File.Exists(_TerrariaPath) == false)
            {
                this.TerrariaPath = null;
            }
        }



        // Properties:
        private string _TerrariaPath;
        public string TerrariaPath
        {
            get
            {
                return _TerrariaPath;
            }
            set
            {
                if (_TerrariaPath != value)
                {
                    _TerrariaPath = value;
                    NotifyPropertyChanged("TerrariaPath");
                }
            }
        }

        public string TerrariaPatchedServer
        {
            get
            {
                Validate();

                if (_TerrariaPath == null)
                {
                    return null;
                }

                string path = Path.GetDirectoryName(TOSLConfig.GetConfig().TerrariaPath);
                path += Path.DirectorySeparatorChar + "TerrrariaServerMod.exe";                
                return path;
            }
        }

        private TOSLConfig()
        {
        }

        public bool Save()
        {
            return Serializers.SerializeXML(CONFIG_NAME, configSerializer, this);
        }


    }
}
