using UnityEngine;
using System.Collections.Generic;

using Battle.Cycle;

namespace Battle.Board {    
    public class RngManager : MonoBehaviour {
        private List<ManaColor> bag; // = new List<ManaColor>();
        // reason for only init on start or method call on this instance: each instance (player) should needs to its own list
        private int CenterMatchCallCount = 0;

        void Start() {
            if (bag == null) bag = new List<ManaColor>();
        }

        private void RefillBag()
        {
            if (bag == null) bag = new List<ManaColor>();

            // generate the next piece colors with 2x bag, where x is uniqsue cycle colors
            // create the unsorted list with 2 of each color
            bag.Clear();
            for (int i = 0; i < (ManaCycle.lockPieceColors ? ManaCycle.cycleUniqueColors : 5); i++)
            {
                bag.Add(( (ManaColor) i));
                bag.Add(( (ManaColor) i));
            }
            // Debug.Log(string.Join(",",newBag));

            Utils.Shuffle(bag);
        }

        // pull the next color from bag
        public ManaColor PullColorFromBag()
        {
            if (bag == null) bag = new List<ManaColor>();

            // fill bag if empty
            if (bag.Count == 0) RefillBag();
            
            ManaColor pulledColor = bag[0];
            bag.RemoveAt(0);
            return pulledColor;
        }
    

        // returns the cycle colors in order, increasing each time called
        public ManaColor GetCenterMatch()
        {
            CenterMatchCallCount += 1;
            return ManaCycle.cycle[(CenterMatchCallCount-1) % ManaCycle.cycle.Count];
        }
    }
}