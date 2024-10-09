/*
constellationLines.cs for the Virtual Observatory project in Unity 3D.
Released under MIT License. Copyright (c) 2023 João Teles and Vitor Coluci.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/*
Class for parsing the Sky & Telescope individual constellation lines as arc of circunference.
*/
public class ArcSnT
{
    public float ra = 0.0f;
    public float dec = 0.0f;
    public float x = 0.0f;
    public float y = 0.0f;
    public float z = 0.0f;
    static Dictionary<int, Color> colorDict = new Dictionary<int, Color>()
    {
        {1, Color.blue},
        {2, Color.red},
        {3, Color.yellow},
        {4, Color.yellow}
    };
    static Dictionary<int, float> widthDict = new Dictionary<int, float>()
    {
        {1, 0.006f},
        {2, 0.004f},
        {3, 0.002f},
        {4, 0.002f}
    };
    public float width = widthDict[1];
    public Color color = colorDict[1];    
    public ArcSnT(string constellationLine)
    {
        float toRad = (float)Math.PI/180.0f;
        ra = float.Parse(constellationLine.Substring(6, 8))*toRad*360.0f/24.0f;
        dec = float.Parse(constellationLine.Substring(15, 8))*toRad;        
        y = Mathf.Sin(dec)*Mathf.Cos(ra);
        x = Mathf.Sin(dec)*Mathf.Sin(ra);
        z = Mathf.Cos(dec);
        int w = Int32.Parse(constellationLine.Substring(28, 1));
        width = widthDict[w];
        color = colorDict[w];
    }
}

/*
Class for parsing the Sky & Telescope file SnT_constellations used by Stellarium from:
https://github.com/Stellarium/stellarium/tree/master/skycultures/modern_st
*/
public class ConstellationsSnT
{
    public Dictionary<string, ArcSnT[]> dict = new Dictionary<string, ArcSnT[]>();
    Dictionary<string, string> dictS = new Dictionary<string, string>();
    public ConstellationsSnT(string constellationsFilename)
    {
        int linesNumber = File.ReadAllLines(constellationsFilename).Length;        
        StreamReader constFile = new StreamReader(constellationsFilename);        
        string earlierName = "";
        string sep;
        for (int i = 0; i < linesNumber; i++)
        {
            string constLine = constFile.ReadLine();
            if (constLine.Substring(0, 1) != "#")
            {
                string name = constLine.Substring(28, 4);
                if (!dictS.ContainsKey(name)) dictS.Add(name, constLine);                
                else {
                    if (earlierName != name) sep = "|";
                    else sep = ",";
                    dictS[name] += sep + constLine;
                }
                earlierName = name;
            }
        }        
        foreach (KeyValuePair<string, string> kvp in dictS)
        {
            string name = kvp.Key;
            string[] lines = kvp.Value.Split('|');
            int N = lines.Length;            
            for (int j = 0; j < N; j++)
            {
                string[] line = lines[j].Split(',');
                int M = line.Length;
                ArcSnT[] arcsObjArray = new ArcSnT[M];
                for (int k = 0; k < M; k++)
                {
                    arcsObjArray[k] = new ArcSnT(line[k]);
                }
                dict.Add(name + j.ToString(), arcsObjArray);   
            }            
        }
        constFile.Close();
    }
}

public class constellationLines : MonoBehaviour
{   
    public GameObject gmoArcSnT; 
    
    void Start()
    {
        ConstellationsSnT consts = new ConstellationsSnT("Assets/Data/SnT_constellations.dat");
        LineRenderer arc = gmoArcSnT.AddComponent<LineRenderer>();
        arc.material = new Material(Shader.Find("Sprites/Default"));            
        arc.loop = false;
        arc.useWorldSpace = false;
                        
        foreach (KeyValuePair<string, ArcSnT[]> kvp in consts.dict) 
        {                           
            int N = kvp.Value.Length;                    
            for (int j = 0; j < N - 1; j++)
            {
                float r = Vars.radius;
                Vector3 v0 = new Vector3(kvp.Value[j].x, kvp.Value[j].y, kvp.Value[j].z)*r;
                Vector3 v1 = new Vector3(kvp.Value[j+1].x, kvp.Value[j+1].y, kvp.Value[j+1].z)*r;
                float ang = Vector3.Angle(v0, v1);
                int na = Math.Max(1, (int)Math.Round((ang*180.0f/3.1416f)/Vars.angleResolution)) + 1;
                var p = new Vector3[na];
                for (int k = 0; k < na; k++)
                {
                    float t = (float)k/(na-1);
                    p[k] = Vector3.Slerp(v0, v1, t);                    
                }                
                LineRenderer iarc = Instantiate(arc, gameObject.transform, false);                
                iarc.positionCount = na;
                iarc.startColor = kvp.Value[j].color;
                iarc.endColor = kvp.Value[j].color;   
                iarc.widthMultiplier = kvp.Value[j].width*r;     
                iarc.SetPositions(p);
            }
        }
        Destroy(gmoArcSnT);
    }
    void Update() {        
    }
}

