using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// handles the star bg in gauntlet mode menu
public class GauntletBGObject : MonoBehaviour
{
    public float speed = 0;

    void Start()
    {
        transform.localPosition = new Vector3(Random.Range(50, -750), Random.Range(0, 450), 0);
    }

    void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x - speed * Time.deltaTime, transform.localPosition.y, 0);
        if (transform.localPosition.x <= -800)
        {
            transform.localPosition = new Vector3(50, Random.Range(0, 450), 0);
        }
    }
}
