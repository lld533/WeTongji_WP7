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

        /// <summary>
        /// The UID of the user signed in. If no user signed in, this
        /// value equals null.
        /// </summary>
        public String CurrentUserID { get; set; }

        public int PersonPageId { get; set; }

        public int ActivityPageId { get; set; }

        public int FavoritePageId { get; set; }

        public int OfficialNotePageId { get; set; }

        public int ClubNewsPageId { get; set; }

        public int TongjiNewsPageId { get; set; }

        public int AroundNewsPageId { get; set; }

        public List<CalendarGroup<CalendarNode>> AgendaSource
        {
            get;
            private set;
        }

        public SourceState CurrentAgendaSourceState { get; set; }
        public SourceState CurrentPeopleOfWeekSourceState { get; set; }
        public SourceState CurrentActivitySourceState { get; set; }

        #endregion

        #region [Event handlers]

        public event EventHandler AgendaSourceStateChanged;

        #endregion

        #region [Constructor]

        public Global()
        {
            Settings = new WTSettings();
            objectToLock = new Object();

            PersonPageId = -1;
            ActivityPageId = -1;
            FavoritePageId = -1;
            OfficialNotePageId = -1;
            ClubNewsPageId = -1;
            TongjiNewsPageId = -1;
            AroundNewsPageId = -1;
        }

        #endregion

        #region [Functions]

        #region [public]

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

        public void CleanAgendaSource()
        {
            if (AgendaSource != null)
            {
                AgendaSource.Clear();
            }

            CurrentAgendaSourceState = SourceState.NotSet;
            OnAgendaSourceStateChanged();
        }

        public void SetAgendaSource(List<CalendarGroup<CalendarNode>> src)
        {
            if (src == null)
                throw new ArgumentNullException("src");

            AgendaSource = src;

            var previousState = CurrentAgendaSourceState;

            CurrentAgendaSourceState = SourceState.Done;
            OnAgendaSourceStateChanged();
        }

        public void StartSettingAgendaSource()
        {
            CurrentAgendaSourceState = SourceState.Setting;
            OnAgendaSourceStateChanged();
        }

        #endregion

        #region [Private]
        private void OnAgendaSourceStateChanged()
        {
            var handler = AgendaSourceStateChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion

        #endregion
    }

    public enum SourceState
    {
        NotSet,
        Setting,
        Done
    }
}
