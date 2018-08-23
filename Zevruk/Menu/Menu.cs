using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Zevruk
{
    internal class Menu : IGameMode
    {
        protected bool disposed;
        private Background background = new Background();
        private IMenuMode menuMode = new MenuMain();

        ~Menu()
        {
            this.Dispose(false);
        }

        public IMenuMode MenuMode
        {
            get => this.menuMode;
            set => this.menuMode = value;
        }

        public void Change(IGameMode mode)
        {
            Major.Instance.Mode = mode;
            this.Dispose();
        }

        public void Handle(RenderWindow window)
        {
            this.background.Draw(window);
            this.menuMode.Handle(window, this);
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
                this.background.Dispose();
                this.menuMode.Dispose();
            }
            
            this.disposed = true;
        }
    }
}
