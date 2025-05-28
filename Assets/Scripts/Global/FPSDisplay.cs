using System;
using UnityEngine;

namespace Match3.Shares
{
    public class FpsDisplay : MonoBehaviour
    {
        float deltaTime = 0.0f;

        private void Awake()
        {
            //application.targetFrameRate = 60; // Set target frame rate

            Application.targetFrameRate = 500;
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
        void OnGUI()
        {
            int width = Screen.width, height = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(10, 10, width, height * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = height * 2 / 100;
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }
}