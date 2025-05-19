using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;


    public static class Utilities
    {
        public static bool IsPointerOverUIElement()
        {
            if (Application.isMobilePlatform)
            {
                foreach (Touch touch in Input.touches)
                {
                    int id = touch.fingerId;
                    // Debug.Log("Touch ID: " + id);
                    if (EventSystem.current.IsPointerOverGameObject(id))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerator WaitAfterCoroutine(float waitTime, Action action)
        {
            yield return new WaitForSeconds(waitTime);
            action?.Invoke();
        }

        public static IEnumerator WaitAfterEndOfFrameCoroutine(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        public static void ButtonInteractableAfter(this Button button)
        {
            button.StartCoroutine(PreventMultipleClick(button));
        }

        private static IEnumerator PreventMultipleClick(this Button button)
        {
            button.interactable = false;
            yield return new WaitForSeconds(0.5f);
            button.interactable = true;
        }
        public static bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        public static Vector2Int ImagePixelToWorld(this SpriteRenderer image, Vector2 pixel)
        {
            Bounds bounds = image.bounds;

            int imgWidth = image.sprite.texture.width;
            int imgHeight = image.sprite.texture.height;

            // Normalize coordinates (bottom-left origin in Unity)
            float normalizedX = pixel.x / imgWidth;
            float normalizedY = pixel.y / imgHeight; // No need to invert Y

            float worldX = Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedX);
            float worldY = Mathf.Lerp(bounds.min.y, bounds.max.y, normalizedY);
            return Vector2Int.RoundToInt(new Vector2(worldX, worldY));
        }

        public static Vector2Int WorldPositionToImagePixel(this SpriteRenderer image, Vector2 worldPosition)
        {
            Bounds bounds = image.bounds;

            int imgWidth = image.sprite.texture.width;
            int imgHeight = image.sprite.texture.height;

            float normalizedX = Mathf.InverseLerp(bounds.min.x, bounds.max.x, worldPosition.x);
            float normalizedY = Mathf.InverseLerp(bounds.min.y, bounds.max.y, worldPosition.y);

            float pixelX = Mathf.Lerp(0, imgWidth, normalizedX);
            float pixelY = Mathf.Lerp(0, imgHeight, normalizedY);

            return new Vector2Int(Mathf.RoundToInt(pixelX), Mathf.RoundToInt(pixelY));
        }


        public static void SetNativeSize(this Image image)
        {
            image.SetNativeSize();
        }


        public static void SetImageSize(this SpriteRenderer image, float width, float height)
        {
            image.size = new Vector2(width, height);
        }

        public static void WaitAfter(float waitTime, System.Action action)
        {
            CoroutineManager.Instance.StartStaticCoroutine(WaitAfterCoroutine(waitTime, action));
        }
        public static void WaitAfterEndOfFrame(System.Action action)
        {
            CoroutineManager.Instance.StartStaticCoroutine(WaitAfterEndOfFrameCoroutine(action));
        }

        public static bool IsConnectedToInternet()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }


        public static void ChangeAllLayer(this Transform transform, LayerMask newLayer)
        {
            transform.gameObject.layer = newLayer.value;
            ChangeLayerOfChildren(transform, newLayer.value);

            void ChangeLayerOfChildren(Transform parent, int newLayer)
            {
                foreach (Transform child in parent)
                {
                    child.gameObject.layer = newLayer;
                    // Recursively change layer for nested children
                    ChangeLayerOfChildren(child, newLayer);
                }
            }
        }

#if UNITY_EDITOR
        public static void ClearEdiorLog()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
#endif

        public static void ScaleIcon(this RectTransform target, float refWidth, float refHeight)
        {
            float scaleFactorWidth = refWidth / target.sizeDelta.x;
            float scaleFactorHeight = refHeight / target.sizeDelta.y;


            // Use the smaller scale factor to ensure the icon fits within the reference dimensions
            float scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight);

            target.sizeDelta *= scaleFactor;
            // target.transform.localScale = new Vector3(scaleFactorWidth, scaleFactorWidth, 1);
        }
        public static Vector3Int ToVector3Int(this Vector3 vector3)
        {
            return new Vector3Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), Mathf.FloorToInt(vector3.z));
        }

        public static Vector2Int ToGridPosition(this Vector3 vector3)
        {
            return new Vector2Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.z));
        }

        public static Vector2 ToVectorXZ(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        public static Vector3 ToGameDirection(this Vector3 worldDirection)
        {
            return worldDirection.Iso(new Vector3(0, Camera.main.transform.eulerAngles.y, 0));
        }


        private static System.Random rng = new System.Random();
        public static void Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]); // Swap
            }
        }

        public static Sprite ConvertTextureToSprite(Texture2D texture)
        {
            if (texture == null) return null;
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), // Pivot (centered)
                100f // Pixels Per Unit (adjust as needed)
            );
        }


        /// <summary>
        /// Map value from a range to another range
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromMin"></param>
        /// <param name="fromMax"></param>
        /// <param name="toMin"></param>
        /// <param name="toMax"></param>
        /// <returns></returns>
        public static float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            // Normalize the value to a 0-1 range relative to the original range
            float normalized = (value - fromMin) / (fromMax - fromMin);

            // Map the normalized value to the new range
            return toMin + (normalized * (toMax - toMin));
        }

        public static Vector3 Iso(this Vector3 input, Vector3 euler) =>
            Matrix4x4.Rotate(Quaternion.Euler(euler)).MultiplyPoint3x4(input);

        public static string ToPlayerKey(this string keyString, string id)
        {
            return $"{keyString}_{id}";
        }
    }

    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager _instance;

        public static CoroutineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineManager");
                    _instance = go.AddComponent<CoroutineManager>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        public void StartStaticCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }

