using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zevruk
{
    internal interface ISounds : IDisposable
    {
        void PlayBallTackle(float volume);
        void PlayBallPostTackle(float volume);
        void PlayWin();
        void PlayWhistle();
        void PlayComplaint();
        void PlayGoal();
    }
}
