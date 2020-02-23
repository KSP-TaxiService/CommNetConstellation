using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace CommNetConstellation.UI
{
    public class GameUtils
    {
        /// <summary>
        /// Fast sorting algorithm in Computer Science area
        /// </summary>
        //source: http://snipd.net/quicksort-in-c
        public static void Quicksort(short[] elements, int left, int right)
        {
            if (elements.Length <= 1)
            {
                return;
            }

            int i = left, j = right;

            while (i <= j)
            {
                while (elements[i] < elements[(left + right) / 2])
                {
                    i++;
                }

                while (elements[j] > elements[(left + right) / 2])
                {
                    j--;
                }

                if (i <= j)
                {
                    // Swap
                    short tmp = elements[i];
                    elements[i] = elements[j];
                    elements[j] = tmp;

                    i++;
                    j--;
                }
            }

            // Recursive calls
            if (left < j)
            {
                Quicksort(elements, left, j);
            }

            if (i < right)
            {
                Quicksort(elements, i, right);
            }
        }

        /// <summary>
        /// Return index of first common element in first sorted array or -1 for no match
        /// </summary>
        public static int firstCommonElement(short[] sortedArray1, short[] sortedArray2)
        {
            int arrayIndex1 = 0, arrayIndex2 = 0;
            while (arrayIndex1 < sortedArray1.Length && arrayIndex2 < sortedArray2.Length)
            {
                if(sortedArray1[arrayIndex1] == sortedArray2[arrayIndex2])
                {
                    return arrayIndex1;
                }
                else if(sortedArray1[arrayIndex1] < sortedArray2[arrayIndex2])
                {
                    arrayIndex1++;
                }
                else
                {
                    arrayIndex2++;
                }
            }

            return -1;
        }

        public static bool NonLinqAny(List<Constellation> constellations, short givenFrequency)
        {
            for (int i = 0; i < constellations.Count; i++)
            {
                if (constellations[i].frequency == givenFrequency)
                    return true;
            }
            return false;
        }

        public static short[] NonLinqIntersect(short[] sortedArray1, short[] sortedArray2)
        {
            if (sortedArray1.Length == 0 || sortedArray2.Length == 0)
                return new short[] { };

            int aIndex = 0, bIndex = 0;
            List<short> commonFreqs = new List<short>(Math.Max(sortedArray1.Length, sortedArray2.Length));
            while (aIndex < sortedArray1.Length && bIndex < sortedArray2.Length)
            {
                if (sortedArray1[aIndex] < sortedArray2[bIndex])
                {
                    aIndex++;
                }
                else if (sortedArray1[aIndex] > sortedArray2[bIndex])
                {
                    bIndex++;
                }
                else if (sortedArray1[aIndex] == sortedArray2[bIndex] && !commonFreqs.Contains(sortedArray1[aIndex]))
                {
                    commonFreqs.Add(sortedArray1[aIndex]);
                    aIndex++;
                    bIndex++;
                }
            }

            return commonFreqs.ToArray();
        }

        public static double NonLinqSum(List<double> list)
        {
            double sum = 0.0;
            for (int i = 0; i < list.Count; i++)
                sum += list[i];
            return sum;
        }

        public static double NonLinqMax(List<double> list)
        {
            double max = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                if (max < list[i])
                    max = list[i];
            }
            return max;
        }

        /// <summary>
        /// Returns the current AssemplyFileVersion from AssemblyInfos.cs
        /// </summary>
        public static string Version
        {
            get
            {
                Assembly executableAssembly = Assembly.GetExecutingAssembly();
                return "v" + FileVersionInfo.GetVersionInfo(executableAssembly.Location).ProductVersion;
            }
        }
    }
}
