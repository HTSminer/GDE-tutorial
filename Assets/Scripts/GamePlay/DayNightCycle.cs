using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public TimeOfDay dayOrNight;
    public bool modifyAmbientLight = false;
    public bool debugIgnoreTime = false;
    public bool debugCustomTime = false;
    public float customTime = 12f;
    public float customTimeScale = 0f;
    public Color filter = Color.white;
    
    private Light2D lightSource;
    private Color[] skyColors;

    public static DayNightCycle i { get; private set; }

    void Awake()
    {
        i = this;
        lightSource = GetComponent<Light2D>(); 
    }

    void Start()
    {
        InitializeSkyColors(); 
        StartCoroutine(UpdateDayNightCycle());
    }

    void InitializeSkyColors()
    {
        skyColors = new Color[]
        {
            // Define colors for each hour of the day
            // [0-23] corresponds to the hour of the day
            // You can adjust these colors as needed
            //            new Color(0.14f, 0.20f, 0.47f, 1),
            new Color(0.14f, 0.20f, 0.47f, 1),
            new Color(0.14f, 0.20f, 0.47f, 1),
            new Color(0.14f, 0.20f, 0.47f, 1),
            new Color(0.47f, 0.51f, 0.86f, 1),
            new Color(0.47f, 0.51f, 0.86f, 1),
            new Color(0.63f, 0.75f, 1, 1),
            new Color(0.63f, 0.75f, 1, 1),
            new Color(0.63f, 0.75f, 1, 1),
            new Color(0.63f, 0.75f, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(1, 1, 1, 1),
            new Color(0.94f, 0.67f, 0.43f, 1),
            new Color(0.94f, 0.67f, 0.43f, 1),
            new Color(0.47f, 0.51f, 0.86f, 1),
            new Color(0.24f, 0.31f, 0.67f, 1),
            new Color(0.24f, 0.31f, 0.67f, 1),
            new Color(0.14f, 0.20f, 0.47f, 1)

        };
    }

    IEnumerator UpdateDayNightCycle()
    {
        while (true)
        {
            UpdateTime(); 

            UpdateLightColor(); 

            if (modifyAmbientLight)
                UpdateAmbientLight();

            yield return new WaitForSeconds(1f); 
        }
    }

    void UpdateTime()
    {
        if (!debugCustomTime)
        {
            System.DateTime currentTime = System.DateTime.Now;
            dayOrNight = IsDayTime(currentTime.Hour) ? TimeOfDay.Day : TimeOfDay.Night;
        }
        else
        {
            customTime += Time.deltaTime / 3600f * customTimeScale;
            customTime = Mathf.Repeat(customTime, 24f); 
            dayOrNight = IsDayTime(Mathf.FloorToInt(customTime)) ? TimeOfDay.Day : TimeOfDay.Night;
        }
    }

    bool IsDayTime(int hour) => hour >= 6 && hour < 20; 

    void UpdateLightColor()
    {
        int currentHour = Mathf.FloorToInt(debugCustomTime ? customTime : System.DateTime.Now.Hour);
        int nextHour = (currentHour + 1) % 24;

        float minuteFraction = (debugCustomTime ? customTime - currentHour : System.DateTime.Now.Minute) / 60f;
        Color currentColor = skyColors[currentHour];
        Color nextColor = skyColors[nextHour];
        Color interpolatedColor = Color.Lerp(currentColor, nextColor, minuteFraction);

        Color finalColor = interpolatedColor * filter;

        lightSource.color = finalColor;
    }

    void UpdateAmbientLight() => RenderSettings.ambientLight = lightSource.color;

    public TimeOfDay DayOrNight => dayOrNight;
}
