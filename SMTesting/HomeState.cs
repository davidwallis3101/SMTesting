using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
//using Abodit.StateMachine;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Abodit.StateMachine;
using Abodit.Units;
using log4net;

namespace SMTesting
{
    // A comment

    /// <summary>
    /// An Occupancy State machine handles not occupied, occupied, asleep
    /// </summary>
    [Serializable]
    public class OccupancyStateMachine : StateMachine<OccupancyStateMachine, Event, BuildingArea>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(OccupancyStateMachine));

        [NonSerialized] private readonly Subject<State> watch = new Subject<State>();

        public IObservable<State> Watch
        {
            get { return watch.AsObservable(); }
        }

        public override void OnStateChanging(StateMachine<OccupancyStateMachine, Event, BuildingArea>.State newState, BuildingArea context)
        {
            Trace.WriteLine("Entered state " + newState);
            watch.OnNext(newState);
        }

        [NonSerialized] private readonly Subject<Event> watchEvents = new Subject<Event>();

        public IObservable<Event> WatchEvents
        {
            get { return watchEvents.AsObservable(); }
        }

        //public override void OnEventHappened(Event @event)
        //{
        //    watchEvents.OnNext(@event);
        //}

        //public static readonly State Starting = AddState("Starting");

        // machine m , event e, 
        public static readonly State NotOccupied = AddState("Not occupied",
            (m, e, s, c) =>
            {
                m.CancelScheduledEvent(eTick); // Stop the clock
                //c.IsTimerRunning.Current = false;
                //c.IsHeavilyOccupied.Current = false;
                m.After(new TimeSpan(0, 30, 0), eHalfHourPostOccupancy);
                m.After(new TimeSpan(0, 60, 0), eOneHourPostOccupancy);
            },
            (m, e, s, c) => { });

        public static readonly State Asleep = AddState("Asleep",
            (m, e, s, c) =>
            {
                // Set a timer going for morning
                var now = TimeProvider.Current.Now.LocalDateTime;
                var morning = now.Hour < 8 ? now.AddHours(-now.Hour + 8) : now.AddHours(24 - now.Hour + 8);
                m.CancelScheduledEvent(eMorning);
                m.At(morning.ToUniversalTime(), eMorning);
            },
            (m, e, s, c) => { },
            parent: NotOccupied);

        public static readonly State Occupied = AddState("Occupied",
            (m, e, s, c) =>
            {
                m.CancelScheduledEvent(eTimeout);
                //m.After(c.OccupancyTimeout, eTimeout);                // start a new timeout
                //c.IsTimerRunning.Current = true;

                // Add a timer that runs while we are occupied
                m.CancelScheduledEvent(eTick); // remove any old eTick events
                m.Every(new TimeSpan(hours: 0, minutes: 0, seconds: 10), eTick);

                // And kill any post occupancy timers
                m.CancelScheduledEvent(eHalfHourPostOccupancy);
                m.CancelScheduledEvent(eOneHourPostOccupancy);
                m.CancelScheduledEvent(eMorning);
            },
            (m, e, s, c) => { });

        /// <summary>
        /// Room is occupied and all doors into it are enclosed
        /// </summary>
        public static readonly State OccupiedAndEnclosed = AddState("Occupied and enclosed",
            (m, e, s, c) => { },
            (m, e, s, c) => { },
            parent: Occupied);

        private static readonly Event eStart = new Event("Starts");
        private static readonly Event eUserActivity = new Event("User activity");

        private static readonly Event eSensorActivity = new Event("Sensor activity")
            ; // maintains occupancy, does not set it

        private static readonly Event eTick = new Event("Tick"); // 10s tick while occupied

        public static readonly Event eDoorOpens = new Event("Door opens"); // At least one door opened
        public static readonly Event eDoorCloses = new Event("Door closes"); // All doors closed

        private static readonly Event eTimeout = new Event("Timeout");
        private static readonly Event eMorning = new Event("Morning");

        private static readonly Event eTimeoutHalfHourPostOccupancy = new Event("Timeout Half Hour");
        private static readonly Event eTimeoutOneHourPostOccupancy = new Event("Timeout One Hour");

        private static readonly Event eAllChildrenNotOccupied = new Event("No child occupied");
        private static readonly Event eAtLeastOneChildOccupied = new Event("At least one child occupied");

        // Public events that we expose
        public static readonly Event eHalfHourPostOccupancy = new Event("Half hour post occupancy");
        public static readonly Event eOneHourPostOccupancy = new Event("One hour post occupancy");

        // TODO: Implement a queue to figure out how occupied the room is

        /// <summary>
        /// A static constructor in your state machine is where you define it.
        /// That way it is only ever defined once per program activation.
        /// 
        /// Each transition you define takes as an argument the state machine instance (m),
        /// the state (s) and the event (e).
        /// 
        /// </summary>
        static OccupancyStateMachine()
        {



            // m = machine
            // s = state
            // e = event
            // c = context (BuildingArea)


        // Note: This is a hierarchical state machine so NotOccupied includes Asleep
            NotOccupied
                .When(eAtLeastOneChildOccupied, Occupied)
                .When(eDoorOpens, Occupied)
                .When(eUserActivity, (m, s, e, c) =>
                    {

                        //if (c.Enclosed.Current)
                        if(c.Enclosed)
                            return OccupiedAndEnclosed;
                        else
                            return Occupied;
                    }
                );

            // Asleep is a substate of not occupied so no need for more logic on becoming occupied ...
            Asleep
                .When(eMorning, NotOccupied);

            // Occupied includes recently occupied and heavily occupied ...
            Occupied
                .When(eUserActivity, (m, s, e, c) =>
                {
                    m.CancelScheduledEvent(eTimeout);                       // cancel the old timeout
                    m.After(c.OccupancyTimeout, eTimeout);                  // start a new timeout
                    //c.IsTimerRunning.Current = true;
                    c.IsTimerRunning = true;
                    if (c.Enclosed)
                       // if (c.Enclosed.Current)
                        return OccupiedAndEnclosed;
                    else
                        return s;
                })
                .When(eSensorActivity, (m, s, e, c) =>
                {
                    m.CancelScheduledEvent(eTimeout);                       // cancel the old timeout
                    m.After(c.OccupancyTimeout, eTimeout);                  // start a new timeout
                    c.IsTimerRunning = true;
                    //c.IsTimerRunning.Current = true;
                    return s;
                })
                .When(eDoorCloses, (m, s, e, c) =>
                {
                    // Tricky, it closes but unless there is motion who knows what state we are in????
                    return s;
                })
                .When(eAllChildrenNotOccupied, (m, s, e, c) =>
                {
                    //if (c.IsTimerRunning.Current)
                    if (c.IsTimerRunning)
                    {
                        // If the timer is running ... wait until it runs out
                        return s;
                    }
                    else
                    {
                        //// recursion??? m.EventHappens(eTimeout, c);
                        //// otherwise ...
                        //if (c.Time.EveningTo6AM.On)
                        //    return Asleep;
                        //else
                            c.AddToLog("Not Occupied");
                            return NotOccupied;
                    }
                })
                .When(eTick, (m, s, e, c) =>
                {
                    return s;
                })
                .When(eTimeout, (m, s, e, c) =>
                {
                    c.IsTimerRunning = false;
                    // c.IsTimerRunning.Current = false;
                    // No action if we have occupied children
                    // if (c.HasOccupiedChildren.Current)
                    c.AddToLog("No action if we have occupied children");
                    if (c.HasOccupiedChildren)
                        return s;

                    // No timeout if occupied and enclosed ???
                    c.AddToLog("No timeout if occupied and enclosed ???");
                    if (s == OccupiedAndEnclosed)
                    {
                        c.AddToLog("Kept occupied because enclosed");
                        m.After(c.OccupancyTimeout, eTimeout);                  // start a new timeout
                        return s;
                    }

                    //if (c.Time.EveningTo6AM.On)
                    //    return Asleep;
                    //else
                    return NotOccupied;
                }); 

            OccupiedAndEnclosed
                .When(eDoorOpens, (m, s, e, c) =>
                {
                    return Occupied;
                })
                .When(eTick, (m, s, e, c) =>
                {
                    return s;
                });
        }


        /*
            http://blog.abodit.com/2016/05/home-automation-states/

            Not occupied
            Not occupied and the residents are out for the evening
            Not occupied and the residents have gone on vacation
            Occupied
            Occupied but everyone is asleep
            Occupied and there are dinner guests
            Occupied and there is a party going on
            Occupied and there are guests staying over

    */


        public OccupancyStateMachine() : base(NotOccupied)
        {
        }

        public override void Start()
        {
            Console.WriteLine("Starting state machine");
            this.EventHappens(eStart, null);
        }

        public void UserActivity(BuildingArea buildingArea)
        {
            var was = this.CurrentState.ToString();
            this.EventHappens(eUserActivity, buildingArea);
            Console.WriteLine("User activity " + was + " -> " + this.CurrentState.ToString());
        }

        public void DoorOpens(BuildingArea buildingArea) { this.EventHappens(eDoorOpens, buildingArea); }
        public void DoorCloses(BuildingArea buildingArea) { this.EventHappens(eDoorCloses, buildingArea); }

        public void SensorMaintainingActivity(BuildingArea buildingArea)
        {
            var was = this.CurrentState.ToString();
            this.EventHappens(eUserActivity, buildingArea);
            Console.WriteLine("Sensor maintaining occupancy state " + was + " -> " + this.CurrentState.ToString());
        }

        public void AllChildrenNotOccupied(BuildingArea buildingArea)
        {
            var was = this.CurrentState.ToString();
            // buildingArea.HasOccupiedChildren.Current = false;
            this.EventHappens(eAllChildrenNotOccupied, buildingArea);
            Console.WriteLine("All children not occupied " + was + " -> " + this.CurrentState.ToString());
        }

        public void AtLeastOneChildOccupied(BuildingArea buildingArea)
        {
            var was = this.CurrentState.ToString();
            // buildingArea.HasOccupiedChildren.Current = true;
            buildingArea.HasOccupiedChildren = true;
            this.EventHappens(eAtLeastOneChildOccupied, buildingArea);
            Console.WriteLine("At least one child occupied " + was + " -> " + this.CurrentState.ToString());
        }

        public override string ToString()
        {
            return "Occupancy state: " + this.CurrentState.Name;
        }


        /// <summary>
        /// An event has happened, transition to next state
        /// </summary>
        //public void EventHappens(TEvent @event)
        //{
        //    if (this.CurrentState == null)
        //    {
        //        var initialStateIfNotSet = definitions.First().Value;
        //        throw new NullReferenceException("You forgot to set the initial State, maybe you wanted to use " +
        //                                         initialStateIfNotSet);
        //    }
        //    var newState = this.CurrentState.OnEvent((TStateMachine)this, @event);
        //    if (newState != null && newState != this.CurrentState)
        //    {
        //        this.OnStateChanging(newState);
        //        this.CurrentState = newState;
        //        if (StateChanges != null)
        //            StateChanges(this.CurrentState);
        //    }
        //}

    }
}

