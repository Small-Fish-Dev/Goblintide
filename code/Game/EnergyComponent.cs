namespace GameJam;

public partial class EnergyComponent : EntityComponent
{
	[Net] public float Value { get; protected set; }

	/// <summary> Max and starting energy </summary>
	public const float MaxEnergy = 100.0f;

	/// <summary> Time (in seconds) for energy to fully regenerate </summary>
	public const float RegenerateTime = 60 * 60 * 3;

	public bool Use( float amount )
	{
		if ( !(Value >= amount) )
			return false;

		Value -= amount;
		return true;
	}

	public EnergyComponent() => Value = MaxEnergy;

	[Event.Tick]
	private void Update()
	{
		// note(gio): this is wrong please fix it
		Value += MaxEnergy / RegenerateTime * Time.Delta;
	}
}
