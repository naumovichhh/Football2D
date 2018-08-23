using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal interface IBackground : IDisposable
    {
        void Draw(RenderWindow window);
    }

    internal class Background : IBackground
    {
        private Texture backgroundTexture = new Texture("images/background.png"), goalTexture;
        private Sprite background, goal1, goal2;

        public Background()
        {
            Image goalImage = null;
            while (this.goalTexture == null)
            {
                try
                {
                    goalImage = new Image("images/goal.png");
                    goalImage.CreateMaskFromColor(Color.Black);
                    this.goalTexture = new Texture(goalImage);
                }
                catch
                {
                    goalImage?.Dispose();
                }
            }

            this.background = new Sprite(this.backgroundTexture);
            this.background.Position = new Vector2f(0, 0);
            this.goal1 = new Sprite(this.goalTexture, new IntRect(0, 0, 100, 200));
            this.goal1.Position = new Vector2f(0, 496);
            this.goal2 = new Sprite(this.goalTexture, new IntRect(0, 0, 100, 200));
            this.goal2.Scale = new Vector2f(-1, 1);
            this.goal2.Position = new Vector2f(1366, 496);
        }

        public void Dispose()
        {
            this.goal1.Dispose();
            this.goal2.Dispose();
            this.background.Dispose();
            this.goalTexture.Dispose();
            this.backgroundTexture.Dispose();
        }

        public void Draw(RenderWindow window)
        {
            window.Draw(this.background);
            window.Draw(this.goal1);
            window.Draw(this.goal2);
        }
    }
}
