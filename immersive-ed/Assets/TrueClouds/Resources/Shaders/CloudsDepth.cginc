inline half3 EncodeDepth(float value)
{
	float2 enc = float2(value, value * float(256));
	enc.y = frac(enc.y);
	enc.x -= enc.y / float(256);
	return half3(enc, 0);
}

inline float DecodeDepth(float4 value) {
	return value.x + value.y / float(256);
}