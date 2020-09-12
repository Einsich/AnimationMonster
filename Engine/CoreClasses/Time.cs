using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Engine
{
    static class Time
    {
        static Stopwatch stopwatch;
        public static float time { get; private set; }
        public static float deltaTime { get; private set; }
        public static void Update()
        {
            if(stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            float t = stopwatch.ElapsedMilliseconds * 0.001f;
            deltaTime = t - time;
            time = t;
        }
    }
}
