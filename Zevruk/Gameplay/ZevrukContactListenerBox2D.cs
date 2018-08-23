using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace Zevruk
{
    internal class ZevrukContactListener : ContactListener
    {
        public delegate void EventListener(ContactPoint point);
        public delegate void EventResultListener(ContactResult point);
        public event EventListener OnAdd;
        public event EventListener OnPersist;
        public event EventListener OnRemove;
        public event EventResultListener OnResult;

        public override void Add(ContactPoint point)
        {
            this.OnAdd?.Invoke(point);
            base.Add(point);
        }

        public override void Persist(ContactPoint point)
        {
            this.OnPersist?.Invoke(point);
            base.Persist(point);
        }

        public override void Remove(ContactPoint point)
        {
            this.OnRemove?.Invoke(point);
            base.Remove(point);
        }

        public override void Result(ContactResult point)
        {
            this.OnResult?.Invoke(point);
            base.Result(point);
        }
    }
}
