//-------------------------------------------------------------------------------------------------------------------------------------------------------------
HEADER
{
	DevShader = true;
	Description = "Compute Shader for RT.";
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
MODES
{
	Default();
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
FEATURES
{
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
COMMON
{
	#include "common/shared.hlsl" // This should always be the first include in COMMON
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
CS
{
    
	//Collision
	CreateTexture2D( g_tDepthBufferCopyTexture ) < Attribute( "DepthBufferCopyTexture" ); SrgbRead( false );  AddressU( CLAMP ); AddressV( CLAMP ); Filter( POINT ); >;
    RWTexture2D<float4> g_tOutput< Attribute( "OutputTexture" ); >;
	float3 rayDirection < Attribute("RayDirection"); > ;
	float3 rayPosition < Attribute("RayPosition");  > ;

	float width < Attribute("TexWidth");  > ;
	float height < Attribute("TexHeight");  > ;

    bool hit_sphere(float3 center, double radius, float3 rpos, float3 rdir) {
        float3 oc = rpos - center;
        double a = dot(rdir, rdir);
        double b = 2.0 * dot(oc, rdir);
        double c = dot(oc, oc) - radius*radius;
        double discriminant = b*b - 4*a*c;
        return (discriminant > 0);
    }


    float4 RayColor(float3 dir, float3 pos) {
        if (hit_sphere(float3(0,0,-1), 0.5, pos,dir))
            return float4(1, 0, 0, 1);
        float t = 0.5*(dir.y + 1.0);
        return (1.0-t)*float4(1.0, 1.0, 1.0, 1) + t*float4(0.5, 0.7, 1.0, 1);
    }

    [numthreads(8, 8, 1)] 
    void MainCs( uint uGroupIndex : SV_GroupIndex, uint3 vThreadId : SV_DispatchThreadID )
    { 
        float3 normalDir;
        float3 normalDir2;
        float u = (float)vThreadId.x / (512);
        float v = 1-((float)vThreadId.y / (512));
        normalDir = float3(u,v,1);
        normalDir2 = float3(vThreadId.x,vThreadId.y,1);
		g_tOutput[vThreadId.xy] = RayColor( normalDir-0.5, float3(0,0,0));
    }
}