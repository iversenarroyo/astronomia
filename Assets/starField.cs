/*
starsBuild.cs for the Virtual Observatory project in Unity 3D.
Released under MIT License. Copyright (c) 2023 João Teles and Vitor Coluci.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/*
Class for parsing the Yale Star Catalog http://tdc-www.harvard.edu/catalogs/bsc5.html
*/
public class StarBSC5
{
    public string name = "";
    public string hd = "";
    public string sao = "";
    public float ra = 0.0f;
    public float dec = 0.0f;
    public float mag = 0.0f;
    float toRad = (float)Math.PI/180.0f;
    public StarBSC5(string starline)
    {
        name = starline.Substring(5, 9);
        hd = starline.Substring(25, 6);
        sao = starline.Substring(31, 6);        
        ra = raToRad(starline.Substring(75, 8));
        dec = decToRad(starline.Substring(83, 7));
        mag = float.Parse(starline.Substring(103, 3));
    }
    public float raToRad(string raS)
    {
        string[] raA = raS.Split(' ');
        float hour = float.Parse(raS.Substring(0, 2));
        float min = float.Parse(raS.Substring(2, 2));
        float sec = float.Parse(raS.Substring(4, 4));
        return (hour + min/60.0f + sec/3600.0f)*toRad*360.0f/24.0f;
    }
    public float decToRad(string decS)
    {        
        float sig = float.Parse(decS.Substring(0, 1) + "1");
        float deg = float.Parse(decS.Substring(1, 2));        
        float min = float.Parse(decS.Substring(3, 2));
        float sec = float.Parse(decS.Substring(5, 2));
        float decDeg = deg + min/60.0f + sec/3600.0f;
        return (90.0f - sig*decDeg)*toRad;
    }
}

public class starField : MonoBehaviour
{    
    ParticleSystem starsSystem;        
    ParticleSystem.Particle[] starsArray;
    StarBSC5[] stars;        
    public float brilho = 100.0f;   
    public float db = 2.0f;
    float maxBrilho = 200.0f;

    public StarBSC5[] loadBSC5Catalog(string starsFilename)
    {           
        int linesNumber = File.ReadAllLines(starsFilename).Length;
        StarBSC5[] starsObjArray = new StarBSC5[linesNumber];        
        StreamReader starsFile = new StreamReader(starsFilename);        
        int j = 0;
        for (int i = 0; i < linesNumber; i++) {            
            string starLine = starsFile.ReadLine();
            if (starLine.Substring(60, 3) != "   ") {
                starsObjArray[j++] = new StarBSC5(starLine);
            }
        }
        starsFile.Close();
        Array.Resize(ref starsObjArray, j);
        return starsObjArray;
    }
    
    void Start()
    {           
        starsSystem = GetComponent<ParticleSystem>();
        stars = loadBSC5Catalog("Assets/Data/bsc5.dat");
        starsArray = new ParticleSystem.Particle[stars.Length];        
        var ss = starsSystem.main;
        ss.maxParticles = stars.Length;
        ss.startLifetime = Mathf.Infinity; 
        ss.loop = false;      
        ss.startDelay = 0;
        ss.startSpeed = 0;   
        var emission = starsSystem.emission;
        emission.enabled = false; 
        starsSystem.Emit(stars.Length);        
        starsSystem.GetParticles(starsArray);
        for (int i = 0; i < stars.Length; i++) {      
            float y = Vars.radius*Mathf.Sin(stars[i].dec)*Mathf.Cos(stars[i].ra);
            float x = Vars.radius*Mathf.Sin(stars[i].dec)*Mathf.Sin(stars[i].ra);
            float z = Vars.radius*Mathf.Cos(stars[i].dec);
            starsArray[i].remainingLifetime = Mathf.Infinity;
            starsArray[i].position = new Vector3(x, y, z);
            starsArray[i].startSize = brilho*Mathf.Exp(-stars[i].mag/2);
            //starsArray[i].startColor = Color.white*Mathf.Exp(-stars[i].mag);
        }                  
        starsSystem.SetParticles(starsArray);
    }
    
    void Update()
    {
        //Increase and decrease stars brightness keyboard commands:
        if (Input.GetKeyDown(KeyCode.M))
        {            
            brilho = Mathf.Min(maxBrilho, brilho + db);
            for (int i = 0; i < stars.Length; i++)
            {
                starsArray[i].startSize = brilho*Mathf.Exp(-stars[i].mag/2);
            }
            starsSystem.SetParticles(starsArray);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {            
            brilho = Mathf.Max(0.0f, brilho - db);
            for (int i = 0; i < stars.Length; i++)
            {
                starsArray[i].startSize = brilho*Mathf.Exp(-stars[i].mag/2);
            }
            starsSystem.SetParticles(starsArray);
        }
    }
}
