using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManyLotto
{
    class Control
    {
        protected const int MAX_PICK_NUMS = 5;
        protected static long userChoice;
        static void Main(string[] args)
        {
            Console.Write("Runs: ");
            userChoice = Convert.ToInt64(Console.ReadLine());

            Simulation simulation = new Simulation();
            simulation.StartSimulation();

            Console.Write("Done..");
            Console.ReadLine();
        }
    }
}
