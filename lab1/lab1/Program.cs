using System;
using System.Collections.Generic;
using System.Linq;

namespace lab1
{

    public class Generate
    {

        public int[] durations;
        public int[] intervals;

        private readonly int minDurationValue = 5;
        private readonly int maxDurationValue = 11;
        private readonly int minIntervalValue = 2;
        private readonly int maxIntervalValue = 10;

        public void GenValue(int count)
        {
            durations = RandomValuesGenerate(count, minDurationValue, maxDurationValue);
            intervals = RandomValuesGenerate(count, minIntervalValue, maxIntervalValue);
            intervals[0] = 0;
            for (int i = 1; i < intervals.Length; i++)
            {
                intervals[i] += intervals[i - 1];
            }
        }

        private int[] RandomValuesGenerate(int count, int minValue, int maxValue)
        {
            int[] GeneratedArray = new int[count];
            Random Rand = new Random();
            for (int i = 0; i < count; i++)
            {
                GeneratedArray[i] = Rand.Next(minValue, maxValue);
            }
            return GeneratedArray;
        }
    }
    abstract class Planner
    {
        protected int[] waitTime;
        protected int[] durations;
        protected int[] thisDurations;
        protected int[] thisArrivalTimes;
        protected int maxQueueLength;

        public Planner()
        {
            waitTime = new int[0];
            thisArrivalTimes = new int[0];
            thisDurations = new int[0];
        }

        public int MaxQueueLength
        {
            get => maxQueueLength;
        }
        public double AverageWaitTime
        {
            get => Math.Round(this.waitTime.Average(), 2);
        }
        public double AverageTurnoverTime
        {
            get => Math.Round(this.durations.Average() + this.waitTime.Average(), 2);
        }

        public abstract void Planning(int count, int[] durations, int[] intervals);
        public abstract int[] NewIntervals(int[] intervals);
    }

    class RRplanner : Planner
    {
        public override void Planning(int count, int[] durations, int[] intervals)
        {
            this.durations = durations;
            Queue<int> processTime = new Queue<int>();
            int index = 0;
            this.thisDurations = new int[count];
            this.thisArrivalTimes = new int[count];
            Array.Copy(durations, this.thisDurations, count);
            Array.Copy(intervals, this.thisArrivalTimes, count);
            this.maxQueueLength = 0;
            this.waitTime = new int[count];
            int finishedProcess = 0;
            processTime.Enqueue(thisArrivalTimes[0]);
            for (int i = 0; finishedProcess < count; i++)
            {
                if (index + 1 < count && NewIntervals(thisArrivalTimes)[index + 1] == 0)
                {
                    processTime.Enqueue(index + 1);
                    index++;
                }
                if (processTime.Count > 0)
                {
                    this.maxQueueLength = Math.Max(maxQueueLength, processTime.Count);
                    int indexOfRunningProcess = processTime.Dequeue();
                    if (processTime.Count > 0)
                    {
                        for (int k = 0; k <= processTime.Count; k++)
                        {
                            int process = processTime.Dequeue();
                            this.waitTime[process]++;
                            processTime.Enqueue(process);
                        }
                    }
                    thisDurations[indexOfRunningProcess]--;
                    if (thisDurations[indexOfRunningProcess] > 0)
                    {
                        if (index + 1 < count && NewIntervals(thisArrivalTimes)[index + 1] == 0)
                        {
                            processTime.Enqueue(index + 1);
                            index++;
                        }
                        thisDurations[indexOfRunningProcess]--;
                        if (thisDurations[indexOfRunningProcess] > 0) processTime.Enqueue(indexOfRunningProcess);
                        else { finishedProcess++; }
                    }
                    else { finishedProcess++; }

                }
            }
        }
        public override int[] NewIntervals(int[] thisArrivalTimes)
        {
            for (int i = 0; i < thisArrivalTimes.Length; i++)
            {
                thisArrivalTimes[i]--;
            }
            return thisArrivalTimes;
        }
    }

    class SRTplanner : Planner
    {
        public override void Planning(int count, int[] durations, int[] intervals)
        {
            this.durations = durations;
            List<int> processes = new List<int>();
            int index = 0;
            this.maxQueueLength = 0;
            this.waitTime = new int[count];
            this.thisDurations = new int[count];
            this.thisArrivalTimes = new int[count];
            Array.Copy(durations, this.thisDurations, count);
            Array.Copy(intervals, this.thisArrivalTimes, count);
            int finishedProcess = 0;
            int minTime;
            bool[] activeProcces = new bool[count];
            activeProcces[0] = true;
            processes.Add(0);
            for (int i = 0; finishedProcess < count; i++)
            {
                int remove = -1;
                if (index + 1 < count && NewIntervals(thisArrivalTimes)[index + 1] == 0)
                {
                    processes.Add(index + 1);
                    index++;
                }
                if (processes.Count > 0)
                {
                    this.maxQueueLength = Math.Max(maxQueueLength, processes.Count);
                    minTime = int.MaxValue;
                    foreach (int process in processes)
                    {
                        if (minTime > thisDurations[process])
                        {
                            if (thisDurations[process] != 0)
                            {
                                minTime = thisDurations[process];
                                Array.Clear(activeProcces, 0, activeProcces.Length);
                                activeProcces[process] = true;
                            }
                            else { remove = process; }
                        }
                    }
                    if (remove > -1)
                    {
                        finishedProcess++;
                        processes.Remove(remove);
                    }
                    foreach (int process in processes)
                    {
                        if (activeProcces[process])
                        {
                            thisDurations[process]--;
                        }
                        else waitTime[process]++;
                    }
                }
            }
        }
        public override int[] NewIntervals(int[] thisArrivalTimes)
        {
            for (int i = 0; i < thisArrivalTimes.Length; i++)
            {
                thisArrivalTimes[i]--;
            }
            return thisArrivalTimes;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int n = 40;
            Generate gen = new Generate();
            gen.GenValue(n);
            int[] durations = gen.durations;
            int[] intervals = gen.intervals;
            int[] qqq = durations;
            Console.WriteLine("Entrance time:");
            for (int i = 0; i < intervals.Length; i++)
            {
                Console.Write(intervals[i] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("Executions time:");
            for (int i = 0; i < durations.Length; i++)
            {
                Console.Write(durations[i] + " ");
            }
            Console.WriteLine();

            Planner planner1 = new RRplanner();
            planner1.Planning(n, durations, intervals);
            Console.WriteLine("RR2:\nWaiting - " + planner1.AverageWaitTime + "\n" + "Turnover time - " + planner1.AverageTurnoverTime + "\n" +
                "Max count process in queue - " + planner1.MaxQueueLength);
            Planner planner2 = new SRTplanner();
            planner2.Planning(n, durations, intervals);
            Console.WriteLine("SRT:\nWaiting - " + planner2.AverageWaitTime + "\n" + "Turnover time - " + planner2.AverageTurnoverTime + "\n" +
                "Max count process in queue - " + planner2.MaxQueueLength + "\n");
            Console.ReadKey();
        }
    }
}
