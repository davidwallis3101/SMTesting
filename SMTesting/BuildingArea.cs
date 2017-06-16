using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SMTesting;

namespace SMTesting
{


    /*
     * Home contains Floors, Floors contain Rooms as 'Children'.
     * Natural language can address any BuildingArea in a rule so I can type "Kitchen off" or "Basement on".
     * BUT...I'm in the process of doing away with the simple 'List<Room> Children' approach and moving to a graph where "contains" is just one possible relationship.
     * 
     * The house contains Floors, Floors contain Rooms, Rooms contain lights and sensors.
     * 
     * I’ve been migrating to a directed graph instead of a tree
     * 
     * relationships like ‘connected to’, ‘open to’, ‘audible from’, ‘visible from’, ‘above’, ‘below'
     * 
     * Angular.js application for the house. This new page uses d3.js and webcola to render a force-directed graph layout.
     * 
     * 
     *  class Home : BuildingArea
     *  class Floor : BuildingArea
     *  class Room : BuildingArea
     */




    // Home contains Floors, Floors contain Rooms as 'Children'.


    public class BuildingArea
    {
        public BuildingArea()
        {
            Children = new List<BuildingArea>();
            OccupancyTimeout = new TimeSpan(0,0,0,5);
        }

        public string Name { get; set; }

        // Default occupancy timeout 
        public TimeSpan OccupancyTimeout { get; set; }

        public void AddToLog(string msg)
        {
            Console.WriteLine("{0} {1}", DateTime.Now, msg);
        }
        
        public bool Enclosed { get; set; }

        public bool IsTimerRunning { get; set; }

        public bool HasOccupiedChildren { get; set; }
        //{
        // get { return Children.Any(child => child.HasOccupiedChildren); }
        //}

    public List<BuildingArea> Children { get; set; }
    }

    public class Home : BuildingArea
    {
        public new List<Floor> Children { get; set; }

        public Home()
        {
            Children = new List<Floor>();
        }

    }

    public class Floor : BuildingArea
    {
        public new List<Room> Children { get; set; }

        public Floor()
        {
            Children = new List<Room>();
        }
    }

    public class Room : BuildingArea
    {
        // Add lights / Devices / Sensors?

        // Add People?
    }

    public class Garden : BuildingArea
    {

 
    };





}
