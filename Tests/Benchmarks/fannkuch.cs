﻿/* The Computer Language Benchmarks Game
   http://shootout.alioth.debian.org/

   contributed by Isaac Gouy, transliterated from Mike Pall's Lua program 
*/

using System;

class Program
{
    public static int[] fannkuch(int n)
    {
        int[] p = new int[n];
        int[] q = new int[n];
        int[] s = new int[n];
        int sign = 1, maxflips = 0, sum = 0, m = n - 1;
        for (int i = 0; i < n; i++)
        {
            p[i] = i;
            q[i] = i;
            s[i] = i;
        }
        do
        {
            // Copy and flip.
            int q0 = p[0];                                     // Cache 0th element.
            if (q0 != 0)
            {
                for (int i = 1; i < n; i++)
                    q[i] = p[i];             // Work on a copy.
                int flips = 1;
                do
                {
                    int qq = q[q0];
                    if (qq == 0)
                    {                                // ... until 0th element is 0.
                        sum += sign * flips;
                        if (flips > maxflips)
                            maxflips = flips;   // New maximum?
                        break;
                    }
                    q[q0] = q0;
                    if (q0 >= 3)
                    {
                        int i = 1, j = q0 - 1, t;
                        do
                        {
                            t = q[i];
                            q[i] = q[j];
                            q[j] = t;
                            i++;
                            j--;
                        }
                        while (i < j);
                    }
                    q0 = qq; flips++;
                } while (true);
            }
            // Permute.
            if (sign == 1)
            {
                int t = p[1];
                p[1] = p[0];
                p[0] = t;
                sign = -1; // Rotate 0<-1.
            }
            else
            {
                int t = p[1];
                p[1] = p[2];
                p[2] = t;
                sign = 1;  // Rotate 0<-1 and 0<-1<-2.
                for (int i = 2; i < n; i++)
                {
                    int sx = s[i];
                    if (sx != 0)
                    {
                        s[i] = sx - 1;
                        break;
                    }
                    if (i == m)
                    {
                        int[] _T = new int[2];
                        _T[0] = sum;
                        _T[1] = maxflips;
                        return _T;// Out of permutations.
                    }
                    s[i] = i;
                    // Rotate 0<-...<-i+1.
                    t = p[0];
                    for (int j = 0; j <= i; j++)
                    {
                        p[j] = p[j + 1];
                    }
                    p[i + 1] = t;
                }
            }
        } while (true);
    }

    static void Main()
    {
        int n = 12;
        var pf = fannkuch(n);
        Console.WriteLine(pf[0]);
        Console.Write("Pfannkuchen(");
        Console.Write(n);
        Console.Write(") = ");
        Console.WriteLine(pf[1]);
    }
}
