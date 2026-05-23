namespace MagicSystem
{
	public interface IGroundTraceable
	{
		// Возвращает true, если объект находится в воздухе (в свободном падении/полете)
		bool IsInAir { get; }
	}
}
