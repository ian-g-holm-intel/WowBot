 using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WowLib
{
    public class RageManager
    {
        private ConcurrentDictionary<DateTime, int> rageDictionary = new ConcurrentDictionary<DateTime, int>();
        private double averageRps = 0;
        private int count = 0;
        private bool generatingRage = false;

        public void AddRage(int rage)
        {
            rageDictionary[DateTime.Now] = rage;
        }

        public void Start()
        {
            if(!generatingRage)
            {
                generatingRage = true;
                rageDictionary[DateTime.Now] = 0;
            }
        }

        public void Stop()
        {
            if(generatingRage)
            {
                generatingRage = false;
                var newRps = GetCurrentRagePerSecond();
                averageRps = (newRps + count * averageRps) / (count + 1);
                count++;
                rageDictionary = new ConcurrentDictionary<DateTime, int>();
            }
        }

        private double GetCurrentRagePerSecond()
        {
            if(rageDictionary.IsEmpty)
                return 0;

            double totalRage = rageDictionary.Values.Sum();
            var times = rageDictionary.Keys.OrderBy(k => k);
            return totalRage / (times.Last() - times.First()).TotalSeconds;
        }

        public double GetRagePerSecond()
        {
            if(averageRps == 0)
                return GetCurrentRagePerSecond();
            return averageRps;
        }
    }
}
