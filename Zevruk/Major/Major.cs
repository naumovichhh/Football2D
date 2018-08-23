using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SFML.Graphics;
using SFML.Window;

namespace Zevruk
{
    internal sealed class Major
    {
        public const int Width = 1366, Height = 768;

        private RenderWindow window = new RenderWindow(ActualVideoMode.Mode, "Zevruk", Styles.Fullscreen, new ContextSettings() { AntialiasingLevel = 8u });
        private IGameMode mode = new Menu();
        private Input input;

        public Major()
        {
            this.input = new Input(this.window);
            this.window.Closed += (s, a) => this.window.Close();
            this.window.MouseButtonPressed += this.input.OnMouseButtonPressed;
            this.window.KeyPressed += this.KeyPressed;
            this.window.SetFramerateLimit(60);
            Vector2f viewSize;
            if (ActualVideoMode.Mode.Width / (double)ActualVideoMode.Mode.Height < 16 / 9.006)
            {
                viewSize = new Vector2f(Width, ActualVideoMode.Mode.Height * Width / ActualVideoMode.Mode.Width);
            }
            else
            {
                viewSize = new Vector2f(ActualVideoMode.Mode.Width * Height / ActualVideoMode.Mode.Height, Height);
            }

            this.window.SetView(
                new View(
                    new Vector2f(Width / 2, Height / 2),
                    viewSize));
            this.window.Resized += this.OnWindowResized;
        }

        public static Major Instance => Nested.Instance;

        public bool Closed => !this.window.IsOpen();

        public IGameMode Mode
        {
            get => this.mode;
            set
            {
                this.mode = value;
                this.mode.Handle(this.window);
            }
        }

        public Input Input
        {
            get => this.input;
        }

        public void Work()
        {
            this.input.Reset();
            this.window.DispatchEvents();
            this.CheckForAltf4();
            this.Mode.Handle(this.window);
            this.Display();
        }

        private void OnWindowResized(object sender, SizeEventArgs args)
        {
            Vector2f viewSize;
            if (args.Width / (double)args.Height < 16 / 9.006)
            {
                viewSize = new Vector2f(Width, args.Height * Width / args.Width);
            }
            else
            {
                viewSize = new Vector2f(args.Width * Height / args.Height, Height);
            }

            this.window.SetView(
                new View(
                    new Vector2f(Width / 2, Height / 2),
                    viewSize));
        }

        private void CheckForAltf4()
        {
            if (this.input.Altf4)
                this.window.Close();
        }

        private void KeyPressed(object sender, KeyEventArgs args)
        {
            if (args.Code == Keyboard.Key.F4 && args.Alt)
            {
                this.window.Close();
            }
        }

        private void Display()
        {
            this.window.Display();
            this.window.Clear();
        }

        private class Nested
        {
            internal static Major Instance = new Major();

            static Nested()
            {
            }
        }
    }
}
