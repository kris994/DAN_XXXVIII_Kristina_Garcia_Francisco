using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DAN_XXXVIII_Kristina_Garcia_Francisco
{
    /// <summary>
    /// Class that creates routes for trucks
    /// </summary>
    class Manager
    {
        #region Property
        /// <summary>
        /// List of all routes
        /// </summary>
        private List<int> allRoutes = new List<int>();
        /// <summary>
        /// List of routes that trucks will take
        /// </summary>
        public static List<int> truckRoutes = new List<int>();
        /// <summary>
        /// Controls the access to route files
        /// </summary>
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        #endregion

        /// <summary>
        /// Reads truck routes from the list
        /// </summary>
        public void ReadChosenRoutes()
        {
            foreach (var item in truckRoutes)
            {
                Console.Write("{0} ", item);
            }
            Console.WriteLine("\nThe trucks can now be loaded.\n");
        }

        /// <summary>
        /// Creates new routes and saves them to the file
        /// </summary>
        public void CreateRoutes()
        {
            ReadWriteFile rwf = new ReadWriteFile();
            Random rng = new Random();
            int timeTaken = rng.Next(0, 3001);

            // Wait for manager to start
            waitHandle.WaitOne();
            Thread.Sleep(timeTaken);
            rwf.WriteToFile();

            // Manager can continue choosing the best routes
            waitHandle.Set();
        }

        /// <summary>
        /// Chooses the the optimal route
        /// </summary>
        public void BestRoutes()
        {
            List<int> distinctValues = allRoutes.Distinct().ToList();
            distinctValues.Sort();

            for (int i = 0; i < distinctValues.Count; i++)
            {
                if (distinctValues[i] % 3 == 0)
                {
                    truckRoutes.Add(distinctValues[i]);
                    if (truckRoutes.Count == 10)
                    {
                        break;
                    }
                }
            }

            Console.Write("\nThe best routes are: ");
            ReadChosenRoutes();

            Console.WriteLine("Loading:");
        }

        /// <summary>
        /// Chooses the possible routes for the time given
        /// </summary>
        public void ChooseRoutes()
        {
            ReadWriteFile rwf = new ReadWriteFile();
            // Counting how long the manager waited for values to be created
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.WriteLine("Manager waiting for routes to be created...");

            // Sleep max 3sec       
            waitHandle.Set();
            waitHandle.WaitOne(3000);
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            int elapsedTime = (ts.Seconds * 1000) + ts.Milliseconds;

            // Get all routes from the file
            rwf.ReadFile(allRoutes);

            if (elapsedTime > 3000)
            {
                Console.WriteLine("\nToo much time has passed, manager will choose from available routes.");
                // If too much time has passed get any 10 routes from the list
                for (int i = 0; i < allRoutes.Count; i++)
                {
                    truckRoutes.Add(allRoutes[i]);
                    if (truckRoutes.Count == 10)
                    {
                        break;
                    }
                }
                Console.Write("\nRoutes: ");
                ReadChosenRoutes();
            }
            else
            {
                Console.WriteLine("\nManager waited for {0} milliseconds for routes to be created.", elapsedTime);
                // Get best 10 routes
                BestRoutes();
            }
        }

        /// <summary>
        /// Creates working threads
        /// </summary>
        public void CreateWorkers()
        {
            Thread getValues = new Thread(CreateRoutes);
            getValues.Start();

            Thread menager = new Thread(ChooseRoutes);
            menager.Start();

            getValues.Join();
            menager.Join();
        }
    }
}
