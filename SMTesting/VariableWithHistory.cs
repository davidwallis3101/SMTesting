using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Abodit.Units;
using Abodit.Utility;

namespace SMTesting
{

    //So I created a class called VariableWithHistory 
    //which is the abstract base class for IntegerWithHistory, DoubleWithHistory, BoolWithHistory, StringWithHistory and a number of others
    // Setting the .Current value stores both the value and the DateTime (Utc of course)
    // A history of all past values is maintained in MongoDB up to some suitable limit per variable (each variable can have its own adjustable history size in bytes by using MongoDB’s capped collections).

    //If the new value is the same as the old one no update is made, the implicit behavior being that the value changed and stayed there until it changes again
    //, so if you want to know what the value is now it is the same as the last change recorded.

    public class VariableWithHistory<T>
    {
        //http://blog.abodit.com/2013/02/variablewithhistory-making-persistence-invisible-making-history-visible/
        private T _current;
       
        public T Current
        {
            get
            {
                return _current;
            }
            set
            {
                Previous = _current;
                _current = value;
            }
        }

        public T Previous { get; private set; }

        public DateTime LastUpdated { get; set; }

        public int CountTransitions(DateTimeRange range, T direction)
        {
            //Counts how many transitions there have been to the value T in a given time range, 
            //eg. how many times did the driveway alarm go ‘true’ this evening?
            throw new NotImplementedException();
        }

        public DateTimeOffset LastChangedState => new DateTimeOffset(LastUpdated);

        public TimedValue<T> ValueAtTime { get; }
    }
    

    public class StringWithHistory: VariableWithHistory<string>
    {

    }

    public class BoolWithHistory : VariableWithHistory<bool>
    {
        
        public double PercentageTrue(DateTimeRange range)
        {
            throw new NotImplementedException();
        }
    }
}
