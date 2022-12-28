using Sandbox;

partial class Pawn
{
	RayTraceCompute compute = new RayTraceCompute();
	void RTrace()
	{
		compute.CameraPosition = Camera.Position;
		compute.CameraDirection = Camera.Rotation.Forward;
		compute.Dispatch();
		var hook = Camera.Main.FindOrCreateHook<DrawTextureRenderHook>();
		hook.Texture = compute.Texture;
	}
}
