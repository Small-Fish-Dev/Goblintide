namespace GameJam;

public enum Behaviour
{
	None,
	Raider, // Will take the behaviour of the raider, breaking stuff and attacking anyone of other factions
	Defender, // Will take the behaviour of the defender, moving closer to victims and attacking anyone of other factions
	Victim // Will take the behaviour of the victim, randomly walking unless there's raiders of opposing faction, then they run around panicking
}
public partial class BaseNPC
{

	[Net] public Behaviour CurrentBehaviour { get; set; } = Behaviour.None;
	/* This is giving errors generating code :-//
	public Dictionary<Behaviour, Action<BaseNPC>> BehaviourTree = new Dictionary<Behaviour, Action<BaseNPC>>()
	{
		{ Behaviour.None, ( npc ) => {} },
		{ Behaviour.Raider, ( npc ) => { npc.RaiderBehaviour(); } },
		{ Behaviour.Defender, ( npc ) => { npc.DefenderBehaviour(); } },
		{ Behaviour.Victim, ( npc ) => { npc.VictimBehaviour(); } }
	};
	*/

	public virtual void ComputeBehaviour()
	{
		//BehaviourTree[CurrentBehaviour].Invoke( this );

		if ( CurrentBehaviour == Behaviour.Defender )
			DefenderBehaviour();
		else if ( CurrentBehaviour == Behaviour.Raider )
			RaiderBehaviour();
		else if ( CurrentBehaviour == Behaviour.Victim )
			VictimBehaviour();
	}

	public virtual void RaiderBehaviour()
	{
		Log.Info( "I'm raiding!" );
	}
	public virtual void DefenderBehaviour()
	{
		Log.Info( "I'm defending!" );
	}
	public virtual void VictimBehaviour()
	{
		Log.Info( "I'm victm!" );
	}
}
