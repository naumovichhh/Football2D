using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Zevruk
{
    internal interface IGameMode : IDisposable
    {
        void Handle(RenderWindow window);
        void Change(IGameMode state);
    }
}
