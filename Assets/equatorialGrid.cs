/*
equatorialGrid.cs for the Virtual Observatory project in Unity 3D.
Released under MIT License. Copyright (c) 2023 João Teles and Vitor Coluci.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equatorialGrid : MonoBehaviour
{
    public Color raColor = Color.blue;
    public Color decColor = Color.red;
    public int raDivisions = 11;
    public int decDivisions = 10;    
    public float width = 0.002f;
    public GameObject gmoCircle; 
    
    void Start()
    {
        //Base circle construction:
        LineRenderer circle = gmoCircle.AddComponent<LineRenderer>();
        circle.material = new Material(Shader.Find("Sprites/Default"));
        circle.widthMultiplier = width*Vars.radius;        
        circle.loop = true;
        circle.useWorldSpace = false;        
        int circleSegments = Mathf.RoundToInt(360.0f/Vars.angleResolution);
        circle.positionCount = circleSegments;                
        var points = new Vector3[circleSegments];
        for (int i = 0; i < circleSegments; i++)
        {
            float t = i*2*Mathf.PI/circleSegments;
            points[i] = new Vector3(Vars.radius*Mathf.Cos(t), 0, Vars.radius*Mathf.Sin(t));
        }                
        circle.SetPositions(points);      
        
        //RA circles instantiation:
        for (int i = 1; i < raDivisions; i++)
        {
            LineRenderer raCircle = Instantiate(circle, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(0, 0, i*180/raDivisions));
            raCircle.startColor = raColor;
            raCircle.endColor = raColor;        
            raCircle.transform.SetParent(gameObject.transform, false);
        }

        //Dec circles instantiation:        
        for (int i = 1; i < decDivisions; i++)
        {
            float d = i*Mathf.PI/decDivisions;
            float r = Vars.radius*Mathf.Sin(d);
            float z = Vars.radius*Mathf.Cos(d);
            var p = new Vector3[circleSegments];
            for (int j = 0; j < circleSegments; j++)
            {
                float t = j*2*Mathf.PI/circleSegments;                                
                p[j] = new Vector3(r*Mathf.Cos(t), r*Mathf.Sin(t), z);
            }            
            LineRenderer decCircle = Instantiate(circle, gameObject.transform, false);
            decCircle.startColor = decColor;
            decCircle.endColor = decColor;        
            decCircle.SetPositions(p);
        }     

        Destroy(gmoCircle);
    }

    void Update()
    {
    }
}
