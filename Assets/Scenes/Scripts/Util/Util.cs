using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using Random = UnityEngine.Random;


/// <summary>
/// 
/// </summary>

public struct Matrix2x2
{
    public float m00, m01;
    public float m10, m11;

    public Matrix2x2(float m00, float m01, float m10, float m11)
    {
        this.m00 = m00;
        this.m01 = m01;
        this.m10 = m10;
        this.m11 = m11;
    }

    // 矩阵乘法
    public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
    {
        return new Matrix2x2(
            a.m00 * b.m00 + a.m01 * b.m10,
            a.m00 * b.m01 + a.m01 * b.m11,
            a.m10 * b.m00 + a.m11 * b.m10,
            a.m10 * b.m01 + a.m11 * b.m11
        );
    }

    // 矩阵和向量乘法
    public static Vector2 operator *(Matrix2x2 a, Vector2 v)
    {
        return new Vector2(
            a.m00 * v.x + a.m01 * v.y,
            a.m10 * v.x + a.m11 * v.y
        );
    }
    
    public static Vector2 operator *(Matrix2x2 a, float v)
    {
        return new Vector2(
            a.m00 * v + a.m01 * v,
            a.m10 * v + a.m11 * v
        );
    }

    // 转置矩阵
    public Matrix2x2 Transpose()
    {
        return new Matrix2x2(
            m00, m10,
            m01, m11
        );
    }

    // 计算矩阵的行列式
    public float Determinant()
    {
        return m00 * m11 - m01 * m10;
    }

    // 计算逆矩阵
    public Matrix2x2 Inverse()
    {
        float det = Determinant();
        if (det == 0f)
            throw new System.InvalidOperationException("Matrix is not invertible.");

        float invDet = 1f / det;

        return new Matrix2x2(
            m11 * invDet,
            -m01 * invDet,
            -m10 * invDet,
            m00 * invDet
        );
    }
}


public static class Perlin
{
    #region Noise functions

