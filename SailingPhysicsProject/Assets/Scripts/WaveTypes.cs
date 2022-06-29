using System.Collections;
using UnityEngine;

//Create different types of waves
public class WaveTypes
{
    // Sin waves
    public static float SinXWave(
        Vector3 position,
        float speed,
        float scale,
        float waveDistance,
        float noiseStrength,
        float noiseWalk,
        float timeSinceStart
        )
    {
        float x = position.x;
        float y = 0f;
        float z = position.z;

        //only X or Y will produce straight waves, only Y will give up/down movement
        //put em together (X+y+Z) to get rolling waves!
        // X*z gives a moving sea but no rolling waves

        float waveType = z;

        y += Mathf.Sin((timeSinceStart * speed + waveType) / waveDistance) * scale;

        //add noise for realism
        y += Mathf.PerlinNoise(x + noiseWalk, y + Mathf.Sin(timeSinceStart * 0.1f)) * noiseStrength;

        return y;
    }
}
