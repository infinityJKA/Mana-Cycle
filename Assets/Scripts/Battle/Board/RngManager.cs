using UnityEngine;
using System.Collections.Generic;

using Battle.Cycle;
using System;

namespace Battle.Board {    
    public class RngManager : MonoBehaviour {
        public System.Random rng;

        private List<int> bag; // = new List<ManaColor>();
        // reason for only init on start or method call on this instance: each instance (player) should needs to its own list
        private int CenterMatchCallCount = 0;

        void Start() {
            if (bag == null) bag = new List<int>();
        }

        public void SetSeed(int seed) {
            rng = new System.Random(seed);
        }

        private void RefillBag()
        {
            if (bag == null) bag = new List<int>();

            // generate the next piece colors with 2x bag, where x is uniqsue cycle colors
            // create the unsorted list with 2 of each color
            bag.Clear();
            for (int i = 0; i < (ManaCycle.lockPieceColors ? ManaCycle.cycleUniqueColors : 5); i++)
            {
                bag.Add(i);
                bag.Add(i);
            }
            // Debug.Log(string.Join(",",newBag));

            Utils.Shuffle(bag, rng);
        }

        // pull the next color from bag
        public int PullColorFromBag()
        {
            if (bag == null) bag = new List<int>();

            // fill bag if empty
            if (bag.Count == 0) RefillBag();
            
            int pulledColor = bag[0];
            bag.RemoveAt(0);
            return pulledColor;
        }
    

        // returns the cycle colors in order, increasing each time called
        public int GetCenterMatch()
        {
            CenterMatchCallCount += 1;
            return ManaCycle.cycle[(CenterMatchCallCount-1) % ManaCycle.cycle.Length];
        }
    }
}