using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// handles the star bg in gauntlet mode menu
public class GauntletStarBG : MonoBehaviour
{
    // time between spawning bg objects
    [SerializeField] private float spawnInterval;
    [SerializeField] private Vector2 speedRange;
    [SerializeField] private GameObject bgObjectPrefab;
    [SerializeField] public Sprite bgSprite;
    [SerializeField] private List<Color> colors;

    private float refTime;

    private int objectCount;
    [SerializeField] private int maxObjectCount = 50;
    private List<float> speeds;

    // Start is called before the first frame update
    void Start()
    {
        refTime = Time.time;

        for (int i = 0; i < maxObjectCount; i++)
        {
            GauntletBGObject o = Instantiate(bgObjectPrefab, transform).GetComponent<GauntletBGObject>();

            o.speed = Random.Range(speedRange.x, speedRange.y);
            o.GetComponent<RectTransform>().sizeDelta = o.GetComponent<RectTransform>().sizeDelta * (1 - 1 / o.speed * speedRange.y * 0.35f); 

            Image image = o.GetComponent<Image>();
            image.sprite = bgSprite;
            image.color = colors[Random.Range(0, colors.Count - 1)];
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
