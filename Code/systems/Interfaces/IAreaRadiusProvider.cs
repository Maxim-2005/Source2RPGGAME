public interface IAreaRadiusProvider : IAttackStateProvider, IRangeProvider
{
    IProjectileBehavior Behavior { get; }
}
