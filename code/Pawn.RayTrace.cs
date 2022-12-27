using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox;
partial class Pawn
{
	void RTrace()
	{
		var hook = Camera.Main.FindOrCreateHook<MyRenderHook>();
		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			for ( int i = 0; i < 256; i++ )
			{
				var ray = new Ray( Position + (Vector3.Random * 0.5f), (Vector3.Random * 1) );
				var tr = Trace.Ray( ray, 10000 ).Ignore( this ).Run();
				var myColour = Color.Yellow;
				switch ( tr.Surface.ResourceName )
				{
					case "concrete":
						myColour = Color.Gray;
						break;
					case "metal":
					case "metal.sheet":
						myColour = Color.White;
						break;
					case "grass":
						myColour = Color.Green;
						break;
					case "rubber":
						myColour = Color.Blue;
						break;
					case "glass":
						myColour = Color.FromBytes( 255, 255, 255, 16 );
						break;
					case "dirt":
						myColour = Color.FromBytes( 64, 32, 0 );
						break;
					default:
						myColour = Color.Yellow;
						break;
				}
				DebugOverlay.Line( tr.EndPosition, tr.EndPosition + tr.Normal, myColour, 30 );
			}
		}
		if ( Input.Down( InputButton.SecondaryAttack ) )
		{
			//Array.Clear( hook.ClrDat, 0, hook.ClrDat.Length );
			hook.PrevScreen.Dispose();
			hook.ClrDat = new byte[MyRenderHook.RT_WIDTH.CeilToInt() * MyRenderHook.RT_HEIGHT.CeilToInt() * 4];
			hook.PrevScreen = Texture.Create( MyRenderHook.RT_WIDTH.CeilToInt(), MyRenderHook.RT_HEIGHT.CeilToInt() ).Finish();
			//hook.PrevScreen.Update( hook.ClrDat ); 
			hook.setVars();
		}

		if ( Input.Pressed( InputButton.Use ) )
		{
			hook.DoClear = !hook.DoClear;
		}

		if ( Input.Pressed( InputButton.Reload ) )
		{
			hook.DoBounce = !hook.DoBounce;
		}

		if ( Input.Pressed( InputButton.Menu ) )
		{
			hook.DoTrace = !hook.DoTrace;
		}

		if ( Input.Pressed( InputButton.Flashlight ) )
		{
			hook.Iterative = !hook.Iterative;
		}



	}


}
internal class MyRenderHook : RenderHook
{
	public Color MyColour { get; set; } = Color.White;
	public bool DoClear { get; set; } = true;
	public bool DoBounce { get; set; } = false;
	public bool DoTrace { get; set; } = false;
	public bool Iterative { get; set; } = false;


	public Texture PrevScreen = Texture.Create( Screen.Width.FloorToInt(), Screen.Height.FloorToInt() ).Finish();
	public byte[] ClrDat = new byte[Screen.Width.FloorToInt() * Screen.Height.FloorToInt() * 4];
	//public Texture CurScreen = Texture.CreateRenderTarget();
	RenderAttributes attributes = new();
	RenderAttributes attributes2 = new();
	float aspect_ratio;
	float image_width;
	float image_height;

	float viewport_height;
	float viewport_width;
	float focal_length;

	Vector3 origin;
	Vector3 horizontal;
	Vector3 vertical;
	Vector3 lower_left_corner;
	Rotation rotation;


	[ConVar.Client( "rt_aa_sharpness" )]
	static public int AA_SHARPNESS { get; set; } = 3;
	[ConVar.Client( "rt_max_bounce_depth" )]
	static public int MAX_DEPTH { get; set; } = 3;
	[ConVar.Client( "rt_max_distance" )]
	static public int MAX_DIST { get; set; } = 1600;
	[ConVar.Client( "rt_samples_per_pixel" )]
	static public int SAMPLES_PER_PIXEL { get; set; } = 1;
	[ConVar.Client( "rt_blend_amount" )]
	static public float BLEND_AMOUNT { get; set; } = 0.5f;

	[ConVar.Client( "rt_width" )]
	static public float RT_WIDTH { get; set; } = 300;
	[ConVar.Client( "rt_height" )]
	static public float RT_HEIGHT { get; set; } = 300;
	[ConVar.Client( "rt_focal_length" )]
	static public float RT_FOCAL_LENGTH { get; set; } = 1.777f;
	[ConVar.Client( "rt_pixels_per_iteration" )]
	static public int PIXELS_PER_ITERATION { get; set; } = 1024;

	public void setVars()
	{
		var w = Screen.Width;
		var h = Screen.Height;
		w = RT_WIDTH;
		h = RT_HEIGHT;
		aspect_ratio = w / h;
		image_width = w;//400.0f;
		image_height = h;// (image_width / aspect_ratio).FloorToInt();

		viewport_height = 2.0f;
		viewport_width = aspect_ratio * viewport_height;
		focal_length = RT_FOCAL_LENGTH;
		//focal_length = Screen.Width / Screen.Height;

		origin = Vector3.Zero;
		horizontal = new Vector3( viewport_width, 0, 0 );
		vertical = new Vector3( 0, viewport_height, 0 );
		lower_left_corner = origin - horizontal / 2 - vertical / 2 - new Vector3( 0, 0, focal_length );
	}

