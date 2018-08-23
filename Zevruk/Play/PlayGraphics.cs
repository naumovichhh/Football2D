using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;
using static System.Math;

namespace Zevruk
{
    internal class PlayGraphics : IDisposable
    {
        protected bool disposed;
        protected GameObjectsState objectsState;
        protected float scale;
        private Sprite playerLeft, legLeft, playerRight, legRight, ball, goalLeft, goalRight, background, arrowLeft,
            arrowRight;
        private Texture playerLeftTexture, playerRightTexture, legTexture, ballTexture,
            goalTexture, backgroundTexture, arrowTexture;
        private Font font;
        private Text[] scoreText = new Text[] { new Text(), new Text() };
        private Text remainTimeText = new Text();
        private int remainTime;
        private int[] score = new int[2];
        private Text golasoText = new Text();
        private IResultText resultText;

        public PlayGraphics(float scale)
        {
            this.scale = scale;
            this.backgroundTexture = new Texture("images/background.png");
            this.background = new Sprite()
            {
                Texture = this.backgroundTexture,
                TextureRect = new IntRect(0, 0, Major.Width, Major.Height),
                Position = new Vector2f(0, 0)
            };
            Image goalImage = new Image("images/goal.png");
            goalImage.CreateMaskFromColor(Color.Black);
            this.goalTexture = new Texture(goalImage);
            this.goalLeft = new Sprite()
            {
                Texture = this.goalTexture,
                TextureRect = new IntRect(0, 0, 100, 200),
                Position = new Vector2f(0, 496)
            };
            this.goalRight = new Sprite()
            {
                Texture = this.goalTexture,
                TextureRect = new IntRect(0, 0, 100, 200),
                Scale = new Vector2f(-1, 1),
                Position = new Vector2f(Major.Width, 496)
            };
            Image playerLeftImage = new Image("images/player1.png");
            playerLeftImage.CreateMaskFromColor(Color.White);
            this.playerLeftTexture = new Texture(playerLeftImage);
            this.playerLeft = new Sprite()
            {
                Texture = this.playerLeftTexture,
                TextureRect = new IntRect(0, 0, 30, 80)
            };
            Image playerRightImage = new Image("images/player2.png");
            playerRightImage.CreateMaskFromColor(Color.White);
            this.playerRightTexture = new Texture(playerRightImage);
            this.playerRight = new Sprite()
            {
                Texture = this.playerRightTexture,
                TextureRect = new IntRect(0, 0, 30, 80),
                Scale = new Vector2f(-1, 1),
            };
            Image ballImage = new Image("images/ball.png");
            ballImage.CreateMaskFromColor(new Color(0, 162, 232));
            this.ballTexture = new Texture(ballImage);
            this.ball = new Sprite() { Texture = this.ballTexture };
            Image legImage = new Image("images/leg.png");
            legImage.CreateMaskFromColor(Color.White);
            this.legTexture = new Texture(legImage);
            this.legLeft = new Sprite(this.legTexture);
            this.legRight = new Sprite(this.legTexture) { Scale = new Vector2f(-1, 1) };
            Image arrowImage = new Image("images/arrow.png");
            arrowImage.CreateMaskFromColor(Color.White);
            this.arrowTexture = new Texture(arrowImage);
            this.arrowLeft = new Sprite(this.arrowTexture);
            this.arrowRight = new Sprite(this.arrowTexture) { Scale = new Vector2f(-1, 1) };
            this.font = Fonts.Verdana;
            this.remainTimeText.Font = this.font;
            this.remainTimeText.CharacterSize = 40;
            this.scoreText[0].Font = this.font;
            this.scoreText[0].CharacterSize = 60;
            this.scoreText[0].Position = new Vector2f(20, 20);
            this.scoreText[1].Font = this.font;
            this.scoreText[1].CharacterSize = 60;
            this.golasoText.Font = this.font;
            this.golasoText.CharacterSize = 100;
            this.golasoText.DisplayedString = "ГОЛ";
            this.golasoText.Position = new Vector2f(
                Major.Width / 2f - this.golasoText.GetGlobalBounds().Width / 2f,
                Major.Height / 2f - this.golasoText.GetGlobalBounds().Height / 2f);
        }

