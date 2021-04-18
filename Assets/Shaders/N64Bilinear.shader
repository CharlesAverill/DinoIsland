Shader "Invertex/Custom/N64Bilinear"
{
    //Unity implementation of N64 3-point Bilinear Filtering example from:
    //http://www.emutalk.net/threads/54215-Emulating-Nintendo-64-3-sample-Bilinear-Filtering-using-Shaders
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
 
        CGPROGRAM
 
        #pragma surface surf Lambert alphatest:_Cutoff
 
        sampler2D _MainTex;
        float4 _MainTex_TexelSize; //Unity will fill this in with the texture dimensions
 
        struct Input
        {
            float2 uv_MainTex;
        };
 
        fixed4 N64Sample(sampler2D tex, float2 uv, float4 texelSize)
        {
            float Texture_X = texelSize.x;
            float Texture_Y = texelSize.y;
            float2 tex_pix_a = float2(Texture_X, 0.0);
            float2 tex_pix_b = float2(0.0, Texture_Y);
            float2 tex_pix_c = float2(tex_pix_a.x, tex_pix_b.y);
            float2 half_tex = float2(tex_pix_a.x * 0.5, tex_pix_b.y * 0.5);
            float2 UVCentered = uv - half_tex;
 
            float4 diffuseColor = tex2D(tex, UVCentered);
            float4 sample_a = tex2D(tex, UVCentered + tex_pix_a);
            float4 sample_b = tex2D(tex, UVCentered + tex_pix_b);
            float4 sample_c = tex2D(tex, UVCentered + tex_pix_c);
 
            float interp_x = modf(UVCentered.x + Texture_X, Texture_X);
            float interp_y = modf(UVCentered.y + Texture_Y, Texture_Y);
 
            if (UVCentered.x < 0) { interp_x = 1 - interp_x * -1; }
            if (UVCentered.y < 0) { interp_y = 1 - interp_y * -1; }
 
            diffuseColor = (diffuseColor + interp_x * (sample_a - diffuseColor) + interp_y * (sample_b - diffuseColor)) * (1 - step(1, interp_x + interp_y));
            diffuseColor += (sample_c + (1 - interp_x) * (sample_b - sample_c) + (1 - interp_y) * (sample_a - sample_c)) * step(1, interp_x + interp_y);
       
            return diffuseColor;
        }
 
        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = N64Sample(_MainTex, IN.uv_MainTex, _MainTex_TexelSize);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
        FallBack "Diffuse"
}
