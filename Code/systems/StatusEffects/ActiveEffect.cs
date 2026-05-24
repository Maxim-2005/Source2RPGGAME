using Sandbox;

public sealed class ActiveEffect
{
    public string Id { get; init; }
    public float TimeRemaining { get; set; }
    public float Magnitude { get; init; }
    public float TickInterval { get; init; }
    public GameObject Source { get; init; }
    public IStatusEffect Logic { get; set; }
    public TimeSince TimeSinceLastTick { get; set; }
}
