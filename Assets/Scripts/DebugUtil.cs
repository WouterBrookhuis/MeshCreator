using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

public class DebugUtil
{
    public class StopwatchData
    {
        public Stopwatch m_stopwatch;
        public int m_count;
        public long m_totalTime;
    }

    private static Dictionary<string, StopwatchData> m_stopwatches = new Dictionary<string, StopwatchData>();

    public static void StartStopwatch(string name)
    {
        if(!m_stopwatches.ContainsKey(name))
        {
            var item = new StopwatchData();
            m_stopwatches.Add(name, item);
            item.m_stopwatch = new Stopwatch();
        }
        m_stopwatches[name].m_stopwatch.Start();
    }

    public static void EndStopwatch(string name)
    {
        var item = m_stopwatches[name];
        item.m_stopwatch.Stop();
        UnityEngine.Debug.Log(name + " finished in " + item.m_stopwatch.ElapsedMilliseconds + "ms");
        item.m_totalTime += item.m_stopwatch.ElapsedMilliseconds;
        item.m_stopwatch.Reset();
    }
}