    public static float Noise(float x)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        x -= Mathf.Floor(x);
        var u = Fade(x);
        return Lerp(u, Grad(perm[X], x), Grad(perm[X + 1], x - 1)) * 2;
    }

    public static float Noise(float x, float y)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        var u = Fade(x);
        var v = Fade(y);
        var A = (perm[X] + Y) & 0xff;
        var B = (perm[X + 1] + Y) & 0xff;
        
        return Lerp(v, 
                    Lerp(u, 
                        Grad(perm[A], x, y), 
                        Grad(perm[B], x - 1, y)),
                    Lerp(u, 
                        Grad(perm[A + 1], x, y - 1), 
                        Grad(perm[B + 1], x - 1, y - 1)));
    }

    public static float Noise(Vector2 coord)
    {
        return Noise(coord.x, coord.y);
    }
    
    // 返回的时候还计算了梯度
    public static Vector3 NoiseWithGradient(float x, float y)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);

        var u = Fade(x);
        var v = Fade(y);

        var A = perm[X] + Y;
        var B = perm[X + 1] + Y;

        var gradAA = Grad(perm[A & 0xff], x, y);
        var gradBA = Grad(perm[B & 0xff], x - 1, y);
        var gradAB = Grad(perm[(A + 1) & 0xff], x, y - 1);
        var gradBB = Grad(perm[(B + 1) & 0xff], x - 1, y - 1);

        // 计算噪声值
        float noise = Lerp(v, Lerp(u, gradAA, gradBA),
            Lerp(u, gradAB, gradBB));

        // 计算梯度
        float dNoise_dx = Lerp(v, 
            LerpDerivative(u, gradBA, gradAA, x - 1, x), 
            LerpDerivative(u, gradBB, gradAB, x - 1, x));
        float dNoise_dy = Lerp(u, 
            LerpDerivative(v, gradAB, gradAA, y - 1, y), 
            LerpDerivative(v, gradBB, gradBA, y - 1, y));

        return new Vector3(noise, dNoise_dx, dNoise_dy);
    }

    public static Vector3 NoiseWithGradient(Vector2 coord)
    {
        return NoiseWithGradient(coord.x, coord.y);
    }

    static float LerpDerivative(float t, float a, float b, float aPos, float bPos)
    {
        return (b - a) * FadeDerivative(t) + (bPos - aPos) * Lerp(t, a, b);
    }
    
    public static float Noise(float x, float y, float z)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        var Z = Mathf.FloorToInt(z) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);
        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);
        var A = (perm[X] + Y) & 0xff;
        var B = (perm[X + 1] + Y) & 0xff;
        var AA = (perm[A] + Z) & 0xff;
        var BA = (perm[B] + Z) & 0xff;
        var AB = (perm[A + 1] + Z) & 0xff;
        var BB = (perm[B + 1] + Z) & 0xff;
        return Lerp(w, Lerp(v, Lerp(u, Grad(perm[AA], x, y, z), Grad(perm[BA], x - 1, y, z)),
                               Lerp(u, Grad(perm[AB], x, y - 1, z), Grad(perm[BB], x - 1, y - 1, z))),
                       Lerp(v, Lerp(u, Grad(perm[AA + 1], x, y, z - 1), Grad(perm[BA + 1], x - 1, y, z - 1)),
                               Lerp(u, Grad(perm[AB + 1], x, y - 1, z - 1), Grad(perm[BB + 1], x - 1, y - 1, z - 1))));
    }

    public static float Noise(Vector3 coord)
    {
        return Noise(coord.x, coord.y, coord.z);
    }

    #endregion

    #region fBm functions

    public static float Fbm(float x, int octave)
    {
        var f = 0.0f;
        var w = 0.5f;
        for (var i = 0; i < octave; i++)
        {
            f += w * Noise(x);
            x *= 2.0f;
            w *= 0.5f;
        }
        return f;
    }

    public static float Fbm(Vector2 coord, int octave)
    {
        var f = 0.0f;
        var w = 0.5f;
        for (var i = 0; i < octave; i++)
        {
            f += w * Noise(coord);
            coord *= 2.0f;
            w *= 0.5f;
        }
        return f;
    }

    public static float Fbm(float x, float y, int octave)
    {
        return Fbm(new Vector2(x, y), octave);
    }

    public static float Fbm(Vector3 coord, int octave)
    {
        var f = 0.0f;
        var w = 0.5f;
        for (var i = 0; i < octave; i++)
        {
            f += w * Noise(coord);
            coord *= 2.0f;
            w *= 0.5f;
        }
        return f;
    }

    public static float Fbm(float x, float y, float z, int octave)
    {
        return Fbm(new Vector3(x, y, z), octave);
    }

    #endregion

    #region Private functions

    static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }
    
    static float FadeDerivative(float t)
    {
        return 30 * t * t * (t - 1) * (t - 1);
    }

    static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    static float Grad(int hash, float x)
    {
        return (hash & 1) == 0 ? x : -x;
    }

    static float Grad(int hash, float x, float y)
    {
        return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
    }

    static float Grad(int hash, float x, float y, float z)
    {
        var h = hash & 15;
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    static int[] perm = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
        151
    };

    #endregion
}


