using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zevruk
{
    internal static class AngleConversion
    {
        public static float DegreeToRadian(float val) => val * (float)System.Math.PI / 180;

        public static float RadianToDegree(float val) => val * 180 / (float)System.Math.PI;
    }
}
