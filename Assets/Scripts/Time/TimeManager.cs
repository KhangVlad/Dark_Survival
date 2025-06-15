using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public Action OnMinuteElapsed;

    [SerializeField] private GameTime currentTime; // Shows in Inspector
    [SerializeField] private float timer;
    [SerializeField] private float timeScale = 1f; // Default real-time

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentTime = GameTime.FromDateTime(DateTime.Now);
    }


    private void Update()
    {
        timer += Time.deltaTime * timeScale;

        while (timer >= 60f)
        {
            timer -= 60f;
            currentTime.AddMinutes(1);
            OnMinuteElapsed?.Invoke();
        }
    }

    public GameTime GetCurrentTime()
    {
        return currentTime;
    }

    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale); // Clamp to 0 or more
    }

    public float GetTimeScale()
    {
        return timeScale;
    }
}

[Serializable]
public struct GameTime
{
    public int hour;

    public int minute;

    public void AddMinutes(int minutes)
    {
        minute += minutes;
        while (minute >= 60)
        {
            minute -= 60;
            hour = (hour + 1) % 24;
        }
    }

    public override string ToString()
    {
        return $"{hour:D2}:{minute:D2}";
    }

    public static GameTime FromDateTime(DateTime dateTime)
    {
        return new GameTime { hour = dateTime.Hour, minute = dateTime.Minute };
    }
}