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
    public float daytimeIntensity = 1.5f;
    public float nightimeIntensity = 0.05f;

    private int hour;
    private int minute;
    private bool isDaytime;

    private int sunrise = 8;
    private int sunset = 20;
    private float realSecondsToGameMinute = 1f;

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
                sun.intensity = nightimeIntensity;
                RenderSettings.skybox = nightSkybox;
            }

            // If we are near the boundry, use LERP
            if (isDaytime && Mathf.Abs(sunset - hour) <= 1) {
                sun.intensity = Mathf.Lerp(daytimeIntensity, nightimeIntensity, (minute / 60.0f));
            } else if (!isDaytime && Mathf.Abs(sunrise - hour) <= 1) {
                sun.intensity = Mathf.Lerp(nightimeIntensity, daytimeIntensity, (minute / 60.0f));
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
