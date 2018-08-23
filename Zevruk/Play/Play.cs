using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal class Play : IGameMode
    {
        protected bool disposed;
        protected bool gameOver;
        protected Texture exitTexture;
        protected Sprite exit;
        protected Gameplay gameplay = new Gameplay();
        protected ISounds sounds = new Sounds();
        protected PlayGraphics playGraphics;
        protected System.Timers.Timer matchEndedTimer = new System.Timers.Timer(), afterGoalBreakTimer = new System.Timers.Timer();
        protected bool matchEnded, afterGoalBreak;

        public Play(RenderWindow window)
        {
            Image exitImage = new Image("images/exit.png");
            exitImage.CreateMaskFromColor(Color.White);
            this.exitTexture = new Texture(exitImage);
            this.exit = new Sprite(this.exitTexture);
            this.exit.Position = new Vector2f(Major.Width - this.exit.TextureRect.Width, 0);
            this.gameplay.BallTackled += this.OnBallTackled;
            this.gameplay.BallPostTackled += this.OnBallPostTackled;
            this.playGraphics = new PlayGraphics(100);
            this.gameplay.LeftPostRaised += this.playGraphics.OnLeftPostRaised;
            this.gameplay.RightPostRaised += this.playGraphics.OnRightPostRaised;
            this.gameplay.LeftConceded += this.OnLeftConceded;
            this.gameplay.RightConceded += this.OnRightConceded;
            this.gameplay.GameDurationDefined += this.playGraphics.OnGameDurationDefined;
            this.gameplay.SecPassed += this.playGraphics.OnSecPassed;
            this.gameplay.MatchEnded += this.OnMatchEnded;
            this.matchEndedTimer.Elapsed += this.ExitingAfterMatch;
            this.matchEndedTimer.AutoReset = false;
            this.matchEndedTimer.Interval = 5000;
            this.afterGoalBreakTimer.AutoReset = false;
            this.afterGoalBreakTimer.Interval = 2000;
            this.afterGoalBreakTimer.Elapsed += this.AfterGoalBreakEnded;
            this.Start();
        }

        ~Play()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Change(IGameMode state)
        {
            Major.Instance.Mode = state;
            this.Dispose();
        }

        public void Handle(RenderWindow window)
        {
            if (this.ExitRequested() || this.gameOver)
            {
                this.Change(new Menu());
                return;
            }

            lock (Major.Instance)
            {
                if (!this.disposed)
                    this.Draw(window);
            }

            this.gameplay.Input = this.GetPlayerInput();
        }

        public void Start()
        {
            this.gameplay.Start();
        }

        protected virtual bool ExitRequested()
        {
            if (this.exit.GetGlobalBounds().Contains(Major.Instance.Input.MouseCoordinates.X, Major.Instance.Input.MouseCoordinates.Y))
                if (Major.Instance.Input.ButtonPressed == Mouse.Button.Left)
                    return true;

            if (Major.Instance.Input.KeyPressed == Keyboard.Key.Escape)
                return true;

            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.sounds.Dispose();
                this.gameplay.Dispose();
                this.exit.Dispose();
                this.exitTexture.Dispose();
                lock (Major.Instance)
                {
                    this.playGraphics.Dispose();
                }

                this.afterGoalBreakTimer.Stop();
                this.afterGoalBreakTimer.Dispose();
                this.matchEndedTimer.Stop();
                this.matchEndedTimer.Dispose();
            }

            this.disposed = true;
        }

        private void OnLeftConceded()
        {
            this.playGraphics.OnLeftConceded();
            this.OnConceded();
        }

        private void OnRightConceded()
        {
            this.playGraphics.OnRightConceded();
            this.OnConceded();
        }

        private void OnConceded()
        {
            this.sounds.PlayGoal();
            this.afterGoalBreak = true;
            this.afterGoalBreakTimer.Start();
        }

        private void OnMatchEnded(int result)
        {
            this.sounds.PlayWhistle();
            if (result != 0)
                this.sounds.PlayWin();

            this.matchEndedTimer.Start();
            this.playGraphics.Result = result;
            this.matchEnded = true;
        }

        private void ExitingAfterMatch(object sender, EventArgs e)
        {
            this.gameOver = true;
        }

        private void OnBallTackled(float intensity)
        {
            this.sounds.PlayBallTackle(intensity * 40);
        }

        private void OnBallPostTackled(float intensity)
        {
            this.sounds.PlayBallPostTackle(intensity * 40);
        }

        private void AfterGoalBreakEnded(object sender, EventArgs args)
        {
            this.afterGoalBreak = false;
        }

        private void Draw(RenderWindow window)
        {
            this.playGraphics.ObjectsState = this.gameplay.GetGameObjectsState();
            this.playGraphics.Draw(window);
            if (this.matchEnded)
            {
                this.playGraphics.DrawResult(window);
            }

            if (this.afterGoalBreak)
                this.playGraphics.DrawGolaso(window);

            window.Draw(this.exit);
        }

        private PlayersInput GetPlayerInput()
        {
            var input = new PlayersInput();
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                input.PlayerLeftLeft = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                input.PlayerLeftRight = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                input.PlayerLeftJump = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.LControl))
            {
                input.PlayerLeftKick = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                input.PlayerRightLeft = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                input.PlayerRightRight = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                input.PlayerRightJump = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.RControl))
            {
                input.PlayerRightKick = true;
            }

            return input;
        }
    }
}
