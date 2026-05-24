using Sandbox;

public sealed class DebugChainVisualizer : Component
{
    public struct LineData
    {
        public Vector3 Start;
        public Vector3 End;
    }

    public LineData[] Lines { get; set; }
    public float Lifetime { get; set; } = 1.5f;

    private TimeSince _spawned;

    protected override void OnStart()
    {
        _spawned = 0;
    }

    protected override void OnUpdate()
    {
        if ( _spawned > Lifetime )
        {
            GameObject.Destroy();
            return;
        }

        if ( Lines == null || Lines.Length == 0 ) return;

        using ( Gizmo.Scope() )
        {
            Gizmo.Draw.Color = Color.Cyan;

            foreach ( var line in Lines )
            {
                if ( line.Start.IsNearlyZero( 0.001f ) && line.End.IsNearlyZero( 0.001f ) )
                    continue;

                Gizmo.Draw.Line( line.Start, line.End );
            }
        }
    }
}
