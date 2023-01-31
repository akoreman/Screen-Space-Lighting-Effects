#ifndef CUSTOM_RANDOM_INCLUDED
#define CUSTOM_RANDOM_INCLUDED

float RandomRange_float(float2 Seed, float Min, float Max)
{
    float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
    return lerp(Min, Max, randomno);
}

#endif