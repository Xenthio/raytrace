
using Sandbox;

internal class DrawTextureRenderHook : RenderHook
{
	public Texture Texture;
	RenderAttributes attributes = new();
	public DrawTextureRenderHook()
	{
		// Create a texture that we can use
		Texture = Texture.Create( 512, 512 )
						 .WithUAVBinding()                        // Needs to have this if we're using it in a compute shader
						 .WithFormat( ImageFormat.RGBA16161616F ) // Other formats are available :-)
						 .Finish();
	}
	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage == Stage.AfterPostProcess )
		{
			var a = Material.UI.Basic;
			a.Set( "Color", Texture );
			attributes.Set( "Texture", Texture );
			Graphics.DrawQuad( new Rect( 0, 0, Screen.Width / 2, Screen.Height / 2 ),
			a,
			Color.White, attributes );
		}
	}
}
