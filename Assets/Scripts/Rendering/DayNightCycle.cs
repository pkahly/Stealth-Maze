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
    public float realSecondsToGameMinute = 1f;

    private Config config;
    private int hour;
    private int minute;
    private bool isDaytime;    

    void Start() {
        config = Config.GetInstance();

        hour = 0;
        minute = 0;
        isDaytime = false;
        clockText.text = "";

        if (config.enableDayNightCycle) {
            StartCoroutine(TrackTimeOfDay());
        }
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
            if (hour >= config.sunrise && hour < config.sunset && !isDaytime) {
                Debug.Log("Switch to daytime");

                isDaytime = true;
                sun.intensity = daytimeIntensity;
                RenderSettings.skybox = daySkybox;
            } else if ((hour >= config.sunset || hour < config.sunrise) && isDaytime) {
                Debug.Log("Switch to nightime");

                isDaytime = false;
                sun.intensity = nightimeIntensity;
                RenderSettings.skybox = nightSkybox;
            }

            // If we are near the boundry, use LERP
            if (isDaytime && config.sunset - hour == 1) {
                Debug.Log("Sunset: " + minute);
                sun.intensity = Mathf.Lerp(daytimeIntensity, nightimeIntensity, (minute / 60.0f));
            } else if (!isDaytime && config.sunrise - hour == 1) {
                Debug.Log("Sunrise: " + minute);
                sun.intensity = Mathf.Lerp(nightimeIntensity, daytimeIntensity, (minute / 60.0f));
            }

            // Update overlay
            if (config.showClock) {
                clockText.text = hour + ":" + minute;
            }

            yield return new WaitForSeconds(realSecondsToGameMinute);
        }
    }

    public bool IsDaytime() {
        return isDaytime;
    }
}
