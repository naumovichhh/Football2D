using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zevruk
{
    internal interface IMenuFactory
    {
        IMenuMode Create();
    }
}