	void DoPixel( int x, int y )
	{

		Vector3 SampleColour = Vector3.Zero;
		bool didonce = false;
		for ( var i = 0; i < SAMPLES_PER_PIXEL; i++ )
		{



			double xaa = ((double)x + (Game.Random.Double( -1, 1 ) / AA_SHARPNESS));
			double yaa = ((double)y + (Game.Random.Double( -1, 1 ) / AA_SHARPNESS));
			var rttr = rt( xaa, yaa );
			SampleColour += new Vector3( rttr.HitColour.r.Clamp( 0, 1 ), rttr.HitColour.g.Clamp( 0, 1 ), rttr.HitColour.b.Clamp( 0, 1 ) );
		}
		//var b = rttr.TraceResult.EndPosition.ToScreen();
		//var bouncedir = 2 * (rttr.TraceResult.Direction.Dot( rttr.TraceResult.Normal )) * rttr.TraceResult.Normal - rttr.TraceResult.Direction;

		var index = GetPixelIndex( x, image_height.CeilToInt() - y, PrevScreen );



		var r = SampleColour.x;
		var g = SampleColour.y;
		var b = SampleColour.z;

		var scale = 1.0f / SAMPLES_PER_PIXEL;
		r *= scale;
		g *= scale;
		b *= scale;

		var correctedColour = new Color( MathF.Sqrt( r ), MathF.Sqrt( g ), MathF.Sqrt( b ), MathF.Sqrt( 1 ) );
		var clr = Color.FromBytes( ClrDat[index], ClrDat[index + 1], ClrDat[index + 2], ClrDat[index + 3] );
		var newclr = Blend( clr, correctedColour, BLEND_AMOUNT );
		ClrDat[index] = (byte)(newclr.r * 255);
		ClrDat[index + 1] = (byte)(newclr.g * 255);
		ClrDat[index + 2] = (byte)(newclr.b * 255);
		ClrDat[index + 3] = (byte)(newclr.a * 255);
	}
	Vector3 reflect( Vector3 direction, Vector3 normal )
	{
		return direction - 2 * Vector3.Dot( direction, normal ) * normal;
	}
	List<int> numbw = new();
	List<int> numbh = new();
	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage == Stage.AfterPostProcess )
		{
			setVars();
			if ( DoClear ) Graphics.Clear( true, false );
			//numbw = Enumerable.Range( 0, image_width.CeilToInt() ).OrderBy( x => Rand.Int( 0, 1000 ) ).ToList();//GetRandomNumber( 0, image_width.CeilToInt(), image_width.CeilToInt() );
			//numbh = Enumerable.Range( 0, image_height.CeilToInt() ).OrderBy( x => Rand.Int( 0, 1000 ) ).ToList();//GetRandomNumber( 0, image_height.CeilToInt(), image_height.CeilToInt() );
			if ( !DoTrace ) return;
			if ( Iterative )
			{
				for ( int i = 0; i < PIXELS_PER_ITERATION; i++ )
				{
					if ( numbw.Count == 0 || numbw == null )
					{
						numbw = Enumerable.Range( 0, image_width.CeilToInt() ).OrderBy( x => Game.Random.Int( 0, 100000 ) ).ToList();//GetRandomNumber( 0, image_width.CeilToInt(), image_width.CeilToInt() );

					}
					if ( numbh.Count == 0 || numbh == null )
					{
						numbh = Enumerable.Range( 0, image_height.CeilToInt() ).OrderBy( x => Game.Random.Int( 0, 100000 ) ).ToList();//GetRandomNumber( 0, image_width.CeilToInt(), image_width.CeilToInt() );

					}
					var pixelx = numbw.First();
					numbw.RemoveAt( 0 );
					var pixely = numbh.First();
					numbh.RemoveAt( 0 );
					int j = pixelx;// Rand.Int( 0, image_width.CeilToInt() );
					int k = pixely;// Rand.Int( 0, image_height.CeilToInt() );
					DoPixel( j, k );
				}
			}
			else
			{
				for ( int j = 0; j < image_width; j++ )
				{
					for ( int k = 0; k < image_height; k++ )
					{
						DoPixel( j, k );
					}
				}
			}
			var a = Material.UI.Basic;

			PrevScreen.Update( ClrDat );
			a.Set( "Color", PrevScreen );
			attributes2.Set( "Texture", PrevScreen );
			Graphics.DrawQuad( new Rect( 0, 0, Screen.Width, Screen.Height ),
			a,
			Color.White, attributes2 );
		}
	}
	public static Color Blend( Color color, Color backColor, float amount )
	{
		float r = (color.r * amount + backColor.r * (1 - amount));
		float g = (color.g * amount + backColor.g * (1 - amount));
		float b = (color.b * amount + backColor.b * (1 - amount));
		float a = (color.a * amount + backColor.a * (1 - amount));
		return new Color( r, g, b, a );
	}
	public static Color BlendMult( Color color, Color backColor )
	{
		float r = ((color.r) * (backColor.r));//.Clamp( -1, 1 );
		float g = ((color.g) * (backColor.g));//.Clamp( -1, 1 );
		float b = ((color.b) * (backColor.b));//.Clamp( -1, 1 );
		float a = ((color.a) * (backColor.a));//.Clamp( -1, 1 );
		return new Color( r, g, b, a );
	}

	//Entity Light;
	RTResult rt( double x, double y )
	{
		var u = (float)(x) / (image_width - 1);
		var v = (float)(y) / (image_height - 1);

		var dir = lower_left_corner + u * horizontal + v * vertical;

		//if ( Light == null ) Light = Entity.All.OfType<SpotLightEntity>().First();
		var rot = Camera.Rotation;
		var trn = new Transform( Camera.Position, rot.RotateAroundAxis( Vector3.Left, -90 ).RotateAroundAxis( Vector3.Up, -90 ) );
		var ray = new Ray( Camera.Position, trn.NormalToWorld( dir ) );

		var tr = DoRay( ray );

		var myColour = GetColour( tr, MAX_DEPTH );




		//myColour = new Color( tr.Normal.x, tr.Normal.y, tr.Normal.z );

		return new RTResult( tr, myColour );
	}
	Vector3 random_in_unit_sphere()
	{
		while ( true )
		{
			var p = Vector3.Random;
			if ( p.LengthSquared >= 1 ) continue;
			return p;
		}
	}
	Vector3 random_in_hemisphere( Vector3 normal )
	{
		Vector3 in_unit_sphere = random_in_unit_sphere();
		if ( Vector3.Dot( in_unit_sphere, normal ) > 0.0 ) // In the same hemisphere as the normal
			return in_unit_sphere;
		else
			return -in_unit_sphere;
	}
	TraceResult DoRay( Ray ray, float distch = 0 )
	{
		int scale = 3;
		var tr1 = Trace.Ray( ray, MAX_DIST - distch ).WithoutTags( "playerclip", "clip" );
		//if (ent != null) { tr1 = tr1.Ignore( ent ); }
		var tr = tr1.Run();
		if ( !tr.Hit ) return tr;


		//var tr3 = Trace.Ray( source, tr.EndPosition ).WithoutTags( "player" ).WithoutTags( "glass" ).Run();
		//if ( tr3.Fraction != 1 ) return tr;

		//myColour = myColour.WithAlphaMultiplied( dist );
		//myColour = myColour.WithAlphaMultiplied( tr3.Fraction );



		//PrevScreen.Update( sp, (b.x * Screen.Width).FloorToInt(), (b.y * PrevScreen.Height).FloorToInt(), scale, scale);
		return tr;
	}
	int GetPixelIndex( int x, int y, Texture Tex )
	{
		x = Math.Clamp( x, 0, Tex.Width - 1 );
		y = Math.Clamp( y, 0, Tex.Height - 1 );

		return ((y * Tex.Width) + x) * 4;
	}
	Color GetColour( TraceResult tr, int depth )
	{

		var surf = tr.Surface.ResourceName;
		if ( tr.Hit && tr.Entity.Tags.Has( "emit" ) )
		{
			return new Color( 10000.0f );
		}
		var t = 0.5f * (tr.Direction.z + 1.0f);
		var clrv = ((1.0f - t) * new Vector3( 1.0f, 1.0f, 1.0f ) + t * new Vector3( 0.5f, 0.7f, 1.0f )).Clamp( -1, 1 );
		var myColour = new Color( clrv.x, clrv.y, clrv.z );

		if ( !tr.Hit ) return myColour;
		switch ( surf )
		{
			case "concrete":
				myColour = Color.Gray;
				break;
			case "metal":
			case "metal.sheet":
				myColour = Color.White;
				break;
			case "grass":
				myColour = Color.Green;
				break;
			case "rubber":
				myColour = Color.Blue;
				break;
			case "glass":
				myColour = Color.FromBytes( 255, 255, 255, 16 );
				break;
			case "dirt":
				myColour = Color.FromBytes( 64, 32, 0 );
				break;
			default:
				myColour = Color.Yellow;
				break;
		}

		if ( depth <= 0 )
			return new Color( 0, 0, 0 );

		if ( tr.Hit && DoBounce )
		{

			Vector3 target = tr.HitPosition + tr.Normal + random_in_unit_sphere();//Vector3.Random;//random_in_hemisphere( tr.Normal);
			if ( surf == "metal" || surf == "metal.sheet" || surf == "glass" )
			{
				target = tr.HitPosition + reflect( tr.Direction, tr.Normal );
			}
			var trgclr = Blend( Color.Black, GetColour( DoRay( new Ray( tr.HitPosition, target - tr.HitPosition ), (tr.Distance / 4) ), depth - 1 ), 0.5f );
			var attenuation = myColour;
			return BlendMult( trgclr, attenuation );
		}

		return myColour;
	}
}
public struct RTResult
{
	public TraceResult TraceResult;
	public Color HitColour;
	public RTResult( TraceResult tr, Color clr )
	{
		TraceResult = tr;
		HitColour = clr;
	}
}
