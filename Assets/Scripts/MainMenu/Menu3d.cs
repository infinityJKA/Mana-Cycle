using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu3d : MonoBehaviour
{
    [SerializeField] private GameObject HTPObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectVersus()
    {
        SceneManager.LoadScene("CharSelect");
    }

    public void SelectHTP()
    {
        HTPObj.SetActive(true);
    }
}
