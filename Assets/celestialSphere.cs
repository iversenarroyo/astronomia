/*
celestialSphere.cs for the Virtual Observatory project in Unity 3D.
Released under MIT License. Copyright (c) 2023 João Teles and Vitor Coluci.

Stars and Solar System bodies motion equations extracted from the work of Paul Schlyter
available at https://stjarnhimlen.se/comp/ppcomp.html
P.S.: All orbital parameters and correction terms in degrees where converted to radians.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Vars
{    
    public static float radius = 1000.0f;  //celestial sphere radius
    public static float angleResolution = 5.0f; //angular resolution for arc discretization [degrees]    
}

public class Sun
{       
    public float w, M;
    public float xs, ys;
    public float ra, dec;
    public float ecl;
    public Sun (float day)
    {
        w = 4.93824157f + 8.219366E-7f*day; //[radians]
        float a = 1.000000f; //[AU]
        float e = 0.016709f - 1.151E-9f*day;
        M = 6.214192441f + 0.01720196962f*day; //[radians]
        ecl = 0.40909296f - 6.218608e-9f*day; //[radians]

        float E = M + e*Mathf.Sin(M)*(1.0f + e*Mathf.Cos(M));
        float xv = a*Mathf.Cos(E) - e;
        float yv = a*Mathf.Sqrt(1.0f - e*e)*Mathf.Sin(E);
        float v = Mathf.Atan2(yv, xv);
        float r = Mathf.Sqrt(xv*xv + yv*yv);
        float lonsun = v + w;
        xs = r*Mathf.Cos(lonsun);
        ys = r*Mathf.Sin(lonsun);
        float xe = xs;
        float ye = ys*Mathf.Cos(ecl);
        float ze = ys*Mathf.Sin(ecl);        
        ra = Mathf.Atan2(ye, xe)*Mathf.Rad2Deg;
        dec = Mathf.Atan2(ze, Mathf.Sqrt(xe*xe+ye*ye))*Mathf.Rad2Deg;
    }
    public Sun Copy()
    {
       return (Sun) this.MemberwiseClone();
    }
}

public class EclipticCoords
{    
    public float lon, lat, r;
    public EclipticCoords (float day, float N, float i, float w, float a, float e, float M)
    {
        float E = eccentricAnomaly(M, e);
        float xv = a*(Mathf.Cos(E) - e);
        float yv = a*Mathf.Sqrt(1.0f - e*e)*Mathf.Sin(E);
        float v = Mathf.Atan2(yv, xv);
        r = Mathf.Sqrt(xv*xv + yv*yv);
        float xh = r*(Mathf.Cos(N)*Mathf.Cos(v+w) - Mathf.Sin(N)*Mathf.Sin(v+w)*Mathf.Cos(i));
        float yh = r*(Mathf.Sin(N)*Mathf.Cos(v+w) + Mathf.Cos(N)*Mathf.Sin(v+w)*Mathf.Cos(i));
        float zh = r*Mathf.Sin(v+w)*Mathf.Sin(i);
        lon = Mathf.Atan2(yh, xh);
        lat = Mathf.Atan2(zh, Mathf.Sqrt(xh*xh + yh*yh));
    }
    float eccentricAnomaly(float M, float e)
    {
        int maxIter = 100;
        float E0 = M + e*Mathf.Sin(M)*(1.0f + e*Mathf.Cos(M));
        float diff = 1.0f;
        int iter = 0;
        while (diff > 1.0e-5f && iter++ < maxIter)
        {
            float E1 = E0 - (E0 - e*Mathf.Sin(E0) - M)/(1.0f - e*Mathf.Cos(E0));
            diff = Mathf.Abs(E1 - E0);
            E0 = E1;            
        }
        return E0;
    }
}

public class EquatorialFromEcliptic
{     
    public float ra, dec;
    public EquatorialFromEcliptic (EclipticCoords eclC, Sun sun)
    {                        
        float xh = eclC.r*Mathf.Cos(eclC.lon)*Mathf.Cos(eclC.lat);
        float yh = eclC.r*Mathf.Sin(eclC.lon)*Mathf.Cos(eclC.lat);
        float zh = eclC.r*Mathf.Sin(eclC.lat);
        
        float xg = xh + sun.xs;
        float yg = yh + sun.ys;
        float zg = zh;
        
        float xe = xg;
        float ye = yg*Mathf.Cos(sun.ecl) - zg*Mathf.Sin(sun.ecl);
        float ze = yg*Mathf.Sin(sun.ecl) + zg*Mathf.Cos(sun.ecl);

        ra = Mathf.Atan2(ye, xe);
        dec = Mathf.Atan2(ze, Mathf.Sqrt(xe*xe + ye*ye));
    }
}

public class RectFromEquatorial
{
    public float x, y, z;
    public RectFromEquatorial(float ra, float dec)
    {        
        y = Mathf.Sin(Mathf.PI/2.0f - dec)*Mathf.Cos(ra);
        x = Mathf.Sin(Mathf.PI/2.0f - dec)*Mathf.Sin(ra);
        z = Mathf.Cos(Mathf.PI/2.0f - dec);
    }
}

public class Moon
{     
    public float ra, dec;
    public Moon (Sun sun, float day, float LST, float latitude)
    {
        float N = 2.18380483f - 9.242183063049e-4f*day;  //[radians]
        float i = 0.08980417f; //[radians]
        float w = 5.55125356f + 2.8685764238965e-3f*day; //[radians]
        float a = 60.2666f; //(Earth radii)
        float e = 0.054900f;
        float M = 2.013506073f + 0.228027143743f*day; //[radians]

        float Ls = sun.M + sun.w;
        float L = M + w + N;
        float D = L - Ls;
        float F = L - N;
        float lon_corr =
            -2.224e-2f*Mathf.Sin(M - 2.0f*D)
            +1.148e-2f*Mathf.Sin(2.0f*D)
            -3.246e-3f*Mathf.Sin(sun.M)
            -1.030e-3f*Mathf.Sin(2.0f*M - 2.0f*D)
            -9.948e-4f*Mathf.Sin(M - 2.0f*D + sun.M)
            +9.250e-4f*Mathf.Sin(M + 2.0f*D)
            +8.029e-4f*Mathf.Sin(2.0f*D - sun.M)
            +7.156e-4f*Mathf.Sin(M - sun.M)
            -6.109e-4f*Mathf.Sin(D)
            -5.411e-4f*Mathf.Sin(M + sun.M)
            -2.618e-4f*Mathf.Sin(2.0f*F - 2.0f*D)
            +1.920e-4f*Mathf.Sin(M - 4.0f*D);
        float lat_corr =
            -3.019e-3f*Mathf.Sin(F - 2.0f*D)
            -9.599e-4f*Mathf.Sin(M - F - 2.0f*D)
            -8.029e-4f*Mathf.Sin(M + F - 2.0f*D)
            +5.760e-4f*Mathf.Sin(F + 2.0f*D)
            +2.967e-4f*Mathf.Sin(2.0f*M + F);
        float r_corr = -0.58f*Mathf.Cos(M - 2.0f*D) -0.46f*Mathf.Cos(2.0f*D);
        
        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        eclC.lon += lon_corr;
        eclC.lat += lat_corr;
        eclC.r += r_corr;
        
        Sun sunAsEarth = sun.Copy();
        sunAsEarth.xs = 0.0f;
        sunAsEarth.ys = 0.0f;

        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sunAsEarth);

        //Topocentric correction for paralax correction:
        float toRad = Mathf.PI/180.0f;
        float mpar = Mathf.Asin(1.0f/eclC.r);
        float gclat = latitude*toRad - 0.1924f*Mathf.Sin(2.0f*latitude*toRad)*toRad;
        float rho = 0.99833f + 0.00167f*Mathf.Cos(2.0f*latitude*toRad);
        float HA = LST*toRad*360.0f/24.0f - eq.ra;
        float g = Mathf.Atan(Mathf.Tan(gclat)/Mathf.Cos(HA));
        ra = eq.ra - mpar*rho*Mathf.Cos(gclat)*Mathf.Sin(HA)/Mathf.Cos(eq.dec);
        dec = eq.dec - mpar*rho*Mathf.Sin(gclat)*Mathf.Sin(g - eq.dec)/Mathf.Sin(g);
    }
}

public class Mercury
{         
    public float ra, dec;
    public Mercury (Sun sun, float day)
    {
        float N = 8.43540317e-1f + 5.66511186e-7f*day;  //[radians]
        float i = 1.22255078e-1f + 8.72664626e-10f*day;  //[radians]
        float w = 5.08311437e-1f + 1.77053181e-7f*day;  //[radians]
        float a = 0.387098f; //(AU)
        float e = 0.205635f + 5.59E-10f*day;
        float M = 2.94360599e0f + 7.14247100149e-2f*day;  //[radians]

        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sun);
        ra = eq.ra;
        dec = eq.dec;
    }
}

public class Venus
{         
    public float ra, dec;
    public Venus (Sun sun, float day)
    {
        float N = 1.33831673e0f + 4.30380740e-7f*day;  //[radians];
        float i = 5.92469468e-2f + 4.79965544e-10f*day;  //[radians];
        float w = 9.58028680e-1f + 2.41508190e-7f*day;  //[radians];
        float a = 0.723330f; //(AU)
        float e = 0.006773f - 1.302E-9f*day;
        float M = 8.37848798e-1f + 2.79624475e-2f*day;  //[radians];

        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sun);
        ra = eq.ra;
        dec = eq.dec;
    }
}

public class Mars
{         
    public float ra, dec;
    public Mars (Sun sun, float day)
    {
        float N = 8.64939799e-1f + 3.68405844e-7f*day;  //[radians];
        float i = 3.22833552e-2f - 3.10668607e-10f*day;  //[radians];
        float w = 5.00039623e0f + 5.11313403e-7f*day;  //[radians];
        float a = 0.723330f; //(AU)
        float e = 0.006773f - 1.302E-9f*day;
        float M = 3.24667893e-1f + 9.14588790e-3f*day;  //[radians];

        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sun);
        ra = eq.ra;
        dec = eq.dec;
    }
}

public class Jupiter
{     
    public float ra, dec;
    public Jupiter (Sun sun, float day)
    {
        float N = 1.75325654e0f + 4.83201385e-7f*day;  //[radians]
        float i = 2.27416402e-2f - 2.71747765e-9f*day;  //[radians]
        float w = 4.78006761e0f + 2.87115389e-7f*day;  //[radians]
        float a = 5.20256f; //(Earth radii)
        float e = 0.048498f + 4.469E-9f*day;
        float M = 3.47233255e-1f + 1.450112046752574e-3f*day;  //[radians]

        float Ms = 5.53211777e0f + 5.837118978783366e-4f*day;  //[radians]  //Saturn
        
        float lon_corr =
            -5.794e-3f*Mathf.Sin(2*M - 5*Ms - 1.180f)
            -9.774e-4f*Mathf.Sin(2*M - 2*Ms + 0.367f)
            +7.330e-4f*Mathf.Sin(3*M - 5*Ms + 0.367f)
            -6.283e-4f*Mathf.Sin(M - 2*Ms)
            +3.840e-4f*Mathf.Cos(M - Ms)
            +4.014e-4f*Mathf.Sin(2*M - 3*Ms + 0.908f)
            -2.793e-4f*Mathf.Sin(M - 5*Ms - 1.204f);  //[radians]
        
        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        eclC.lon += lon_corr;        
        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sun);
        ra = eq.ra;
        dec = eq.dec;
        //Debug.Log($"Lon: {eclC.lon}, Lat: {eclC.lat}, day: {day}");
    }
}

public class Saturn
{     
    public float ra, dec;
    public Saturn (Sun sun, float day)
    {
        float N = 1.98380057e0f + 4.17098785e-7f*day;  //[radians]
        float i = 4.34342638e-2f - 1.88670092e-9f*day;  //[radians]
        float w = 5.92354102e0f + 5.19516450e-7f*day;  //[radians]
        float a = 5.20256f; //(Earth radii)
        float e = 0.048498f + 4.469E-9f*day;        
        float M = 5.53211777e0f + 5.837118978783366e-4f*day;  //[radians]
        
        float Mj = 3.47233255e-1f + 1.450112046752574e-3f*day;  //[radians] Jupiter
        
        float lon_corr =
            +1.417e-2f*Mathf.Sin(2*Mj - 5*M - 1.180e0f)
            -3.997e-3f*Mathf.Cos(2*Mj - 4*M - 3.491e-2f)
            +2.077e-3f*Mathf.Sin(Mj - 2*M - 5.236e-2f)
            +8.029e-4f*Mathf.Sin(2*Mj - 6*M - 1.204e0f)
            +2.443e-4f*Mathf.Sin(Mj - 3*M + 5.585e-1f);  //[radians]
        float lat_corr =
            -3.491e-4f*Mathf.Cos(2*Mj - 4*M - 3.491e-2f)
            +3.142e-4f*Mathf.Sin(2*Mj - 6*M - 8.552e-1f); //[radians]
        
        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        eclC.lon += lon_corr;        
        eclC.lat += lat_corr;
        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sun);
        ra = eq.ra;
        dec = eq.dec;
    }
}

public class Uranus
{     
    public float ra, dec;
    public Uranus (Sun sun, float day)
    {
        float N = 1.29155237e0f + 2.43962123e-7f*day; //[radians]
        float i = 1.34966311e-2f + 3.31612558e-10f*day; //[radians]
        float w = 1.68705620e0f + 5.33459886e-7f*day; //[radians]
        float a = 19.18171f - 1.55E-8f*day;
        float e = 0.047318f + 7.45E-9f*day;
        float M = 2.48867371e0f + 2.04653922e-4f*day; //[radians]
        
        float Mj = 3.47233255e-1f + 1.45011204675e-3f*day;  //[radians] Jupiter
        float Ms = 5.53211777e0f + 5.83711897878e-4f*day;  //[radians] Saturn
        
        float lon_corr =
            +6.981e-4f*Mathf.Sin(Ms - 2*M + 1.047e-1f)
            +6.109e-4f*Mathf.Sin(Ms - 3*M + 5.760e-1f)
            -2.618e-4f*Mathf.Sin(Mj - M + 3.491e-1f); //[radians]
        
        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        eclC.lon += lon_corr;
        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sun);
        ra = eq.ra;
        dec = eq.dec;
        //Debug.Log($"Lon: {eclC.lon}, Lat: {eclC.lat}, day: {day}");
    }
}

public class Neptune
{         
    public float ra, dec;
    public Neptune (Sun sun, float day)
    {
        float N = 2.30000536e0f + 5.26618195e-7f*day; //[radians]
        float i = 3.08923278e-2f - 4.45058959e-9f*day; //[radians]
        float w = 4.76206280e0f - 1.05190994e-7f*day; //[radians]
        float a = 30.05826f + 3.313E-8f*day; // (AU)
        float e = 0.008606f + 2.15E-9f*day;
        float M = 4.54216876e0f + 1.04635054e-4f*day; //[radians]

        EclipticCoords eclC = new EclipticCoords(day, N, i, w, a, e, M);
        EquatorialFromEcliptic eq = new EquatorialFromEcliptic(eclC, sun);
        ra = eq.ra;
        dec = eq.dec;
    }
}

public class celestialSphere : MonoBehaviour
{
    public int year = 2023;
    public int month = 9;
    public int day = 23;
    public int hour = 6;
    public int minute = 0;
    public int second = 0;
    public float latitude = -22.35694f; //local latitude [degrees]
    public float longitude = -47.38417f; //local latitude [degrees]
    float timeIntervalUpdate = 0.1f; //time interval for updating the stars movement [seconds]
    public int timeSpeed = 1;   //time speed multiplier
    public int maxTimeSpeed = 4096;
    public GameObject sunLight, sunFrame;
    public GameObject moonSphere, mercurySphere, venusSphere, marsSphere, jupiterSphere, saturnSphere, uranusSphere, neptuneSphere;
    DateTime lastUpdatedTime, renderingTime;
    DateTime dateJ2000 = new DateTime(1999, 12, 31, 0, 0, 0);
        
    void OnGUI()
    {
        int x0 = 25;
        int y0 = 25;
        int dy = 50;
        int dx = 300;
        GUI.Label(new Rect (x0, y0, dx, dy), $"{renderingTime.ToString("R")}");
    }
    float getDays(DateTime date) {        
        DateTime dateUTC = TimeZoneInfo.ConvertTimeToUtc(date);
        TimeSpan interval = dateUTC - dateJ2000;
        return (float)interval.TotalDays;
    }
    float localSideralTime(DateTime date)
    {        
        DateTime dateUTC = TimeZoneInfo.ConvertTimeToUtc(date);
        TimeSpan interval = dateUTC - dateJ2000;
        float day = (float)interval.TotalDays;
        float w = 282.9404f + 4.70935E-5f*day;        
        float M = 356.0470f + 0.9856002585f*day;        
        float Ls = M + w;
        float hourUTC = dateUTC.Hour + dateUTC.Minute/60.0f + dateUTC.Second/3600.0f;
        return Ls/15.0f + 12.0f + hourUTC + longitude/15.0f;
    }
    void UpdateCelestialSphere()
    {
        float days = getDays(renderingTime);
        float LST = localSideralTime(renderingTime);

        Sun sun = new Sun(days);
        sunLight.transform.localRotation = Quaternion.Euler(sun.dec, -sun.ra, 0.0f);
        sunFrame.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);        
        
        Moon moon = new Moon(sun, days, LST, latitude);
        RectFromEquatorial coord = new RectFromEquatorial(moon.ra, moon.dec);
        moonSphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;

        Mercury mercury = new Mercury(sun, days);
        coord = new RectFromEquatorial(mercury.ra, mercury.dec);
        mercurySphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;

        Venus venus = new Venus(sun, days);
        coord = new RectFromEquatorial(venus.ra, venus.dec);
        venusSphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;

        Mars mars = new Mars(sun, days);
        coord = new RectFromEquatorial(mars.ra, mars.dec);
        marsSphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;

        Jupiter jupiter = new Jupiter(sun, days);
        coord = new RectFromEquatorial(jupiter.ra, jupiter.dec);
        jupiterSphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;

        Saturn saturn = new Saturn(sun, days);
        coord = new RectFromEquatorial(saturn.ra, saturn.dec);
        saturnSphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;

        Uranus uranus = new Uranus(sun, days);
        coord = new RectFromEquatorial(uranus.ra, uranus.dec);
        uranusSphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;

        Neptune neptune = new Neptune(sun, days);
        coord = new RectFromEquatorial(neptune.ra, neptune.dec);
        neptuneSphere.transform.localPosition = new Vector3(coord.x, coord.y, coord.z)*Vars.radius;
        
        float ang = LST*360.0f/24.0f;        
        gameObject.transform.localRotation = Quaternion.Euler(-latitude, 0.0f, ang);        
    }    
    void Start()
    {
        lastUpdatedTime = DateTime.Now;
        renderingTime = new DateTime(year, month, day, hour, minute, second);        
        UpdateCelestialSphere();
    }
    // Update is called once per frame
    void Update()
    {
        TimeSpan elapsedIntervalUpdate = DateTime.Now - lastUpdatedTime;
        if (elapsedIntervalUpdate.TotalSeconds > timeIntervalUpdate) {
            TimeSpan dt = TimeSpan.FromSeconds(timeSpeed*timeIntervalUpdate);
            renderingTime = renderingTime.Add(dt);            
            UpdateCelestialSphere();
            
            year = renderingTime.Year;
            month = renderingTime.Month;
            day = renderingTime.Day;
            hour = renderingTime.Hour;
            minute = renderingTime.Minute;
            second = renderingTime.Second;
            lastUpdatedTime = DateTime.Now;
        }
        
        //Time speed control:
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (timeSpeed < maxTimeSpeed)
            {
                if (timeSpeed < -1) timeSpeed /= 2;
                else if (timeSpeed > -2 && timeSpeed < 2) timeSpeed += 1;
                else timeSpeed *= 2;
            }            
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (timeSpeed > -maxTimeSpeed)
            {
                if (timeSpeed > 1) timeSpeed /= 2;
                else if (timeSpeed > -2 && timeSpeed < 2) timeSpeed -= 1;
                else timeSpeed *= 2;
            }            
        }
    }
}