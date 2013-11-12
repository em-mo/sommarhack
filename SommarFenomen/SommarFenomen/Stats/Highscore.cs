using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SommarFenomen.Stats
{
    [Serializable()]
    public class Score
    {
        public long TimeTicks { get; set; }
        public string Name { get; set; }

        public Score()
        {
            TimeTicks = long.MaxValue;
            Name = "noname";
        }

        public Score(long time, string name)
        {
            TimeTicks = time;
            Name = name;
        }
    }

    [Serializable()]
    public class Highscore
    {
        private static int NUMBER_OF_SCORES = 10;
        public List<Score> _scores;
        private int _latestScore;
 
        public Highscore()
        {
            _scores = new List<Score>();
        }

        public bool InsertScore(long value, string name = "")
        {
            int i;
            bool inserted = false;

            Score item = new Score(value, name);

            for (i = 0; i < _scores.Count; i++)
            {
                if (_scores[i].TimeTicks > item.TimeTicks)
                {
                    _latestScore = i;
                    _scores.Insert(i, item);
                    inserted = true;
                    break;
                }
            }

            if (inserted == false)
            {
                _scores.Add(item);
                _latestScore = i;
            }

            if (_scores.Count > NUMBER_OF_SCORES)
                _scores.RemoveAt(_scores.Count - 1);

            return i < NUMBER_OF_SCORES;
        }

        public Score GetScore(int index)
        {
            return _scores[index];
        }

        public Score GetLatestScore()
        {
            return GetScore(_latestScore);
        }

        public int GetLatestScoreIndex()
        {
            return _latestScore;
        }
    }
}
