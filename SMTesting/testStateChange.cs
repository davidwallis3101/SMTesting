using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abodit.StateMachine;

namespace SMTesting
{
    class testStateChange
    {
        public static void demoStateMachine_StateChanges(StateMachine<OccupancyStateMachine, Event, BuildingArea>.State state)
        {
            var p = state.ParentState;
            if (p != null)
                Console.WriteLine("Parent State: {0}", p.Name);

            Console.WriteLine("New state: {0}", state);
        }
    }
}
