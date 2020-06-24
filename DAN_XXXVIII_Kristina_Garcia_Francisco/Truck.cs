using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAN_XXXVIII_Kristina_Garcia_Francisco
{
    /// <summary>
    /// Creates trucks and represents all actions trucks have to do
    /// </summary>
    class Truck
    {
        #region Properties
        /// <summary>
        /// List of all active trucks
        /// </summary>
        private List<Thread> allTrucks = new List<Thread>();
        /// <summary>
        /// Saving each trucks time it took to load
        /// </summary>
        private Dictionary<int, string> allLoadingTime = new Dictionary<int, string>();
        /// <summary>
        /// Locks the trucks arrival
        /// </summary>
        private readonly object lockTruck = new object();
        /// <summary>
        /// Barrier for checking if all 10 trucks finished loading
        /// </summary>
        private Barrier loadingBarrier = new Barrier(10);
        private EventWaitHandle waitTruck = new AutoResetEvent(false);
        private EventWaitHandle waitArrivalTime = new AutoResetEvent(false);
        /// <summary>
        /// Generate random numbers when needed
        /// </summary>
        private Random rng = new Random();
        /// <summary>
        /// Counter increases as threads enter
        /// </summary>
        private int enterCounter = 0;
        /// <summary>
        /// Restarts the thread counts
        /// </summary>
        private int restartThreadCount = 0;
        /// <summary>
        /// Counts the amount of routes that were given to threads
        /// </summary>
        private int routeCounter = 0;
        #endregion

        /// <summary>
        /// All actions that a truck needs to do
        /// </summary>
        public void TruckActions()
        {
            TruckLoading();
            TruckRouting();
            TruckArriving();
        }

        #region Loading
        /// <summary>
        /// Load 2 trucks at the same time, each loading takes a random amount of time
        /// </summary>
        public void TruckLoading()
        {
            int waitTime;

            // Amount fo threads that can enter the semaphore
            MultipleThreads(2);

            Console.WriteLine("Truck {0} started loading.", Thread.CurrentThread.Name);

            // Threads trying to pass
            waitTruck.WaitOne();           
            waitTime = rng.Next(500, 5001);
            allLoadingTime.Add(waitTime, Thread.CurrentThread.Name);
            // Let the next thread in
            waitTruck.Set();
            Thread.Sleep(waitTime);
            // Write the time it took to load
            if (allLoadingTime.Any(tr => tr.Value.Equals(Thread.CurrentThread.Name)))
            {
                Console.WriteLine("Truck {0} finished loading after {1} milliseconds."
                    , Thread.CurrentThread.Name, allLoadingTime.FirstOrDefault(x => x.Value == Thread.CurrentThread.Name).Key);
            }

            // Let more threads enter the semaphore
            restartThreadCount--;
            if (restartThreadCount == 0)
            {
                enterCounter = 0;
            }

            // Wait all threads to finish loading
            loadingBarrier.SignalAndWait();

            // Let first truck waiting for the route pass
            waitTruck.Set();
        }
        #endregion

        #region Routing
        /// <summary>
        /// Give routes to each thread
        /// </summary>
        public void TruckRouting()
        {
            // All trucks waiting for a route
            waitTruck.WaitOne();
            Console.WriteLine("\t\t\t\t\t\t\t\tTruck {0} received route {1}", Thread.CurrentThread.Name, Manager.truckRoutes[routeCounter]);
            routeCounter++;
            // Let the next truck pass
            waitTruck.Set();
            // Let the first truck that needs arrival time pass
            waitArrivalTime.Set();
        }
        #endregion

        #region Arrival
        /// <summary>
        /// Calculates the trucks arrival time
        /// </summary>
        public void TruckArriving()
        {
            int arrivalTime;
            waitArrivalTime.WaitOne();
            arrivalTime = rng.Next(500, 5000);
            Console.WriteLine("Truck {0} expected arrival time {1} milliseconds."
                , Thread.CurrentThread.Name, arrivalTime);
            enterCounter++;
            waitArrivalTime.Set();
            Thread.Sleep(arrivalTime);
            ArrivalActions(arrivalTime);
        }

        /// <summary>
        /// Depending on the time a truck took to arrive, different actions will be done
        /// </summary>
        /// <param name="arrivalTime">the time it took for the truck to arrive</param>
        public void ArrivalActions(int arrivalTime)
        {
            lock (lockTruck)
            {
                if (arrivalTime > 3000)
                {
                    Console.WriteLine("Truck {0} delivery canceled. " +
                    "Return time {1} milliseconds.", Thread.CurrentThread.Name, arrivalTime);
                    Monitor.Wait(lockTruck, arrivalTime);
                    Console.WriteLine("\t\t\t\t\t\t\t\tTruck {0} successfully returned.", Thread.CurrentThread.Name);
                }
                else
                {
                    int unloadingTime = allLoadingTime.FirstOrDefault(x => x.Value == Thread.CurrentThread.Name).Key;
                    Console.WriteLine("Truck {0} arrived, unloading time {1} milliseconds."
                        , Thread.CurrentThread.Name, unloadingTime / 2);
                    Monitor.Wait(lockTruck, unloadingTime / 2);
                    Console.WriteLine("\t\t\t\t\t\t\t\tTruck {0} successfully unloaded.", Thread.CurrentThread.Name);
                }
            }
        }
        #endregion

        #region Manipulating truck threads
        /// <summary>
        /// Only let a fixed amount of threads to enter
        /// </summary>
        /// <param name="amount">the fixed amount</param>
        public void MultipleThreads(int amount)
        {
            // Allow only 2 threads at the same time
            while (true)
            {
                lock (lockTruck)
                {
                    enterCounter++;

                    if (enterCounter > amount)
                    {
                        Thread.Sleep(0);
                    }
                    else
                    {
                        // Set for 1st thread to pass
                        waitTruck.Set();
                        restartThreadCount++;
                        break;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Creates 10 different truck threads and starts them at the same time
        /// </summary>
        public void CreateTrucks()
        {
            for (int i = 1; i < 11; i++)
            {
                Thread truckThreads = new Thread(TruckActions)
                {
                    Name = "Truck_" + i
                };
                allTrucks.Add(truckThreads);
            }

            foreach (var item in allTrucks)
            {
                item.Start();
            }
        }
    }
}
