using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal struct GameObjectsState
    {
        public Vector2f PlayerLeftPosition, LegLeftPosition, PlayerRightPosition, LegRightPosition, BallPosition;
        public float BallAngle, LegLeftAngle, LegRightAngle;
    }

    internal class Gameplay : IDisposable
    {
        protected bool disposed;
        protected PhysicsProcess physics;
        protected Settings settings = new Settings();
        protected Scoreboard scoreboard;
        protected System.Timers.Timer afterScoreTimer = new System.Timers.Timer(), beginPlayTimer = new System.Timers.Timer();
        protected bool stopped = false;
        protected PlayersInput input;
        protected bool scored;
        protected Stopwatch stopwatch;
        protected Thread handlerThread;
        protected bool shouldAttack;
        protected PhysicsProcess.BallRebound ballRebound;
        protected int matchDuration;

        public Gameplay()
        {
            this.settings.Read();
            int ballRebound;
            bool shouldAttack;
            int matchDuration;
            this.physics = new PhysicsProcess(
                this.ballRebound = int.TryParse(this.settings["BallRebound"], out ballRebound) ? (PhysicsProcess.BallRebound)ballRebound : PhysicsProcess.BallRebound.Medium,
                this.shouldAttack = bool.TryParse(this.settings["ShouldAttack"], out shouldAttack) ? shouldAttack : false);
            this.scoreboard = new Scoreboard(this.matchDuration = int.TryParse(this.settings["MatchDuration"], out matchDuration) ? matchDuration : 120);
            this.afterScoreTimer.Elapsed += this.OnAfterScoreEnded;
            this.afterScoreTimer.Interval = 2000;
            this.afterScoreTimer.AutoReset = false;
            this.beginPlayTimer.AutoReset = false;
            this.beginPlayTimer.Interval = 2000;
            this.beginPlayTimer.Elapsed += this.OnBeginningPlay;
            this.physics.BallTackled += this.OnBallTackled;
            this.physics.BallPostTackled += this.OnBallTackledPost;
            this.physics.RightPostRaised += this.OnRightPostRaised;
            this.physics.LeftPostRaised += this.OnLeftPostRaised;
            this.physics.LeftConceded += this.OnLeftConceded;
            this.physics.RightConceded += this.OnRightConceded;
            this.scoreboard.SecondPassed += this.OnSecondPassed;
            this.scoreboard.MatchEnded += this.OnMatchEnded;
        }

        ~Gameplay()
        {
            this.Dispose(false);
        }

        public event Action<float> BallTackled, BallPostTackled;
        public event Action LeftPostRaised, RightPostRaised;
        public event Action LeftConceded, RightConceded;
        public event Action SecPassed;
        public event Action<int> GameDurationDefined;
        public event Action<int> MatchEnded;

        public PlayersInput Input
        {
            get => this.input;
            set => this.input = value;
        }

        public GameObjectsState GetGameObjectsState()
        {
            GameObjectsState result = new GameObjectsState()
            {
                BallPosition = new Vector2f(this.physics.BallPosition.X, this.physics.BallPosition.Y),
                BallAngle = this.physics.BallAngle,
                PlayerLeftPosition = new Vector2f(this.physics.PlayerLeftPosition.X, this.physics.PlayerLeftPosition.Y),
                LegLeftPosition = new Vector2f(this.physics.LegLeftPosition.X, this.physics.LegLeftPosition.Y),
                LegLeftAngle = this.physics.LegLeftAngle,
                PlayerRightPosition = new Vector2f(this.physics.PlayerRightPosition.X, this.physics.PlayerRightPosition.Y),
                LegRightPosition = new Vector2f(this.physics.LegRightPosition.X, this.physics.LegRightPosition.Y),
                LegRightAngle = this.physics.LegRightAngle
            };
            return result;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            this.beginPlayTimer.Start();
            this.GameDurationDefined?.Invoke(this.matchDuration);
        }

        public void Stop()
        {
            this.stopped = true;
        }

        public void OnAfterScoreEnded(object sender, EventArgs e)
        {
            this.ReloadPhysics();
            this.physics.BallTackled += this.OnBallTackled;
            this.physics.BallPostTackled += this.OnBallTackledPost;
            this.physics.RightPostRaised += this.OnRightPostRaised;
            this.physics.LeftPostRaised += this.OnLeftPostRaised;
            this.physics.LeftConceded += this.OnLeftConceded;
            this.physics.RightConceded += this.OnRightConceded;
            this.StopHandler();
            this.beginPlayTimer.Start();
        }

        public void OnBeginningPlay(object sender, EventArgs e)
        {
            this.StartHandler();
        }

        public void Handle()
        {
            while (!this.stopped)
            {
                float iterationDuration = this.GetIterationDuration();
                this.physics.Handle(iterationDuration, this.input);
                if (!this.scored)
                {
                    this.scoreboard.TimePassed(iterationDuration);
                }

                Thread.Sleep(4);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                if (this.handlerThread.IsAlive)
                    this.handlerThread.Abort();
                this.afterScoreTimer.Stop();
                this.afterScoreTimer.Dispose();
                this.beginPlayTimer.Stop();
                this.beginPlayTimer.Dispose();
            }

            this.disposed = true;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void WaitTime()
        {
            while (this.stopwatch.Elapsed.TotalMilliseconds < 4)
            {
                for (int i = 1; i <= 10000; ++i)
                {
                }
            }
        }

        private void OnSecondPassed()
        {
            this.SecPassed?.Invoke();
        }

        private void ReloadPhysics()
        {
            this.physics = new PhysicsProcess(this.ballRebound, this.shouldAttack);
            this.scored = false;
        }

        private void OnBallTackled(float intensity)
        {
            this.BallTackled?.Invoke(intensity);
        }

        private void OnBallTackledPost(float intensity)
        {
            this.BallPostTackled?.Invoke(intensity);
        }

        private void OnMatchEnded()
        {
            this.Stop();
            this.MatchEnded?.Invoke(this.scoreboard.Score[0].CompareTo(this.scoreboard.Score[1]));
        }

        private float GetIterationDuration()
        {
            if (this.stopwatch.IsRunning)
            {
                this.WaitTime();
                double result = this.stopwatch.Elapsed.TotalMilliseconds;
                this.stopwatch.Restart();
                return (float)result;
            }
            else
            {
                this.stopwatch.Restart();
                return 4;
            }
        }

        private void StartHandler()
        {
            this.stopwatch = new Stopwatch();
            this.handlerThread = new Thread(new ThreadStart(this.Handle));            
            this.handlerThread.Priority = ThreadPriority.Highest;
            this.handlerThread.IsBackground = true;
            this.handlerThread.Start();
        }

        private void StopHandler()
        {
            this.handlerThread.Abort();
            this.stopwatch.Stop();
        }

        private void OnRightConceded()
        {
            this.scoreboard.Score[0]++;
            this.RightConceded?.Invoke();
            this.afterScoreTimer.Start();
            this.scored = true;
        }

        private void OnLeftConceded()
        {
            this.scoreboard.Score[1]++;
            this.LeftConceded?.Invoke();
            this.afterScoreTimer.Start();
            this.scored = true;
        }

        private void OnLeftPostRaised()
        {
            this.LeftPostRaised?.Invoke();
        }

        private void OnRightPostRaised()
        {
            this.RightPostRaised?.Invoke();
        }
    }

    internal class Scoreboard
    {
        private uint[] score = new uint[2];
        private float remainTime;

        public Scoreboard(int timeLeft)
        {
            this.remainTime = timeLeft * 1000;
        }

        public event Action SecondPassed, MatchEnded;

        public uint[] Score
        {
            get => this.score;
        }

        public float RemainTime
        {
            get => this.remainTime;
        }

        public void OnLeftConceded()
        {
            this.score[1]++;
        }

        public void RightConcede()
        {
            this.score[0]++;
        }

        public void TimePassed(float milliseconds)
        {
            float prevRemainTime = this.remainTime;
            this.remainTime -= milliseconds;
            if (Math.Floor(this.remainTime / 1000 - 0.04f) != Math.Floor(prevRemainTime / 1000 - 0.04f))
                this.SecondPassed?.Invoke();

            if (this.remainTime <= 0)
                this.MatchEnded?.Invoke();
        }
    }
}
