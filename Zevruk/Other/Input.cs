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
    internal class Input
    {
        private RenderWindow window;

        public Input(RenderWindow window)
        {
            this.window = window;
        }

        public Mouse.Button? ButtonPressed { get; set; }
        public Keyboard.Key? KeyPressed { get; set; }
        public bool Altf4 { get; set; }

        public Vector2f MouseCoordinates
        {
            get
            {
                Mouse.GetPosition();
                Vector2i i = Mouse.GetPosition(this.window);
                return this.window.MapPixelToCoords(i, this.window.GetView());
            }
        }

        public void OnMouseButtonPressed(object sender, MouseButtonEventArgs args)
        {
            this.ButtonPressed = args.Button;
        }

        public void OnKeyPressed(object sender, KeyEventArgs args)
        {
            this.KeyPressed = args.Code;
            if (args.Code == Keyboard.Key.F4 && args.Alt)
                this.Altf4 = true;
        }

        public void Reset()
        {
            this.ButtonPressed = null;
            this.KeyPressed = null;
            this.Altf4 = false;
        }
    }
}
