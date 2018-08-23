using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Zevruk
{
    internal class MenuPlayTransfer : IMenuMode
    {
        public void Handle(RenderWindow window, Menu menu)
        {
            menu.Change(new Play(window));
        }

        public void Dispose()
        {
        }
    }
}
