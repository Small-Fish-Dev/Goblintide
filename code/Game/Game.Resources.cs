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
	public static int TotalIQ
	{
		get => Instance.totalIQ;
		set
		{
			Instance.totalIQ = value;
		}
	}
	public static int MaxIQ
	{
		get => Instance.maxIQ;
		set
		{
			Instance.maxIQ = value;
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
	public static double TotalEnergy
	{
		get => Instance.totalEnergy;
		set
		{
			Instance.totalEnergy = Math.Clamp( value, 0, MaxEnergy );
		}
	}
	public static double MaxEnergy
	{
		get => Instance.maxEnergy;
		set
		{
			Instance.maxEnergy = value;
		}
	}
	public static double EnergyRechargeRate
	{
		get => Instance.energyRechargeRate;
		set
		{
			Instance.energyRechargeRate = value;
		}
	}
	public static long LastEnergyUpdate
	{
		get => Instance.lastEnergyUpdate;
		set
		{
			Instance.lastEnergyUpdate = value;
		}
	}
	[Net, Change(nameof(EmitWoodChange))] private int totalWood { get; set; } = 0;
	[Net, Change( nameof( EmitGoldChange ) )] private int totalGold { get; set; } = 0;
	[Net, Change( nameof( EmitIQChange ) )] private int totalIQ { get; set; } = 0;
	[Net, Change( nameof( EmitFoodChange ) )] private int totalFood { get; set; } = 0;
	[Net, Change( nameof( EmitWomenChange ) )] private int totalWomen { get; set; } = 0;
	[Net] private double totalEnergy { get; set; } = 10;
	[Net] private double maxEnergy { get; set; } = 30; // Default value = 30
	[Net] private int maxIQ { get; set; } = 0;
	[Net] private double energyRechargeRate { get; set; } = 1f / 60f; // Energy per second ( 1 / 60 means 1 unit every 60 seconds )
	[Net] private long lastEnergyUpdate { get; set; } = DateTime.UtcNow.Ticks;

	void EmitWoodChange(int oldValue, int newValue)
	{
		Game.AssertClient();
		Event.Run( "resources.wood", newValue );
	}

	void EmitGoldChange( int oldValue, int newValue )
	{
		Game.AssertClient();
		Event.Run( "resources.gold", newValue );
	}
	void EmitIQChange( int oldValue, int newValue )
	{
		Game.AssertClient();
		Event.Run( "resources.iq", newValue );
	}

	void EmitFoodChange( int oldValue, int newValue )
	{
		Game.AssertClient();
		Event.Run( "resources.food", newValue );
	}

	void EmitWomenChange( int oldValue, int newValue )
	{
		Game.AssertClient();
		Event.Run( "resources.women", newValue );
	}

	[Event.Tick.Server]
	public void CalculateEnergy()
	{
		TotalEnergy += energyRechargeRate * Time.Delta;
		LastEnergyUpdate = DateTime.UtcNow.Ticks / 10000000;
	}

	/// <summary>
	/// Make sure to call this after LastEnergyUpdate has been correctly set
	/// </summary>
	public static void SetEnergyFromLastEnergyDate()
	{
		var currentTime = DateTime.UtcNow.Ticks / 10000000;
		var difference = (currentTime - LastEnergyUpdate);
		TotalEnergy += (float)difference * EnergyRechargeRate;
		LastEnergyUpdate = currentTime;
	}

	[ConCmd.Admin("iq")]
	public static void AddIQ( int amount )
	{
		TotalIQ += amount;
		MaxIQ += amount;
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

	[ConCmd.Admin("wood")]
	public static void AddWood( int amount )
	{
		TotalWood += amount;
	}
	[ConCmd.Admin( "gold" )]
	public static void AddGold( int amount )
	{
		TotalGold += amount;
	}
	[ConCmd.Admin( "food" )]
	public static void AddFood( int amount )
	{
		TotalFood += amount;
	}
	[ConCmd.Admin( "women" )]
	public static void AddWomen( int amount )
	{
		TotalWood += amount;
	}
	[ConCmd.Admin( "energy" )]
	public static void AddEnergy( int amount )
	{
		TotalEnergy += amount;
	}

}
