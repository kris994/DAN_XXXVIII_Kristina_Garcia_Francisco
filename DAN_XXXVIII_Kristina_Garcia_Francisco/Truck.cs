using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        /// Locks the trucks loading to be one by one
        /// </summary>
        private readonly object lockLoading = new object();
        /// <summary>
        /// Locks the countdown
        /// </summary>
        private readonly object lockerCuntdown = new object();
        /// <summary>
        /// Locks the truck thread
        /// </summary>
        private readonly object lockTruck = new object();
        /// <summary>
        /// Barrier for checking if all 10 trucks finished loading
        /// </summary>     
        private Barrier loadingBarrier = new Barrier(10);
        /// <summary>
        /// Wait for a truck to get an arrival time so another can get it too
        /// </summary>
        private EventWaitHandle waitArrivalTime = new AutoResetEvent(false);
        /// <summary>
        /// Loading countdown event for loading
        /// </summary>
        private CountdownEvent countdownLoading = new CountdownEvent(2);
        /// <summary>
        /// Loading countdown event for assigning routes
        /// </summary>
        private CountdownEvent countdownRoutes = new CountdownEvent(1);
        /// <summary>
        /// Generate random numbers when needed
        /// </summary>
        private Random rng = new Random();
        /// <summary>
        /// Counter increases as threads enter
        /// </summary>
        private int enterCounter = 0;
        /// <summary>
        /// Shows a whiteline in console and then it counts the number of routes
        /// </summary>
        private int counter = 10;
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

            // Amount fo threads in countdown
            MultipleThreads(countdownLoading);

            Console.WriteLine("Truck {0} started loading.", Thread.CurrentThread.Name);
            lock (lockLoading)
            {
                enterCounter++;
                waitTime = rng.Next(500, 5001);

                allLoadingTime.Add(waitTime, Thread.CurrentThread.Name);
                Monitor.Wait(lockLoading, waitTime);
            }
            // Write the time it took to load
            if (allLoadingTime.Any(tr => tr.Value.Equals(Thread.CurrentThread.Name)))
            {
                Console.WriteLine("Truck {0} finished loading after {1} milliseconds."
                    , Thread.CurrentThread.Name, allLoadingTime.FirstOrDefault(x => x.Value == Thread.CurrentThread.Name).Key);
            }
            
            lock (lockTruck)
            {
                enterCounter--;
                if (enterCounter == 0)
                {
                    countdownLoading.Reset(2);                    
                }
            }

            // Put a white line for console
            lock (lockTruck)
            {
                counter--;
                if (counter == 0)
                {
                    Console.WriteLine("\nDestination:");
                }
            }

            // Wait all threads to finish loading
            loadingBarrier.SignalAndWait();
        }
        #endregion

        #region Routing
        /// <summary>
        /// Give routes to each thread
        /// </summary>
        public void TruckRouting()
        {
            // Amount fo threads in countdown
            MultipleThreads(countdownRoutes);
            Console.WriteLine("\t\t\t\t\t\t\t\tTruck {0} received route {1}", Thread.CurrentThread.Name, Manager.truckRoutes[counter]);
            counter++;
            countdownRoutes.Reset(1);

            // Set the first truck on wait to pass
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
                    int unloadingTime = Convert.ToInt32(allLoadingTime.FirstOrDefault(x => x.Value == Thread.CurrentThread.Name).Key / 1.5);
                    Console.WriteLine("Truck {0} arrived, unloading time {1} milliseconds."
                        , Thread.CurrentThread.Name, unloadingTime);
                    Monitor.Wait(lockTruck, unloadingTime);
                    Console.WriteLine("\t\t\t\t\t\t\t\tTruck {0} successfully unloaded.", Thread.CurrentThread.Name);
                }
            }
        }
        #endregion

        #region Manipulating truck threads
        /// <summary>
        /// Only let a fixed amount of threads to enter with countdown
        /// </summary>
        /// <param name="cd">the chosent countdown</param>
        public void MultipleThreads(CountdownEvent cd)
        {
            // Countdown amount
            while (true)
            {
                lock (lockerCuntdown)
                {
                    if (cd.CurrentCount == 0)
                    {
                        cd.Wait();
                        Thread.Sleep(0);
                    }
                    else
                    {
                        cd.Signal();                        
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
