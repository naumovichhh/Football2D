using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;

namespace Zevruk
{
    internal class Sounds : ISounds
    {
        private SoundBuffer ballTackleBuff = new SoundBuffer("sounds/ball.ogg"),
                            ballPostTackleBuff = new SoundBuffer("sounds/post.ogg"),
                            complaintBuff = new SoundBuffer("sounds/complaint.ogg"),
                            goalBuff = new SoundBuffer("sounds/goal.ogg"),
                            winBuff = new SoundBuffer("sounds/win.ogg"),
                            whistleBuff = new SoundBuffer("sounds/final_whistle.ogg");
        private Sound ballTackle, ballPostTackle, complaint, goal, win, whistle;

        public Sounds()
        {
            this.ballTackle = new Sound(this.ballTackleBuff);
            this.ballPostTackle = new Sound(this.ballPostTackleBuff);
            this.complaint = new Sound(this.complaintBuff);
            this.complaint.Volume = 30;
            this.goal = new Sound(this.goalBuff);
            this.goal.Volume = 30;
            this.win = new Sound(this.winBuff);
            this.win.Volume = 30;
            this.whistle = new Sound(this.whistleBuff);
            this.whistle.Volume = 30;
        }

        public void Dispose()
        {
            this.ballTackle.Dispose();
            this.ballPostTackle.Dispose();
            this.goal.Dispose();
            this.complaint.Dispose();
            this.whistle.Dispose();
            this.ballTackleBuff.Dispose();
            this.ballPostTackleBuff.Dispose();
            this.winBuff.Dispose();
            this.complaintBuff.Dispose();
            this.whistleBuff.Dispose();
        }

        public void PlayBallTackle(float volume)
        {
            this.ballTackle.Volume = volume;
            this.ballTackle.Play();
        }

        public void PlayBallPostTackle(float volume)
        {
            this.ballPostTackle.Volume = volume;
            this.ballPostTackle.Play();
        }

        public void PlayWin()
        {
            this.win.Play();
        }

        public void PlayWhistle()
        {
            this.whistle.Play();
        }

        public void PlayComplaint()
        {
            this.complaint.Play();
        }

        public void PlayGoal()
        {
            this.goal.Play();
        }
    }
}
