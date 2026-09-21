using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Gallop
{
    /// <summary>
    /// UmaViewer adapter for the official MasterJukeboxMusicData table.
    /// The public API and row fields follow the official dummy class; rows come
    /// from UmaDatabaseController.QueryMaster (Mono.Data.Sqlite on desktop,
    /// Sqlite3MC on WebGL).
    /// </summary>
    public sealed class MasterJukeboxMusicData
    {
        public const string TABLE_NAME = "jukebox_music_data";

        private readonly Func<string, List<DataRow>> _query;
        private bool _preloaded;
        private readonly HashSet<int> _notFounds = new HashSet<int>();
        private readonly Dictionary<int, JukeboxMusicData> _lazyPrimaryKeyDictionary =
            new Dictionary<int, JukeboxMusicData>();
        private readonly Dictionary<int, List<JukeboxMusicData>> _dictionaryWithVersionType =
            new Dictionary<int, List<JukeboxMusicData>>();

        public Dictionary<int, JukeboxMusicData> dictionary
        {
            get
            {
                ForcePreloadAllEntries();
                return _lazyPrimaryKeyDictionary;
            }
        }

        public MasterJukeboxMusicData(Func<string, List<DataRow>> query)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
        }

        public JukeboxMusicData Get(int musicId)
        {
            if (musicId == JukeboxUtil.NONE_MUSIC_ID)
                return null;

            if (_lazyPrimaryKeyDictionary.TryGetValue(musicId, out JukeboxMusicData cached))
                return cached;

            if (_notFounds.Contains(musicId))
                return null;

            JukeboxMusicData selected = SelectOne(musicId);
            if (selected == null)
            {
                _notFounds.Add(musicId);
                return null;
            }

            _lazyPrimaryKeyDictionary[musicId] = selected;
            return selected;
        }

        public JukeboxMusicData GetWithVersionType(int versionType)
        {
            List<JukeboxMusicData> list = GetListWithVersionType(versionType);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public List<JukeboxMusicData> GetListWithVersionType(int versionType)
        {
            if (_dictionaryWithVersionType.TryGetValue(versionType, out List<JukeboxMusicData> cached))
                return cached;

            List<JukeboxMusicData> list = ListSelectWithVersionType(versionType);
            _dictionaryWithVersionType[versionType] = list;
            return list;
        }

        public List<JukeboxMusicData> MaybeListWithVersionType(int versionType)
        {
            List<JukeboxMusicData> list = GetListWithVersionType(versionType);
            return list != null && list.Count > 0 ? list : null;
        }

        /// <summary>
        /// Viewer convenience method. The official dialog requests several
        /// VersionType lists and combines them. Until that exact dialog filter
        /// is fully reconstructed, this returns the union of every table row.
        /// </summary>
        public List<JukeboxMusicData> GetAll()
        {
            ForcePreloadAllEntries();
            return _lazyPrimaryKeyDictionary.Values
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.MusicId)
                .ToList();
        }

        public void Unload()
        {
            _preloaded = false;
            _notFounds.Clear();
            _lazyPrimaryKeyDictionary.Clear();
            _dictionaryWithVersionType.Clear();
        }

        private JukeboxMusicData SelectOne(int musicId)
        {
            var rows = _query($"SELECT * FROM jukebox_music_data WHERE music_id = {musicId} LIMIT 1");
            return rows.Count > 0 ? CreateOrmByQueryResult(rows[0]) : null;
        }

        private List<JukeboxMusicData> ListSelectWithVersionType(int versionType)
        {
            var list = new List<JukeboxMusicData>();
            foreach (var row in _query($"SELECT * FROM jukebox_music_data WHERE version_type = {versionType} ORDER BY sort, music_id"))
            {
                JukeboxMusicData item = CreateOrmByQueryResult(row);
                if (_lazyPrimaryKeyDictionary.TryGetValue(item.MusicId, out JukeboxMusicData cached))
                    item = cached;
                else
                    _lazyPrimaryKeyDictionary.Add(item.MusicId, item);

                list.Add(item);
            }

            return list;
        }

        private void ForcePreloadAllEntries()
        {
            if (_preloaded)
                return;

            _preloaded = true;
            foreach (var row in _query("SELECT * FROM jukebox_music_data ORDER BY sort, music_id"))
            {
                JukeboxMusicData item = CreateOrmByQueryResult(row);
                if (!_lazyPrimaryKeyDictionary.ContainsKey(item.MusicId))
                    _lazyPrimaryKeyDictionary.Add(item.MusicId, item);
            }
        }

        private static JukeboxMusicData CreateOrmByQueryResult(DataRow row)
        {
            return new JukeboxMusicData(
                musicId: ReadInt(row, "music_id"),
                sort: ReadInt(row, "sort"),
                conditionType: ReadInt(row, "condition_type"),
                isHidden: ReadInt(row, "is_hidden"),
                versionType: ReadInt(row, "version_type"),
                requestType: ReadInt(row, "request_type"),
                lampColor: ReadInt(row, "lamp_color"),
                lampAnimation: ReadInt(row, "lamp_animation"),
                nameTextureLength: ReadInt(row, "name_texture_length"),
                songType: (byte)ReadInt(row, "song_type"),
                bgmCueNameShort: ReadString(row, "bgm_cue_name_short"),
                bgmCuesheetNameShort: ReadString(row, "bgm_cuesheet_name_short"),
                bgmCueNameGamesize: ReadString(row, "bgm_cue_name_gamesize"),
                bgmCuesheetNameGamesize: ReadString(row, "bgm_cuesheet_name_gamesize"),
                shortLength: ReadInt(row, "short_length"),
                alterJacket: ReadInt(row, "alter_jacket"),
                startDate: ReadLong(row, "start_date"),
                endDate: ReadLong(row, "end_date"));
        }

        private static object ReadValue(DataRow row, string name)
        {
            foreach (DataColumn col in row.Table.Columns)
            {
                if (string.Equals(col.ColumnName, name, StringComparison.OrdinalIgnoreCase))
                {
                    object v = row[col];
                    return v == DBNull.Value ? null : v;
                }
            }
            return null;
        }

        private static int ReadInt(DataRow row, string name, int fallback = 0)
        {
            object v = ReadValue(row, name);
            if (v == null) return fallback;
            try { return Convert.ToInt32(v, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private static long ReadLong(DataRow row, string name, long fallback = 0L)
        {
            object v = ReadValue(row, name);
            if (v == null) return fallback;
            try { return Convert.ToInt64(v, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private static string ReadString(DataRow row, string name)
        {
            object v = ReadValue(row, name);
            if (v == null) return string.Empty;
            return Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public sealed class JukeboxMusicData
        {
            public readonly int MusicId;
            public readonly int Sort;
            public readonly int ConditionType;
            public readonly int IsHidden;
            public readonly int VersionType;
            public readonly int RequestType;
            public readonly int LampColor;
            public readonly int LampAnimation;
            public readonly int NameTextureLength;
            public readonly byte SongType;
            public readonly string BgmCueNameShort;
            public readonly string BgmCuesheetNameShort;
            public readonly string BgmCueNameGamesize;
            public readonly string BgmCuesheetNameGamesize;
            public readonly int ShortLength;
            public readonly int AlterJacket;
            public readonly long StartDate;
            public readonly long EndDate;

            public JukeboxMusicData(
                int musicId = 0,
                int sort = 0,
                int conditionType = 0,
                int isHidden = 0,
                int versionType = 0,
                int requestType = 0,
                int lampColor = 0,
                int lampAnimation = 0,
                int nameTextureLength = 0,
                byte songType = 0,
                string bgmCueNameShort = "",
                string bgmCuesheetNameShort = "",
                string bgmCueNameGamesize = "",
                string bgmCuesheetNameGamesize = "",
                int shortLength = 0,
                int alterJacket = 0,
                long startDate = 0L,
                long endDate = 0L)
            {
                MusicId = musicId;
                Sort = sort;
                ConditionType = conditionType;
                IsHidden = isHidden;
                VersionType = versionType;
                RequestType = requestType;
                LampColor = lampColor;
                LampAnimation = lampAnimation;
                NameTextureLength = nameTextureLength;
                SongType = songType;
                BgmCueNameShort = bgmCueNameShort ?? string.Empty;
                BgmCuesheetNameShort = bgmCuesheetNameShort ?? string.Empty;
                BgmCueNameGamesize = bgmCueNameGamesize ?? string.Empty;
                BgmCuesheetNameGamesize = bgmCuesheetNameGamesize ?? string.Empty;
                ShortLength = shortLength;
                AlterJacket = alterJacket;
                StartDate = startDate;
                EndDate = endDate;
            }

            public bool TryGetPreferredAudio(out string cuesheetName, out string cueName)
            {
                // The home jukebox table explicitly provides a short version.
                // Use it first and only fall back to game-size when the short
                // pair is missing from the current regional master.
                return TryGetAudio(false, out cuesheetName, out cueName);
            }

            public bool HasShortAudio =>
                !string.IsNullOrWhiteSpace(BgmCuesheetNameShort) &&
                !string.IsNullOrWhiteSpace(BgmCueNameShort);

            public bool HasGameSizeAudio =>
                !string.IsNullOrWhiteSpace(BgmCuesheetNameGamesize) &&
                !string.IsNullOrWhiteSpace(BgmCueNameGamesize);

            public bool TryGetAudio(bool useGameSize, out string cuesheetName, out string cueName)
            {
                if (useGameSize && HasGameSizeAudio)
                {
                    cuesheetName = BgmCuesheetNameGamesize;
                    cueName = BgmCueNameGamesize;
                    return true;
                }

                if (!useGameSize && HasShortAudio)
                {
                    cuesheetName = BgmCuesheetNameShort;
                    cueName = BgmCueNameShort;
                    return true;
                }

                // Some regional rows only contain one version. Keep those songs
                // playable even when the requested version is unavailable.
                if (HasGameSizeAudio)
                {
                    cuesheetName = BgmCuesheetNameGamesize;
                    cueName = BgmCueNameGamesize;
                    return true;
                }

                if (HasShortAudio)
                {
                    cuesheetName = BgmCuesheetNameShort;
                    cueName = BgmCueNameShort;
                    return true;
                }

                cuesheetName = string.Empty;
                cueName = string.Empty;
                return false;
            }
        }
    }
}
