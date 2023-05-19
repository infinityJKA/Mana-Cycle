using UnityEngine;
using UnityEngine.UI;
using System;

namespace Animation {
    public class MaterialTextureScroll : MonoBehaviour {
        [SerializeField] private Vector2 scrollDirection;
        public Vector2 scrollOffset = new Vector2(0,0);


        void Update() {
            // update position of objects
            rt.anchoredPosition = new Vector2((scrollDirection.x*Time.time ) % rt.rect.width + scrollOffset.x, (scrollDirection.y*Time.time) % rt.rect.height + scrollOffset.y );
        }


        Image image;
        RectTransform rt;

        void Start() {
            image = GetComponent<Image>();
            rt = GetComponent<RectTransform>();
            // Debug.Log(rt.rect.width);

            // create 3 additional grid images with offsets to make scroll seamless
            if (name != "CLONE")
            {
                GameObject o1 = Instantiate(gameObject, gameObject.transform.parent.gameObject.transform);
                o1.GetComponent<MaterialTextureScroll>().scrollOffset = new Vector2(-rt.rect.width * Math.Abs(scrollDirection.x)/scrollDirection.x,0);
                o1.name = "CLONE";
    
                GameObject o2 = Instantiate(gameObject, gameObject.transform.parent.gameObject.transform);
                o2.GetComponent<MaterialTextureScroll>().scrollOffset = new Vector2(-rt.rect.width * Math.Abs(scrollDirection.x)/scrollDirection.x,-rt.rect.height * Math.Abs(scrollDirection.y)/scrollDirection.y);
                o2.name = "CLONE";

                GameObject o3 = Instantiate(gameObject, gameObject.transform.parent.gameObject.transform);
                o3.GetComponent<MaterialTextureScroll>().scrollOffset = new Vector2(0,-rt.rect.height * Math.Abs(scrollDirection.y)/scrollDirection.y);
                o3.name = "CLONE";
            }


        }
    }
}