public static class BaseSurfaceNoise
{
    public static float GetPlainSmallNoise(Vector2 pos, GlobalParams param) {
        float total = 0;
        float frequency = 1 / param.plain_noise_scale;
        float amplitude = param.plain_noise_amplify;
        int octaves = param.plain_noise_octaves;

        float persistence = param.plain_noise_persistence;
        float lacunarity = param.plain_noise_lacunarity;
        Vector2 offset = param.plain_noise_offset;
        
        float maxValue = 0;  // 用于归一化结果

        for(int i = 0; i < octaves; i++) {
            total += Mathf.PerlinNoise(pos.x * frequency + offset.x, pos.y * frequency + offset.y) * amplitude;
        
            maxValue += amplitude;
        
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        maxValue = (maxValue == 0) ? 0.1f : maxValue;

        return param.plain_noise_hill_curve.Evaluate(total / maxValue);
    }

    private static List<List<float>> Generate01PerlinFbmNoiseMap(GlobalParams global_params) {
        List<List<float>> noise_map = new List<List<float>>();

        var width = global_params.width;
        var height = global_params.length;
        
        var seed = global_params.base_noise_seed;
        var scale = global_params.base_noise_scale;
        var octaves = global_params.base_noise_octaves;
        var persistence = global_params.base_noise_persistence;
        var lacunarity = global_params.base_noise_lacunarity;
        var offset = global_params.base_noise_offset;

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
            scale = 0.0001f;

        float max_local_noise_height = float.MinValue;
        float min_local_noise_height = float.MaxValue;

        float half_width = width / 2f;
        float half_height = height / 2f;
        
        // Generate height noise map
        for (int y = 0; y < height; y++) {
            var line = new List<float>();

            for (int x = 0; x < width; x++) {                
                float amplitude = 1;
                float frequency = 1;
                
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - half_width) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - half_height) / scale * frequency + octaveOffsets[i].y;

                    // range -1 to 1
                    // float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    float perlinValue = Perlin.NoiseWithGradient(sampleX, sampleY).x;

                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > max_local_noise_height)
                    max_local_noise_height = noiseHeight;
                if (noiseHeight < min_local_noise_height)
                    min_local_noise_height = noiseHeight;

                line.Add(noiseHeight);
            }
            
