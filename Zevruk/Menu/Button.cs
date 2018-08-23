using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal class Button : IDisposable
    {
        protected bool disposed;
        protected RectangleShape shape;
        protected Text text;
        protected bool unactive;

        public Button(string text, Vector2f position, Vector2f size, bool unactive = false)
        {
            this.shape = new RectangleShape(new Vector2f(size.X, size.Y));
            this.shape.Position = position;
            this.text = new Text(text, Fonts.Ubuntu);
            this.text.Position = new Vector2f(
                this.shape.Position.X + this.shape.Size.X / 2 - this.text.GetGlobalBounds().Width / 2,
                this.shape.Position.Y + this.shape.Size.Y / 2 - this.text.GetGlobalBounds().Height * 5 / 6);
            this.text.Color = Color.White;
            this.unactive = unactive;
        }

        ~Button()
        {
            this.Dispose(false);
        }

        public FloatRect Bounds => this.shape.GetGlobalBounds();

        public virtual void Draw(RenderWindow window, bool active)
        {
            if (this.unactive)
            {
                this.shape.FillColor = new Color(80, 80, 80);
            }
            else if (active)
            {
                this.shape.FillColor = new Color(2, 25, 60);
            }
            else
            {
                this.shape.FillColor = new Color(8, 74, 100);
            }

            window.Draw(this.shape);
            window.Draw(this.text);
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
                this.text.Dispose();
                this.shape.Dispose();
            }

            this.disposed = true;
        }
    }
}
