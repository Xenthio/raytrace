

// This is what contains our compute shader instance, as well as the texture
// we're going to be rendering to.
using Sandbox;

public class RayTraceCompute
{
	private ComputeShader computeShader;
	public Vector3 CameraPosition;
	public Vector3 CameraDirection;
	public Texture Texture { get; }

	public RayTraceCompute()
	{
		// Create a texture that we can use
		Texture = Texture.Create( 512, 512 )
						 .WithUAVBinding()                        // Needs to have this if we're using it in a compute shader
						 .WithFormat( ImageFormat.RGBA16161616F ) // Other formats are available :-)
						 .Finish();

		computeShader = new ComputeShader( "rt_gpu_compute" ); // This should be the name of your shader
	}

	public void Dispatch()
	{
		// Set up the shader...
		computeShader.Attributes.Set( "OutputTexture", Texture );
		computeShader.Attributes.Set( "TexHeight", Texture.Height );
		computeShader.Attributes.Set( "TexWidth", Texture.Width );

		computeShader.Attributes.Set( "RayPosition", CameraPosition );
		computeShader.Attributes.Set( "RayDirection", CameraDirection );

		// ...and run it!
		computeShader.Dispatch( Texture.Width, Texture.Height, 1 );
	}
}
