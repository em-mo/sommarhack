using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SommarFenomen.Objects;

namespace SommarFenomen.LevelHandling
{
    class Level
    {
        private List<ActiveGameObject> _enemyList;
        private List<ActiveGameObject> _friendlyList;
        private List<Wall> _wallList;

        public PlayerCell Player { get; set; }

        public Level()
        {
            _enemyList = new List<ActiveGameObject>();
            _friendlyList = new List<ActiveGameObject>();
            _wallList = new List<Wall>();
        }

        public void AddWall(Wall wall)
        {
            _wallList.Add(wall);
        }

        public List<Wall> GetWalls()
        {
            return _wallList;
        }

        public void AddFriendly(ActiveGameObject item)
        {
            _friendlyList.Add(item);
        }

        public List<ActiveGameObject> GetFriendlies()
        {
            return _friendlyList;
        }

        public void AddEnemy(ActiveGameObject item)
        {
            _enemyList.Add(item);
        }

        public List<ActiveGameObject> GetEnemies()
        {
            return _enemyList;
        }

        public void SetPlayer(PlayerCell player)
        {
            Player = player;
        }
    }
}
