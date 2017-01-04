using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public static class IEnumerableShuffle
{
    //RNGCSP随机生成器
    private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

    //Fisher-Yates Shuffle算法
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
    {
        T[] array = collection.ToArray();
        for (int i = 0; i < array.Length - 1; i++)
        {
            byte[] bytes = new byte[sizeof(int)];
            rngCsp.GetBytes(bytes);
            int j = i + Roll(array.Length - i);
            Swap<T>(ref array[i], ref array[j]);
        }
        return array.AsEnumerable();
    }

    private static void Swap<T> (ref T a, ref T b)
    {
        T c = a;
        a = b;
        b = c;
    }

    //生成[0, max)之间的随机整数
    public static int Roll(int max)
    {
        int value = -1;
        if (max <= 0)
        {
            return value;
        }
        byte[] bytes = new byte[sizeof(int)];
        do
        {
            rngCsp.GetBytes(bytes);
            value = BitConverter.ToInt32(bytes, 0) & 0x7fffffff;
        }
        while (value >= max * (int.MaxValue / max));
        return value % max;
    }
}
