using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTesting
{
    class ObjectState
    {


        /*
         Events
            eOnAuto
            eOnManual
            eOffAuto
            eOffManual
            eRoomGoesNotOcuppied

            eDim - action?

        On
            .When(eUserTurnsLightOff, (m,s,e,c) => OffManual)
            .When(eSystemTurnsLightOff, (m,s,e,c) => OffAuto)
            .When(eRoomGoesNotOcuppied, (m,s,e,c) => 
              {
                c.AddToLog($"Starting dim sequence for {c.Name}");
                m.Every(new Timespan(0,0,4 eDim);
                c.DimmingCountdown = DimmingSteps;
                return Dimming;
              });
        Off


        Dimming

        */


    }
}