            noise_map.Add(line);
        }

        // Clamp ALL value to 0 ~ 1
        for (int x = 0; x < height; x++) {
            for (int y = 0; y < width; y++) {
                noise_map[x][y] = Mathf.InverseLerp(min_local_noise_height, max_local_noise_height, noise_map[x][y]);
            }
        }

        return noise_map;
    }

    public static List<List<float>> GeneratePerlinNoiseMap(GlobalParams global_params) {
        List<List<float>> perlin_noise_map = Generate01PerlinFbmNoiseMap(global_params);

        var length = global_params.length;
        var width = global_params.width;
        var amplitude = global_params.base_noise_amplify;
        var heightCurve = global_params.base_noise_curve;

        for (int x = 0; x < length; x++) // 0~250
        {
            for (int y = 0; y < width; y++) {
                perlin_noise_map[x][y] = heightCurve.Evaluate(perlin_noise_map[x][y]) * amplitude;
            }
        }

        return perlin_noise_map;
    }

    // public static List<List<float>> GenerateRidgeNoiseMap(GlobalParams global_params) {
    //
    //     List<List<float>> ridge_noise_map = Generate01PerlinFbmNoiseMap(global_params);
    //
    //     var length = global_params.length;
    //     var width = global_params.width;
    //     var amplitude = global_params.bs_amplitude;
    //     
    //     var heightCurve = global_params.height_curve;
    //
    //     for (int x = 0; x < length; x++) // 0~250
    //     {
    //         for (int y = 0; y < width; y++) {
    //             float temp = ridge_noise_map[x][y];
    //             temp = 1 - Mathf.Abs(temp);
    //             temp *= temp;
    //             temp = heightCurve.Evaluate(temp);
    //             temp = temp * 2f * amplitude - amplitude;
    //             ridge_noise_map[x][y] = temp;
    //         }
    //     }
    //
    //     return ridge_noise_map;
    // }

    public static List<List<float>> GenerateGradientTrickNoiseMap(GlobalParams global_params)
    {
        /*
         * Gradient Trick Algorithm 思路：
         *      关键在于得到每一个octave层的梯度
         *      梯度越大，越减少该基础层的影响
         * 
         */
        List<List<float>> noiseMap = new List<List<float>>();
    
        var width = global_params.width;
        var height = global_params.length;

        var seed = global_params.base_noise_seed;
        var scale = global_params.base_noise_scale;
        var octaves = global_params.base_noise_octaves;
        var persistence = global_params.base_noise_persistence; // 0.5 around
        var lacunarity = global_params.base_noise_lacunarity;   // 2.0 around
        var offset = global_params.base_noise_offset;

        System.Random prng = new System.Random(Random.Range(1, 100000));
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
            scale = 0.0001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        
        // Generate height noise map
        for (int y = 0; y < height; y++) {
            var valueLine = new List<float>();

            for (int x = 0; x < width; x++) {                
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                Vector2 noiseGradient = new Vector2(0, 0);
                
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    // range -1 to 1
                    // float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    Vector3 perlinValueWithGradient = Perlin.NoiseWithGradient(sampleX, sampleY);
                    float perlinValue = perlinValueWithGradient.x * 2 - 1;
                    Vector2 perlinGradient = new Vector2(perlinValueWithGradient.y, perlinValueWithGradient.z);
                    perlinGradient = perlinGradient * 2.0f;

                    noiseGradient += perlinGradient;
                    noiseHeight += perlinValue * amplitude * ExpDecayFunction(global_params.gradient_trick_decay, 
                        noiseGradient.magnitude);
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                    maxLocalNoiseHeight = noiseHeight;
                if (noiseHeight < minLocalNoiseHeight)
                    minLocalNoiseHeight = noiseHeight;

                valueLine.Add(noiseHeight); 
            }
            noiseMap.Add(valueLine);
        }

        AnimationCurve heightCurve = global_params.base_noise_curve;
        // Clamp ALL value to 0 ~ 1
        for (int x = 0; x < height; x++) {
            for (int y = 0; y < width; y++) {
                noiseMap[x][y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x][y]);
                noiseMap[x][y] = heightCurve.Evaluate(noiseMap[x][y]) * global_params.base_noise_amplify;
            }
        }

        return noiseMap;
    }
    
    // Gradient Trick方法用的反比函数，k为参数，m为输入值
    // 公式： f(n) = 1 / (1 + km)
    public static float BaseDecayFunction(float k, float m)
    {
        return 1.0f / (1.0f + k * m);
    }
    // 公式： f(n) = e ^{-k * m^2}
    public static float ExpDecayFunction(float k, float m)
    {
        return Mathf.Exp(-k * m * m);
    }

    
    // 下面都是DLA算法的部分
    // public static List<List<float>> GenerateDLANoiseMap(GlobalParams global_params)
    // {
    //     var finalLength = global_params.length;
    //     var finalWidth = global_params.width;
    //     
    //     var processLevel = global_params.DLA_process_level;
    //     var occupancyPercent = global_params.DLA_occupantion_percent;
    //
    //     var currentLength = (int)Math.Ceiling(finalLength / Math.Pow(2, processLevel));
    //     var currentWidth = (int)Math.Ceiling(finalWidth / Math.Pow(2, processLevel));
    //
    //     var randSeed = global_params.DLA_random_seed;
    //     var prng = new System.Random(randSeed);
    //
    //     List<List<float>> initialMap = new List<List<float>>(currentLength);
    //     List<List<float>> blurryMap = new List<List<float>>(currentLength);;
    //     List<List<float>> crispMap = new List<List<float>>(currentLength);;
    //     for (int i = 0; i < currentLength; i++)
    //     {
    //         initialMap[i] = new List<float>(currentWidth);
    //         blurryMap[i] = new List<float>(currentWidth);
    //         crispMap[i] = new List<float>(currentWidth);
    //     }
    //
    //     RunDLAInPlace(initialMap, occupancyPercent, randSeed++, true);
    //     blurryMap = UpscaleAndBlurMap(initialMap);
    //     crispMap = UpscaleAndCrispMap(initialMap);
    //     initialMap = CombineBlurAndCrispMap(blurryMap, crispMap);
    //     
    //     currentLength = initialMap.Count;
    //     currentWidth = initialMap[0].Count;
    //
    //     while (currentLength < finalLength || currentWidth < finalWidth)
    //     {
    //         blurryMap = UpscaleAndBlurMap(initialMap);
    //         crispMap = UpscaleAndCrispMap(crispMap);
    //         RunDLAInPlace(crispMap, occupancyPercent, randSeed++, false);
    //         initialMap = CombineBlurAndCrispMap(blurryMap, crispMap);
    //         
    //         currentLength = initialMap.Count;
    //         currentWidth = initialMap[0].Count;
    //     }
    //
    //     initialMap = ResizeMap(finalLength, finalWidth, initialMap);
    //
    //     return initialMap;
    // }
    //
    // private static void RunDLAInPlace(List<List<float>> map, float occupancyPercent, int seed = 114514, bool isFirst = false)
    // {
    //     var prng = new System.Random(seed);
    //     var length = map.Count;
    //     var width = map[0].Count;
    //
    //     int x, y;
    //     int total = prng.Next(length * width - (int)(length * width * occupancyPercent),
    //         length * width);
    //
    //     if (isFirst)
    //     {
    //         x = prng.Next(length / 4, 3 * length / 4);
    //         y = prng.Next(width / 4, 3 * width / 4);
    //         
    //         map[x][y] = 1.0f;
    //     }
    //
    //     while (total < length * width)
    //     {
    //         x = prng.Next(length);
    //         y = prng.Next(width);
    //
    //         if (map[x][y] > 0.0f)
    //         {
    //             continue;
    //         }
    //
    //         // Random Walk
    //         bool hit = false;
    //         while (!hit)
    //         {
    //             int prevX = x;
    //             int prevY = y;
    //             int[] newValues = { prng.Next(-1, 2), prng.Next(-1, 2) };
    //
    //             if (newValues[0] == 0 && newValues[1] == 0)
    //             {
    //                 newValues[prng.Next(2)] = prng.Next(2) == 0 ? 1 : -1;
    //             }
    //
    //             x += newValues[0];
    //             y += newValues[1];
    //
    //             if (x < 0 || y < 0 || x > length - 1 || y > width - 1)
    //             {
    //                 break;
    //             }
    //
    //             if (map[x][y] > 0.0f)
    //             {
    //                 hit = true;
    //                 total += 1;
    //                 map[x][y] += 1.0f;
    //                 
    //             }
    //         }
    //
    //     }
    //     
    // }
    //
    // private static List<List<float>> UpscaleAndBlurMap(List<List<float>> map)
    // {
    //
    //     return map;
    // }
    //
    // private static List<List<float>> UpscaleAndCrispMap(List<List<float>> map)
    // {
    //
    //     return map;
    // }
    //
    // private static List<List<float>> CombineBlurAndCrispMap(List<List<float>> blurryMap, List<List<float>> crispMap)
    // {
    //     var length = blurryMap.Count;
    //     var width = blurryMap[0].Count;
    //
    //     List<List<float>> newMap = new List<List<float>>(length);
    //
    //     for (int x = 0; x < length; x++)
    //     {
    //         newMap[x] = new List<float>(width);
    //         for (int y = 0; y < width; y++)
    //         {
    //             newMap[x][y] = blurryMap[x][y] + crispMap[x][y];
    //         }
    //     }
    //
    //     return blurryMap;
    // }
    //
    //
    // private static List<List<float>> ResizeMap(int targetLength, int targetWidth, List<List<float>> map)
    // {
    //     List<List<float>> resizedMap = new List<List<float>>();
    //
    //     for (int i = 0; i < targetLength; i++)
    //     {
    //         // 如果原始地图的行数不足，添加新的行
    //         if (i >= map.Count)
    //         {
    //             List<float> newRow = new List<float>(new float[targetWidth]);  // 使用0初始化新行
    //             resizedMap.Add(newRow);
    //         }
    //         else
    //         {
    //             List<float> currentRow = map[i];
    //             List<float> resizedRow = new List<float>();
    //
    //             for (int j = 0; j < targetWidth; j++)
    //             {
    //                 if (j >= currentRow.Count)
    //                 {
    //                     resizedRow.Add(0.0f);
    //                 }
    //                 else
    //                 {
    //                     resizedRow.Add(currentRow[j]);
    //                 }
    //             }
    //             resizedMap.Add(resizedRow);
    //         }
    //     }
    //
    //     return resizedMap;
    // }
    //
}



