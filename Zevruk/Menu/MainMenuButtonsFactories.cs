using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zevruk
{
    internal class InstructionsMenuFactory : IMenuFactory
    {
        public IMenuMode Create()
        {
            return new MenuInstructions();
        }
    }

    internal class NetworkPlayMenuFactory : IMenuFactory
    {
        public IMenuMode Create()
        {
            return new MenuNetworkPlay();
        }
    }

    internal class ExitMenuFactory : IMenuFactory
    {
        public IMenuMode Create()
        {
            return new MenuExit();
        }
    }

    internal class MenuPlayTransferFactory : IMenuFactory
    {
        public IMenuMode Create()
        {
            return new MenuPlayTransfer();
        }
    }
}
