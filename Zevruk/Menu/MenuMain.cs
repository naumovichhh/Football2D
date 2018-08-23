using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal class MenuMain : IMenuMode
    {
        protected bool disposed;
        private List<MainMenuButton> buttons = new List<MainMenuButton>();

        public MenuMain()
        {
            this.buttons.Add(new MainMenuButton("Начать игру", 200, new MenuPlayTransferFactory()));
            this.buttons.Add(new MainMenuButton("Сетевая игра", 300, null, true));
            this.buttons.Add(new MainMenuButton("Инструкция", 400, new InstructionsMenuFactory()));
            this.buttons.Add(new MainMenuButton("Выйти", 500, new ExitMenuFactory()));
        }

        ~MenuMain()
        {
            this.Dispose(false);
        }

        public void Handle(RenderWindow window, Menu menu)
        {
            foreach (var button in this.buttons)
            {
                if (button.Bounds.Contains(Major.Instance.Input.MouseCoordinates.X, Major.Instance.Input.MouseCoordinates.Y))
                {
                    button.Draw(window, true);
                    if (Major.Instance.Input.ButtonPressed == Mouse.Button.Left)
                    {
                        if (button.Factory != null)
                            menu.MenuMode = button.Factory?.Create();
                    }
                }
                else
                {
                    button.Draw(window, false);
                }
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
                foreach (var button in this.buttons)
                {
                    button.Dispose();
                }
            }

            this.disposed = true;
        }
    }

    internal class MainMenuButton : Button
    {
        private const int Width = 300, Height = 50;
        private IMenuFactory factory;

        public MainMenuButton(string text, int top, IMenuFactory factory, bool unactive = false) : base(text, new Vector2f((Major.Width / 2) - (Width / 2), top), new Vector2f(Width, Height), unactive)
        {
            this.factory = factory;
        }

        public IMenuFactory Factory => this.factory;
    }
}
