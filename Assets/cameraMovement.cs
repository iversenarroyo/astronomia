using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovements : MonoBehaviour
{
    //public float speed = 5.0f;
    public float sensitivity = 1.0f;
    public GameObject EquatorialGrid;
    public GameObject ConstellationLines;
    public GameObject Ground;

    // Start is called before the first frame update
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the camera based on the mouse movement
        float rotX = -Input.GetAxis("Vertical")*sensitivity;
        float rotY = Input.GetAxis("Horizontal")*sensitivity;
        transform.eulerAngles += new Vector3(rotX, rotY, 0);

        //Toogle Equatorial Grid visibility:
        if (Input.GetKeyDown(KeyCode.E)) EquatorialGrid.SetActive(!EquatorialGrid.activeSelf);
        //Toogle Constellation Lines visibility:
        if (Input.GetKeyDown(KeyCode.L)) ConstellationLines.SetActive(!ConstellationLines.activeSelf);        
        //Toogle Ground visibility:
        if (Input.GetKeyDown(KeyCode.G)) Ground.SetActive(!Ground.activeSelf);
    }
}
