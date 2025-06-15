public class LogBehavior : ResourceBehavior
{
    public override void OnCollected()
    {
        Destroy(gameObject);
    }
}
