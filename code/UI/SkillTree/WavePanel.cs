using Sandbox.UI;

namespace GameJam.UI;

public class WavePanel : Panel
{
	private const int Points = 128;
	private const int DropAfter = 13;
	private readonly float[] _jumps = new float[Points];

	public WavePanel()
	{
		for ( var i = 0; i < Points; i++ )
		{
			_jumps[i] = Random.Shared.Float( 0.0f, 1.15f );
		}
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		Render();
	}

	private void Render()
	{
	}
}
