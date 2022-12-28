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
    
    [numthreads(8, 8, 1)] 
    void MainCs( uint uGroupIndex : SV_GroupIndex, uint3 vThreadId : SV_DispatchThreadID )
    {
		g_tOutput[vThreadId.xy] = float4( 1, 0, 1, 1 );
    }
}