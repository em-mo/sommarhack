using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using SommarFenomen.Objects;
using System.Diagnostics;

namespace SommarFenomen.Stats
{
    public class StatsHandler
    {
        private class SessionStats
        {
            public int EnemyKills = 0;
            public int FriendliesLost = 0;
            public bool Win = false;
            public TimeSpan Time = TimeSpan.MinValue;
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
            public Highscore Scores;
        }

        private SessionStats _sessionStats;
        private DailyStats _dailyStats;
        private GlobalStats _globalStats;

        private Stopwatch _sessionTimer;
        private string _globalStatsPath = @"stats.xml";
        public StatsHandler()
        {
            _globalStats = ReadFromFile(_globalStatsPath);
            _dailyStats = new DailyStats();
            _sessionTimer = new Stopwatch();
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
            _sessionTimer.Restart();
        }

        public void EndSession(bool winLoss)
        {
            _sessionTimer.Stop();
            _sessionStats.Time = _sessionTimer.Elapsed;
            _sessionStats.Win = winLoss;
            AddSessionStats(winLoss);
            SaveToFile(_globalStatsPath, _globalStats);

            _sessionStats = new SessionStats();
        }

        public void Save()
        {
            SaveToFile(_globalStatsPath, _globalStats);
        }

        private void AddSessionStats(bool winLoss)
        {
            _globalStats.EnemyKills += _sessionStats.EnemyKills;
            _dailyStats.EnemyKills += _sessionStats.EnemyKills;
            _globalStats.FriendliesLost += _sessionStats.FriendliesLost;
            _dailyStats.FriendliesLost += _sessionStats.FriendliesLost;

            if (winLoss)
                _globalStats.Scores.InsertScore(_sessionStats.Time.Ticks);
            
            Console.WriteLine("Scores ");
            foreach (Score score in _globalStats.Scores._scores)
                Console.WriteLine(new TimeSpan(score.TimeTicks));
            if (_sessionStats.Win)
            {
                _dailyStats.Wins++;
                _globalStats.Wins++;
            }
        }

        public Highscore GetHighscores()
        {
            return _globalStats.Scores;
        }
        
        private XmlSerializer GetSerializer()
        {
            Type[] types = new Type[2];
            types[0] = typeof(Highscore);
            types[1] = typeof(Score);
            return new XmlSerializer(typeof(GlobalStats), types);
        }

        private GlobalStats ReadFromFile(string filePath)
        {
            XmlSerializer formatter = GetSerializer();
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

            if (loadedStats.Scores == null)
                loadedStats.Scores = new Highscore();

            return loadedStats;
        }

        private void SaveToFile(string filePath, Object o)
        {
            XmlSerializer formatter = GetSerializer();

            using (TextWriter writeFileStream = new StreamWriter(filePath))
            {
                formatter.Serialize(writeFileStream, o);
            }
        }

    }
}