        ~PlayGraphics()
        {
            this.Dispose(false);
        }
        
        internal interface IResultText : IDisposable
        {
            void Draw(RenderWindow window);
        }

        public GameObjectsState ObjectsState
        {
            set
            {
                this.objectsState = value;
            }
        }

        public int Result
        {
            set
            {
                if (value == 0)
                {
                    this.resultText = new TieResultText(this.font);
                }
                else
                {
                    this.resultText = new WinLooseResultText(value, this.font);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Draw(RenderWindow window)
        {
            this.playerLeft.Position = new Vector2f(
                this.objectsState.PlayerLeftPosition.X * this.scale - this.playerLeft.TextureRect.Width / 2f,
                this.objectsState.PlayerLeftPosition.Y * this.scale - 50);
            this.legLeft.Position = new Vector2f(
                this.objectsState.LegLeftPosition.X * this.scale - 7 * (float)Cos(this.objectsState.LegLeftAngle) + 16 * (float)Sin(this.objectsState.LegLeftAngle),
                this.objectsState.LegLeftPosition.Y * this.scale - 16 * (float)Cos(this.objectsState.LegLeftAngle) - 7 * (float)Sin(this.objectsState.LegLeftAngle));
            this.playerRight.Position = new Vector2f(
                this.objectsState.PlayerRightPosition.X * this.scale + this.playerRight.TextureRect.Width / 2f,
                this.objectsState.PlayerRightPosition.Y * this.scale - 50);
            this.legRight.Position = new Vector2f(
                this.objectsState.LegRightPosition.X * this.scale + 7 * (float)Cos(this.objectsState.LegRightAngle) + 16 * (float)Sin(this.objectsState.LegRightAngle),
                this.objectsState.LegRightPosition.Y * this.scale - 16 * (float)Cos(this.objectsState.LegRightAngle) + 7 * (float)Sin(this.objectsState.LegRightAngle));
            this.ball.Rotation = AngleConversion.RadianToDegree(this.objectsState.BallAngle);
            this.ball.Position = new Vector2f(
                this.objectsState.BallPosition.X * this.scale - (float)Sqrt(2) * this.ball.TextureRect.Width / 2f * (float)Sin(PI / 4 - this.objectsState.BallAngle),
                this.objectsState.BallPosition.Y * this.scale - (float)Sqrt(2) * this.ball.TextureRect.Width / 2f * (float)Cos(PI / 4 - this.objectsState.BallAngle));
            string remainTimeString = (this.remainTime / 60).ToString().PadLeft(2, '0') + ":" + (this.remainTime % 60).ToString().PadLeft(2, '0');
            this.remainTimeText.DisplayedString = remainTimeString;
            this.remainTimeText.Position = new Vector2f(Major.Width / 2 - this.remainTimeText.GetGlobalBounds().Width / 2, 10);
            this.scoreText[0].DisplayedString = this.score[0].ToString();
            this.scoreText[1].DisplayedString = this.score[1].ToString();
            this.scoreText[1].Position = new Vector2f(Major.Width - 20 - this.scoreText[1].GetGlobalBounds().Width, 20);
            window.Draw(this.background);
            window.Draw(this.playerLeft);
            window.Draw(this.legLeft);
            window.Draw(this.playerRight);
            window.Draw(this.legRight);
            window.Draw(this.ball);
            window.Draw(this.goalLeft);
            window.Draw(this.goalRight);
            window.Draw(this.remainTimeText);
            window.Draw(this.scoreText[0]);
            window.Draw(this.scoreText[1]);
        }

        public void OnSecPassed()
        {
            --this.remainTime;
        }

        public virtual void OnLeftPostRaised()
        {
            this.goalLeft.TextureRect = new IntRect(0, 0, 100, this.goalLeft.TextureRect.Height + 50);
            this.goalLeft.Position = new Vector2f(this.goalLeft.Position.X, this.goalLeft.Position.Y - 50);
        }

        public virtual void OnRightPostRaised()
        {
            this.goalRight.TextureRect = new IntRect(0, 0, 100, this.goalRight.TextureRect.Height + 50);
            this.goalRight.Position = new Vector2f(this.goalRight.Position.X, this.goalRight.Position.Y - 50);
        }

        public virtual void DrawGolaso(RenderWindow window)
        {
            window.Draw(this.golasoText);
        }

        public virtual void DrawResult(RenderWindow window)
        {
            this.resultText.Draw(window);
        }

        public void OnGameDurationDefined(int duration)
        {
            this.remainTime = duration;
        }

        public void OnLeftConceded()
        {
            ++this.score[1];
        }

        public void OnRightConceded()
        {
            ++this.score[0];
        }

        public virtual void DrawLeftArrow(RenderWindow window)
        {
            this.arrowLeft.Position = new Vector2f(this.playerLeft.Position.X * this.scale - this.arrowLeft.TextureRect.Width / 2f, this.playerLeft.Position.Y * this.scale - this.playerLeft.TextureRect.Height - this.arrowLeft.TextureRect.Height / 2f);
            window.Draw(this.arrowLeft);
        }

        public virtual void DrawRightArrow(RenderWindow window)
        {
            this.arrowRight.Position = new Vector2f(this.playerRight.Position.X * this.scale + this.arrowRight.TextureRect.Width / 2f, this.playerRight.Position.Y * this.scale - this.playerRight.TextureRect.Height - this.arrowRight.TextureRect.Height / 2f);
            window.Draw(this.arrowRight);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.playerLeft.Dispose();
                this.playerRight.Dispose();
                this.background.Dispose();
                this.ball.Dispose();
                this.arrowLeft.Dispose();
                this.arrowRight.Dispose();
                this.goalLeft.Dispose();
                this.goalRight.Dispose();
                this.golasoText.Dispose();
                this.legLeft.Dispose();
                this.legRight.Dispose();
                this.remainTimeText.Dispose();
                this.scoreText[0].Dispose();
                this.scoreText[1].Dispose();
                this.resultText?.Dispose();
                this.playerLeftTexture.Dispose();
                this.playerRightTexture.Dispose();
                this.legTexture.Dispose();
                this.ballTexture.Dispose();
                this.arrowTexture.Dispose();
                this.backgroundTexture.Dispose();
                this.goalTexture.Dispose();
            }

            this.disposed = true;
        }

        internal class WinLooseResultText : IResultText
        {
            private Text win = new Text(), loose = new Text();

            public WinLooseResultText(int result, Font font)
            {
                this.win.Font = this.loose.Font = font;
                this.win.CharacterSize = this.loose.CharacterSize = 80;
                this.win.DisplayedString = "ПОБЕДИЛ";
                Vector2f p = new Vector2f(Major.Width / 4f, Major.Height / 2f - this.win.GetGlobalBounds().Height / 2f);
                this.loose.DisplayedString = "ПРОИГРАЛ";
                if (result > 0)
                {
                    this.win.Position = new Vector2f(p.X - this.win.GetGlobalBounds().Width / 2f, p.Y);
                    this.loose.Position = new Vector2f(p.X + Major.Width / 2 - this.loose.GetGlobalBounds().Width / 2f, p.Y);
                }
                else
                {
                    this.win.Position = new Vector2f(p.X + Major.Width / 2 - this.win.GetGlobalBounds().Width / 2f, p.Y);
                    this.loose.Position = new Vector2f(p.X - this.loose.GetGlobalBounds().Width / 2f, p.Y);
                }
            }

            public void Dispose()
            {
                this.win.Dispose();
                this.loose.Dispose();
            }

            public void Draw(RenderWindow window)
            {
                window.Draw(this.loose);
                window.Draw(this.win);
            }
        }

        internal class TieResultText : IResultText
        {
            protected Text text = new Text();

            public TieResultText(Font font)
            {
                this.text.DisplayedString = "НИЧЬЯ";
                this.text.Font = font;
                this.text.CharacterSize = 100;
                this.text.Position = new Vector2f(
                    Major.Width / 2f - this.text.GetGlobalBounds().Width / 2f,
                    Major.Height / 2f - this.text.GetGlobalBounds().Height / 2f);
            }

            public void Dispose()
            {
                this.text.Dispose();
            }

            public void Draw(RenderWindow window)
            {
                window.Draw(this.text);
            }
        }
    }
}
