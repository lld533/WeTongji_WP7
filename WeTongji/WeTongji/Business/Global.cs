using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeTongji.Api.Domain;
using WeTongji.DataBase;
using System.IO;
using System.IO.IsolatedStorage;

namespace WeTongji.Business
{
    public class Global
    {
        #region [Instance]

        private static Global instance = null;

        public static Global Instance
        {
            get
            {
                if (instance == null)
                    instance = new Global();

                return instance;
            }
        }

        #endregion

        #region [Field]

        /// <summary>
        /// Lock this object when saving settings
        /// </summary>
        private Object objectToLock;

        #endregion

        #region [Properties]

        public WTSettings Settings { get; private set; }

        public String Session { get; private set; }

        #endregion

        #region [Constructor]

        private Global()
        {
            Settings = new WTSettings();
            objectToLock = new Object();
        }

        #endregion

        #region [Functions]

        public void LoadSettings()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            if (!store.FileExists(WTSettingsExt.SettingsFileName))
            {
                SaveSettings();
                return;
            }

            using (var fs = store.OpenFile(WTSettingsExt.SettingsFileName, FileMode.Open))
            {
                var sr = new StreamReader(fs);
                var str = sr.ReadToEnd();
                Settings = str.DeserializeSettings();

                fs.Close();
            }
        }

        public void SaveSettings()
        {
            lock (objectToLock)
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();

                using (var fs = store.OpenFile(WTSettingsExt.SettingsFileName, FileMode.OpenOrCreate))
                {
                    var str = Settings.GetSerializedString();
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(str);
                    sw.Flush();

                    fs.Close();
                }
            }
        }

        public void UpdateSettings(String uid, String pw, String session)
        {
            Settings.UID = uid;
            Settings.CryptPassword = pw.GetCryptPassword();
            Session = session;

            SaveSettings();
        }

        public void CleanSettings()
        {
            Settings.UID = String.Empty;
            Settings.CryptPassword = null;
            Session = String.Empty;

            SaveSettings();
        }

        #endregion

    }
}
