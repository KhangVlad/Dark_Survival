public class PineTreeBehavior : ResourceBehavior
{
    public override void OnCollected()
    {
        Destroy(gameObject);
    }
}
