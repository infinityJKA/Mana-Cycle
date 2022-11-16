using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Color of this title. (Public read but no public write)
    public ManaColor color;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Randomizes this tile's own color.
    // Intended to be called after the prefab with this script is cloned.
    public void Randomize()
    {
        color = (ManaColor) Random.Range(1,6);
    }
}
