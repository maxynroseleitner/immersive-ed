#ifndef _WEATHER_MAKER_NOISE_SHADER_INCLUDE_
#define _WEATHER_MAKER_NOISE_SHADER_INCLUDE_

uniform float _Frequency = 10.0;
uniform float _Lacunarity = 2.0;
uniform float _Gain = 0.5;
uniform float _Jitter = 1.0;

//1/7
#define K 0.142857142857
//3/7
#define Ko 0.428571428571

float3 mod(float3 x, float y) { return x - y * floor(x / y); }
float2 mod(float2 x, float y) { return x - y * floor(x / y); }

// Permutation polynomial: (34x^2 + x) mod 289
float3 Permutation(float3 x)
{
	return mod((34.0 * x + 1.0) * x, 289.0);
}

float2 cell_noise_3d(float3 P)
{
	float3 Pi = mod(floor(P), 289.0);
	float3 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = Permutation(Pi.x + oi);
	float3 py = Permutation(Pi.y + oi);

	float3 p, ox, oy, oz, dx, dy, dz;
	float2 F = 1e6;

	for (int i = 0; i < 3; i++)
	{
		for (int j = 0; j < 3; j++)
		{
			p = Permutation(px[i] + py[j] + Pi.z + oi); // pij1, pij2, pij3

			ox = frac(p*K) - Ko;
			oy = mod(floor(p*K), 7.0)*K - Ko;

			p = Permutation(p);

			oz = frac(p*K) - Ko;

			dx = Pf.x - of[i] + _Jitter*ox;
			dy = Pf.y - of[j] + _Jitter*oy;
			dz = Pf.z - of + _Jitter*oz;

			float3 d = dx * dx + dy * dy + dz * dz; // dij1, dij2 and dij3, squared

													//Find lowest and second lowest distances
			for (int n = 0; n < 3; n++)
			{
				if (d[n] < F[0])
				{
					F[1] = F[0];
					F[0] = d[n];
				}
				else if (d[n] < F[1])
				{
					F[1] = d[n];
				}
			}
		}
	}

	return F;
}





#endif // _WEATHER_MAKER_NOISE_SHADER_INCLUDE_