public static class BaseAlgorithm
{
    public class Line2D
    {
        public Vector2 pt1 = Vector2.zero;
        public Vector2 pt2 = Vector2.zero;
    }
    
    public static Vector2 RotateVector2D(Vector2 v, float angle)
    {
        return new Vector2(
            v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle),
            v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle));
    }

    public static (float, float) DistanceFromPointToLineSeg(Vector2 pt, Vector2 linePt0, Vector2 linePt1)
    {
        float x = pt.x, y = pt.y;
        float x1 = linePt0.x, y1 = linePt0.y;
        float x2 = linePt1.x, y2 = linePt1.y;

        var A = x - x1;
        var B = y - y1;
        var C = x2 - x1;
        var D = y2 - y1;

        float dot = A * C + B * D;
        float len_sq = C * C + D * D;
        float param = -1;
        if (len_sq != 0)     //in case of 0 length line
            param = dot / len_sq;

        float xx, yy;

        if (param < 0)
        {
            xx = x1;
            yy = y1;
        }
        else if (param > 1)
        {
            xx = x2;
            yy = y2;
        }
        else
        {
            xx = x1 + param * C;
            yy = y1 + param * D;
        }

        var dx = x - xx;
        var dy = y - yy;
        return (Mathf.Sqrt(dx * dx + dy * dy), param);
    }

    public static List<string> QueryAllFilesUnderDirectory(string dirPath, string fileSuffix)
    {
        System.IO.DirectoryInfo direction = new System.IO.DirectoryInfo(dirPath);
        System.IO.FileInfo[] files = direction.GetFiles("*", System.IO.SearchOption.TopDirectoryOnly);
        Debug.Log(files.Length);

        List<string> queried_files = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            //判断文件的后缀
            if (files[i].Name.EndsWith(fileSuffix))
            {
                queried_files.Add(files[i].Name); 

            }
        }
        return queried_files;
    }

    public static List<List<float>> GaussianBlur(List<List<float>> heightMap, int radius) {
        var length = heightMap.Count;
        var width = heightMap[0].Count;

        // 初始化
        List<List<float>> blurred_map = new List<List<float>>(length);
        for (int x = 0; x < length; x++) {
            blurred_map.Add(new List<float>(Enumerable.Repeat(0f, width)));
        }

        int halfRadius = radius / 2;
        float sigma = radius / 2.0f;
        float sumTotal = 0;
        float[,] kernel = new float[radius, radius];

        for (int filterY = -halfRadius; filterY <= halfRadius; filterY++) {
            for (int filterX = -halfRadius; filterX <= halfRadius; filterX++) {
                float distance = (filterX * filterX + filterY * filterY) / (2 * sigma * sigma);
                kernel[filterY + halfRadius, filterX + halfRadius] = Mathf.Exp(-distance);
                sumTotal += kernel[filterY + halfRadius, filterX + halfRadius];
            }
        }
        
        for (int y = 0; y < radius; y++) {
            for (int x = 0; x < radius; x++) {
                kernel[y, x] *= 1.0f / sumTotal;
            }
        }
        
        // blur
        for (int x = 0; x < length; x++) {
            for (int y = 0; y < width; y++) {
                float sum = 0;
                for (int filterY = -halfRadius; filterY <= halfRadius; filterY++) {
                    int imageY = y + filterY;
                    if (imageY < 0) imageY = 0;
                    if (imageY >= width) imageY = width - 1;

                    for (int filterX = -halfRadius; filterX <= halfRadius; filterX++) {
                        int imageX = x + filterX;
                        if (imageX < 0) imageX = 0;
                        if (imageX >= length) imageX = length - 1;

                        sum += heightMap[imageX][imageY] * kernel[filterY + radius / 2, filterX + radius / 2];
                    }
                }

                blurred_map[x][y] = sum;
            }
        }

        return blurred_map;
    }
}


