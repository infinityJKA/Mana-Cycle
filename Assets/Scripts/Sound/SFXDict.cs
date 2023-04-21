using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Sound {
    public class SFXDict : MonoBehaviour
    {
        // unity cannot serialize dicts :(
        [Serializable]
        private struct KeyValuePair 
        {
            public string key;
            public AudioClip value;
        }

        
        [Serializable]
        public class sfxDict 
        {
            [SerializeField] private KeyValuePair[] dict;

            // return the array of KeyValuesPairs as an actual dictionary to use dictionary methods on
            public Dictionary<string,AudioClip> asDictionary()
            {
                Dictionary<string,AudioClip> returnDict = new Dictionary<string,AudioClip>();

                for (int i = 0; i < dict.Length; i++)
                {
                    returnDict.Add(dict[i].key, dict[i].value);
                }

                return returnDict;
            }
        }

        [SerializeField] public sfxDict sfxDictionary;
    }
}