using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using SommarFenomen.Objects;

namespace SommarFenomen.Stats
{
    public class StatsHandler
    {
        private class SessionStats
        {
            public int EnemyKills = 0;
            public int FriendliesLost = 0;
            public bool Win = false;
        }

        private class DailyStats
        {
            public int EnemyKills = 0;
            public int FriendliesLost = 0;
            public int Wins = 0;
        }

        [Serializable()]
        public class GlobalStats
        {
            public int EnemyKills = 0;
            public int FriendliesLost = 0;
            public int Wins = 0;
        }

        private SessionStats _sessionStats;
        private DailyStats _dailyStats;
        private GlobalStats _globalStats;

        private string _globalStatsPath = @"stats.xml";
        public StatsHandler()
        {
            _globalStats = ReadFromFile(_globalStatsPath);
            _dailyStats = new DailyStats();
        }

        public void RegisterDeath(Type type)
        {
            if (type == typeof(Virus))
            {
                _sessionStats.EnemyKills++;
            }
            else if (type == typeof(GoodCell))
            {
                _sessionStats.FriendliesLost++;
            }
        }

        public void SetWinState(bool state)
        {
            _sessionStats.Win = state;
        }

        public void StartSession()
        {
            _sessionStats = new SessionStats();
        }

        public void EndSession(bool winLoss)
        {
            _sessionStats.Win = winLoss;
            AddSessionStats();
            SaveToFile(_globalStatsPath, _globalStats);

            _sessionStats = new SessionStats();
        }

        private void AddSessionStats()
        {
            _globalStats.EnemyKills += _sessionStats.EnemyKills;
            _dailyStats.EnemyKills += _sessionStats.EnemyKills;
            _globalStats.FriendliesLost += _sessionStats.FriendliesLost;
            _dailyStats.FriendliesLost += _sessionStats.FriendliesLost;

            if (_sessionStats.Win)
            {
                _dailyStats.Wins++;
                _globalStats.Wins++;
            }
        }

        private GlobalStats ReadFromFile(string filePath)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(GlobalStats));
            GlobalStats loadedStats;
            try
            {
                using (FileStream readFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    loadedStats = (GlobalStats)formatter.Deserialize(readFileStream);
                }
            }
            catch (Exception)
            {
                loadedStats = new GlobalStats();
            }

            return loadedStats;
        }
        private void SaveToFile(string filePath, Object o)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(GlobalStats));

            using (TextWriter writeFileStream = new StreamWriter(filePath))
            {
                formatter.Serialize(writeFileStream, o);
            }
        }

    }
}
