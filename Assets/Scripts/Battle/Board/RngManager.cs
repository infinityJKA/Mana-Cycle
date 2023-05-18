using UnityEngine;
using System.Collections.Generic;

using Battle.Cycle;

namespace Battle.Board {    
    public class RngManager : MonoBehaviour {
        private List<ManaColor> bag = new List<ManaColor>();
        private int CenterMatchCallCount = 0;

        void Start() {
            // bag = new List<ManaColor>();
        }

        private void RefillBag()
        {
            // generate the next piece colors with 2x bag, where x is unique cycle colors
            // create the unsorted list with 2 of each color
            bag.Clear();
            for (int i = 0; i < ManaCycle.cycleUniqueColors; i++)
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