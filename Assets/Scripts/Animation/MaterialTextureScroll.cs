using UnityEngine;
using UnityEngine.UI;

namespace Animation {
    public class MaterialTextureScroll : MonoBehaviour {
        [SerializeField] private Vector2 scrollDirection;


        void Update() {
            image.material.mainTextureOffset = scrollDirection*Time.time;
        }


        Image image;

        void OnValidate() {
            image = GetComponent<Image>();
        }
    }
}