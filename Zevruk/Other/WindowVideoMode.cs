using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;

namespace Zevruk
{
    internal class ActualVideoMode
    {
        private static Lazy<VideoMode> lazy = new Lazy<VideoMode>(
            () => VideoMode.DesktopMode);

        private ActualVideoMode()
        {
        }

        public static VideoMode Mode => lazy.Value;
    }
}
