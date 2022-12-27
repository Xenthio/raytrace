namespace Sandbox;
partial class Pawn
{
	void PawnMovement()
	{
		Rotation = ViewAngles.ToRotation();

		// build movement from the input values
		var movement = new Vector3( Input.AnalogMove.x, Input.AnalogMove.y, 0 ).Normal;

		// rotate it to the direction we're facing
		Velocity = Rotation * movement;

		// apply some speed to it
		Velocity *= Input.Down( InputButton.Run ) ? 1000 : 200;

		// apply it to our position using MoveHelper, which handles collision
		// detection and sliding across surfaces for us
		MoveHelper helper = new MoveHelper( Position, Velocity );
		helper.Trace = helper.Trace.Size( 16 );
		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			Position = helper.Position;
		}

		// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
		//if ( IsServer && Input.Pressed( InputButton.PrimaryAttack ) )
		//{
		//var ragdoll = new ModelEntity();
		//ragdoll.SetModel( "models/citizen/citizen.vmdl" );
		//ragdoll.Position = EyePosition + EyeRotation.Forward * 40;
		//ragdoll.Rotation = Rotation.LookAt( Vector3.Random.Normal );
		//ragdoll.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		//ragdoll.PhysicsGroup.Velocity = EyeRotation.Forward * 1000;
		//}
	}
}
