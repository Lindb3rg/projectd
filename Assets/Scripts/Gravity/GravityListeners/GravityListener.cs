public interface IGravityListener
{
    void OnGravityFlipStarted(GravityDirection newDirection);
    void OnGravityFlipCompleted(GravityDirection newDirection);
    
}