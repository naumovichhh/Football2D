using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal class MenuInstructions : IMenuMode
    {
        protected bool disposed;
        protected RectangleShape instructionsRect;
        protected Button backButton;
        protected Text instructionsText;
        protected Font font = Fonts.Ubuntu;

        public MenuInstructions()
        {
            this.instructionsRect = new RectangleShape(new Vector2f(1200, 480))
            {
                OutlineColor = new Color(20, 20, 20),
                OutlineThickness = 2,
                FillColor = new Color(220, 220, 220)
            };
            this.instructionsRect.Position = new Vector2f(Major.Width / 2 - this.instructionsRect.Size.X / 2, Major.Height / 2 - this.instructionsRect.Size.Y / 2);
            this.backButton = new Button("Назад", new Vector2f(Major.Width / 2 - 400, Major.Height / 2 + 120), new Vector2f(200, 50));
            string instructionsStr = "Игрок слева (синий) управляется клавишами A – движение влево, D – движение вправо, W – прыжок, Space – удар.\n" +
                                     "Игрок справа (красный) управляется клавишами Left – движение влево, Right – движение вправо, Up – прыжок, Right Control – удар.\n" +
                                     "В режиме сетевой игры для управления своим игроком могут использоваться клавиши обоих наборов.\n\n" +
                                     "Приложение Settings.exe позволяет настроить длительность матча, силу отскока мяча, а также включить/отключить\n" +
                                     "опцию наказания игрока за продолжительное нахождение в своей вратарской зоне (если игрок долго стоит в своей зоне,\n" +
                                     "стрелка указывает игроку, что нужно покинуть зону, иначе увеличится высота ворот).\n";
            this.instructionsText = new Text(instructionsStr, this.font, 18)
            {
                Color = Color.Black
            };
            this.instructionsText.Position = new Vector2f(Major.Width / 2 - this.instructionsText.GetGlobalBounds().Width / 2, Major.Height / 2 - this.instructionsText.GetGlobalBounds().Height / 2 - 100);
        }

        ~MenuInstructions()
        {
            this.Dispose(false);
        }

        public void Handle(RenderWindow window, Menu menu)
        {
            window.Draw(this.instructionsRect);
            window.Draw(this.instructionsText);
            if (this.backButton.Bounds.Contains(Major.Instance.Input.MouseCoordinates.X, Major.Instance.Input.MouseCoordinates.Y))
            {
                this.backButton.Draw(window, true);
                if (Major.Instance.Input.ButtonPressed == Mouse.Button.Left)
                {
                    menu.MenuMode = new MenuMain();
                }
            }
            else
            {
                this.backButton.Draw(window, false);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.instructionsRect.Dispose();
                this.instructionsText.Dispose();
                this.backButton.Dispose();
            }

            this.disposed = true;
        }
    }
}
