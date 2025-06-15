using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance { get; private set; }
    [SerializeField] private UITextPooling _uiTextPooling;
    private Canvas canvas;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        canvas = GetComponent<Canvas>();
    }


    //get from pool

    public UIText GetFromPool()
    {

        if (_uiTextPooling != null && _uiTextPooling.Pool != null)
        {
            UIText text = _uiTextPooling.Pool.Get();
            text.transform.SetParent(canvas.transform, false);
            return text;
        }

        Debug.LogError("UITextPooling or Pool is not initialized.");
        return null;
    }


    // Return UIText to pool
    public void ReturnToPool(UIText text)
    {
        if (_uiTextPooling != null && _uiTextPooling.Pool != null)
        {
            _uiTextPooling.Pool.Release(text);
        }
        else
        {
            Debug.LogError("UITextPooling or Pool is not initialized.");
        }
    }

}