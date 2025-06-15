public class StoneBehavior : ResourceBehavior
{
    public override void OnCollected()
    {
        Destroy(gameObject);
    }
}