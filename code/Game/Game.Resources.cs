using GameJam.Props.Collectable;

namespace GameJam;

public partial class GameMgr
{
	public static IList<BaseNPC> GoblinArmy
	{
		get => Instance.goblinArmy;
		set
		{
			Instance.goblinArmy = value;
		}
	}
	[Net] private IList<BaseNPC> goblinArmy { get; set; }

	public static int TotalGold
	{
		get => Instance.totalGold;
		set
		{
			Instance.totalGold = value;
		}
	}
	public static int TotalWood
	{
		get => Instance.totalWood;
		set
		{
			Instance.totalWood = value;
		}
	}
	public static int TotalFood
	{
		get => Instance.totalFood;
		set
		{
			Instance.totalFood = value;
		}
	}
	public static int TotalWomen
	{
		get => Instance.totalWomen;
		set
		{
			Instance.totalWomen = value;
		}
	}
	public static float TotalEnergy
	{
		get => Instance.totalEnergy;
		set
		{
			LastEnergyUpdate = DateTime.UtcNow;
			Instance.totalEnergy = value;
		}
	}
	public static float MaxEnergy
	{
		get => Instance.maxEnergy;
		set
		{
			Instance.maxEnergy = value;
		}
	}
	public static float EnergyRechargeRate
	{
		get => Instance.energyRechargeRate;
		set
		{
			Instance.energyRechargeRate = value;
		}
	}
	public static DateTime LastEnergyUpdate
	{
		get => Instance.lastEnergyUpdate;
		set
		{
			Instance.lastEnergyUpdate = value;
		}
	}
	[Net] private int totalWood { get; set; } = 0;
	[Net] private int totalGold { get; set; } = 0;
	[Net] private int totalFood { get; set; } = 0;
	[Net] private int totalWomen { get; set; } = 0;
	[Net] private float totalEnergy { get; set; } = 0;
	[Net] private float maxEnergy { get; set; } = 30; // Default value
	[Net] private float energyRechargeRate { get; set; } = 1; // Energy per second
	[Net] private DateTime lastEnergyUpdate { get; set; } = DateTime.UtcNow;

	[Event.Tick.Server]
	public void CalculateEnergy()
	{
		TotalEnergy += 
	}

	public static void GoblinArmyEnabled( bool enabled )
	{
		foreach ( var goblin in GoblinArmy )
		{
			goblin.Disabled = !enabled;
		}
	}

	/// <summary>
	/// InsideTown = Place them inside the town or in the forest?
	/// </summary>
	/// <param name="insideTown"></param>
	public static void PlaceGoblinArmy( bool insideTown )
	{
		var currentTown = GameMgr.CurrentTown;
		if ( currentTown == null ) return;
		var clearingDistance = currentTown.TownRadius + 600f;
		var forestSize = clearingDistance + 1000f;

		foreach ( var goblin in GoblinArmy )
		{
			var distance = insideTown ? Game.Random.Float( -currentTown.TownRadius, currentTown.TownRadius ) : Game.Random.Float( clearingDistance, forestSize );
			var newPosition = currentTown.Position + Vector3.Random.WithZ( 0 ).Normal * distance;
			goblin.Position = newPosition;
		}
	}

	public static void AddResource( Collectable type, int amount )
	{
		if ( type == Collectable.Wood )
			TotalWood += amount;
		if ( type == Collectable.Gold )
			TotalGold += amount;
		if ( type == Collectable.Food )
			TotalFood += amount;
		if ( type == Collectable.Woman )
			TotalWomen += amount;
	}

}
