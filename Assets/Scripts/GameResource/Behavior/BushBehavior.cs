using UnityEngine;

public class BushBehavior : ResourceBehavior
{
    public override void OnCollected()
    {
        Destroy(gameObject);
    }
}