using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SommarFenomen.Windows.WindowUtils;

namespace SommarFenomen.Windows
{
    class ProgressionSelectWindow : LevelSelectWindow
    {
        private int _currentLevel;

        public ProgressionSelectWindow(WindowHandler windowHandler) : base(windowHandler)
        {
            _bodyParts.OrderBy(o => o.LevelNumber);
        }

        public override LevelBodyPart ChooseNextBodyPart()
        {
            return _bodyParts[_currentLevel];
        }

        public override void OnChange(object o)
        {
            if (o == null)
                _currentLevel = 0;
            else if (o is bool)
            {
                if ((bool)o)
                    _currentLevel++;
            }
            base.OnChange(o);
        }
    }
}
