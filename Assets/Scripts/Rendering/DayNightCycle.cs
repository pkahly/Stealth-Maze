using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public Material daySkybox;
    public Material nightSkybox;
    public Text clockText;
    public float daytimeIntensity = 2.5f;

    private int hour;
    private int minute;
    private bool isDaytime;

    private int sunrise = 8;
    private int sunset = 20;
    private int realSecondsToGameMinute = 1;

    void Start()
    {
        hour = 0;
        minute = 0;
        isDaytime = false;

        StartCoroutine(TrackTimeOfDay());
    }

    IEnumerator TrackTimeOfDay() {
        while (true) {
            // Update Time
            minute++;

            if (minute >= 60) {
                hour++;
                minute -= 60;
            }

            if (hour >= 24) {
                hour = 0;
            }

            // Set Light Intensity
            if (hour >= sunrise && hour < sunset && !isDaytime) {
                isDaytime = true;
                sun.intensity = daytimeIntensity;
                RenderSettings.skybox = daySkybox;
            } else if ((hour >= sunset || hour < sunrise) && isDaytime) {
                isDaytime = false;
                sun.intensity = 0;
                RenderSettings.skybox = nightSkybox;
            }

            // Update overlay
            clockText.text = hour + ":" + minute;

            yield return new WaitForSeconds(realSecondsToGameMinute);
        }
    }

    public bool IsDaytime() {
        return isDaytime;
    }
}
