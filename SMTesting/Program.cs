using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.Reactive;
using System.Timers;
using Abodit.StateMachine;

namespace SMTesting
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            var stateMachine = new OccupancyStateMachine();
            var house = Home();
            
            stateMachine.StateChanges += new StateMachine<OccupancyStateMachine, Event, BuildingArea>.ChangesStateDelegate(testStateChange.demoStateMachine_StateChanges);

            var obsvr = Observer.Create<OccupancyStateMachine.State>(s=> { Console.WriteLine("Observer: {0}", s.ToString()); });
            stateMachine.Watch.Subscribe(obsvr);


            stateMachine.Start();



            // Serialise the statemachine
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(stateMachine.GetType());
            //x.Serialize(Console.Out, stateMachine);
            //Console.WriteLine();

            //garden.OccupancyTimeout = new TimeSpan(hours: 1, minutes: 0, seconds: 30);
            var office = house.Children.Find(b => b.Name == "Upstairs").Children.Find(b => b.Name == "Office");

            var hme = house.Children.Find(b => b.Name == "House");
            stateMachine.DoorOpens(office);
            stateMachine.DoorOpens(hme); 


            //var test = new StringWithHistory {Current = "initial value"};
            //Console.WriteLine("test value - current: '{0}' prev: '{1}'", test.Current, test.Previous);
            //test.Current = "new Value";
            //Console.WriteLine("test value - current: '{0}' prev: '{1}'", test.Current, test.Previous);

            stateMachine.DoorCloses(office);

            //    stateMachine.DoorOpens(garden);

            //stateMachine.SensorMaintainingActivity(house);
            //stateMachine.AtLeastOneChildOccupied(house);

            //stateMachine.Tick(DateTime.UtcNow.AddDays(1), garden);

            Console.WriteLine("Sleeping");
            System.Threading.Thread.Sleep(15000);

            

        Console.WriteLine("Press a key");
        Console.ReadKey();
    }




        private static BuildingArea Home()
        {
            var house = new BuildingArea();

            var floors = new List<Floor>
            {
                new Floor {Name = "Upstairs"},
                new Floor {Name = "Downstairs"}
            };

            house.Children.AddRange(floors);

            var upstairsRooms = new List<Room>
            {
                new Room {Name = "Front Bedroom"},
                new Room {Name = "Back Bedroom"},
                new Room {Name = "Office"},
                new Room {Name = "Bathroom"}
            };

            var upstairsFloor = house.Children.FirstOrDefault(f => f.Name == "Upstairs");
            if (upstairsFloor != null) upstairsFloor.Children.AddRange(upstairsRooms);

            var downstairsRooms = new List<Room>
            {
                new Room {Name = "Living Room"},
                new Room {Name = "Kitchen"},
                new Room {Name = "Downstairs Toilet"}
            };

            var downStairsFloor = house.Children.FirstOrDefault(f => f.Name == "DownStairs");
            if (downStairsFloor != null) downStairsFloor.Children.AddRange(downstairsRooms);
            return house;
        }
    }
}
