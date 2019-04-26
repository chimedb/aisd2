using System;
using System.Collections.Generic;

namespace ASD
{
    public class LZ77 : MarshalByRefObject
    {
        /// <summary>
        /// Odkodowywanie napisu zakodowanego algorytmem LZ77. Dane kodowanie jest poprawne (nie trzeba tego sprawdzać).
        /// </summary>
        public string Decode(List<EncodingTriple> encoding)
        {
            int n = encoding.Count;
            if (n == 0) return "";
            int length = n;
            for (int i = 0; i < n; i++)
                length += encoding[i].c;
            
            char[] array = new char[length];
            array[0] = encoding[0].s;

            int current = 0;
            for (int i = 1; i < n; i++)
            {
                int start = current - encoding[i].p;
                for (int j = 0; j < encoding[i].c; j++)
                    array[++current] = array[start + j];
            
                array[++current] = encoding[i].s;
            }

            return new string(array);
        }

        /// <summary>
        /// Kodowanie napisu s algorytmem LZ77
        /// </summary>
        /// <returns></returns>
        public List<EncodingTriple> Encode(string s, int maxP)
        {
            if (s == "") return null;
            List<EncodingTriple> ans = new List<EncodingTriple>
            {
                new EncodingTriple(0, 0, s[0])
            };
            int n = s.Length, encoded = 1;
            while (encoded < n)
            {
                int length = encoded - 1;
                if (length > maxP) length = maxP;
                int[] lps = new int[n - encoded + 1];
                bool[] check = new bool[n - encoded + 1];
                lps[0] = lps[1] = 0;
                check[0] = check[1] = true;
                int i = 0, j = 0, c = 0, startId = 0;
                for (j = i = 0; i <= length; i += j == 0 ? 1 : j - lps[j])
                {
                    for (j = lps[j]; j < n - encoded && s[encoded - length - 1 + i + j] == s[encoded + j]; ++j);
                    if (j > c)
                    {
                        c = j;
                        startId = i;
                    }

                    if (!check[j])
                    {
                        //int len = 0, k = 2;
                        //while (k <= j)
                        //{
                        //    check[k] = true;
                        //    if (s[encoded + k - 1] == s[encoded + len])
                        //    {
                        //        len++;
                        //        lps[k] = len;
                        //        k++;
                        //    }
                        //    else
                        //    {
                        //        if (len != 0) len = lps[len - 1];
                        //        else
                        //        {
                        //            lps[k] = len;
                        //            k++;
                        //        }
                        //    }
                        //}
                        int t = 0;
                        for (int l = 2; l <= j; ++l)
                        {
                            while (t > 0 && s[encoded + t] != s[encoded + l - 1])
                                t = lps[t];
                            if (s[encoded + t] == s[encoded + l - 1]) ++t;
                            lps[l] = t;
                            check[l] = true;
                        }
                    }
                }
                if (c == s.Length - encoded) c--;
                ans.Add(new EncodingTriple(length - startId, c, s[encoded + c]));
                encoded += c + 1;
            }
            return ans;
        }

        public static int[] ComputeLPS(string pat)
        {
            int len = 0, i = 1;
            int n = pat.Length;
            int[] lps = new int[n];
            
            lps[0] = 0;
            while (i < n)
            {
                if (pat[i] == pat[len])
                {
                    len++;
                    lps[i] = len;
                    i++;
                }
                else 
                {
                    if (len != 0) len = lps[len - 1];
                    else 
                    {
                        lps[i] = len;
                        i++;
                    }
                }
            }
            return lps;
        }
    }

    [Serializable]
    public struct EncodingTriple
    {
        public int p, c;
        public char s;

        public EncodingTriple(int p, int c, char s)
        {
            this.p = p;
            this.c = c;
            this.s = s;
        }
    }
}
