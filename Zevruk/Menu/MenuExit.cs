using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Zevruk
{
    internal class MenuExit : IMenuMode
    {
        public void Handle(RenderWindow window, Menu menu)
        {
            window.Close();
        }

        public void Dispose()
        {
        }
    }
}
