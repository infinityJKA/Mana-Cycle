using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    public class TextureOffsetScroll : MonoBehaviour
    {
        [SerializeField] private Vector2 speed;
        [SerializeField] private Material mat;

        // Update is called once per frame
        void Update()
        {
            mat.mainTextureOffset = new Vector2(speed.x * Time.time, speed.y * Time.time);
        }
    }
}

