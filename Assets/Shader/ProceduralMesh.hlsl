
void Ripple_float(float3 PositionIn,float3 Origin,float Period,float Speed, float Amplitude,
    out  float3 PositionOut,out float3 NormalOut,out float3 TangentOut)
{
    float3 p = PositionIn - Origin;
    float d = length(p);
    float f = 2.0 * PI * Period * (d - Speed * _Time.y);
    
    PositionOut = PositionIn + float3(0.0,Amplitude * sin(f),0.0);

    //需要求该函数对应的导数来获取法线的状态
    // float2 derivatives = float2(ddx(PositionOut.y),ddy(PositionOut.y));
    float2 derivatives = (2 * PI * Period * Amplitude * cos(f) / max(d,0.0001)) * p.xz;

    TangentOut = float3(1.0,derivatives.x,0.0);
    NormalOut = cross(float3(0.0,derivatives.y,1.0),TangentOut);     //这里为什么不需要归一化？因为使用Shader Graph 从对象空间到世界空间过程中会自动归一化
    //PositionOut = float3(0.0,0.0,0.0);
}