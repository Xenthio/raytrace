namespace Sandbox;

partial class Pawn : AnimatedEntity
{
	/// <summary>
	/// Called from the gamemode, clientside only.
	/// </summary>
	public override void BuildInput()
	{
		OriginalViewAngles = ViewAngles;

		if ( Input.StopProcessing )
			return;

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
		if ( Game.LocalPawn is Player userPlayer )
		{
			userPlayer.ViewAngles = Camera.Rotation.Angles();
		}
	}
	[ClientInput] public Angles ViewAngles { get; set; }
	public Angles OriginalViewAngles { get; private set; }
	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		//
		// Use a watermelon model
		//
		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		PawnMovement();
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Rotation = ViewAngles.ToRotation();
		// Update rotation every frame, to keep things smooth 
		Camera.Rotation = ViewAngles.ToRotation();
		Camera.Position = Position;
		RTrace();
	}
}
