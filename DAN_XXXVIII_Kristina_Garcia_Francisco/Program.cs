using System;

namespace DAN_XXXVIII_Kristina_Garcia_Francisco
{
    /// <summary>
    /// The main program class
    /// </summary>
    class Program
    {
        /// <summary>
        /// The main method
        /// </summary>
        /// <param name="args">main arguments</param>
        static void Main(string[] args)
        {
            Manager manager = new Manager();
            Truck truck = new Truck();

            manager.CreateWorkers();
            truck.CreateTrucks();

            Console.ReadKey();
        }
    }
}
