using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
public class BoxAssigner
{

    //public List<int> AssignNumbers(int boxCount, int lastDigit)
    //{
    //    List<int> numbers = new List<int>();

    //    if (boxCount != lastDigit * 2)
    //    {
    //        throw new ArgumentException("Box count must be twice the value of the last digit.");
    //    }

    //    for (int i = 0; i < boxCount; i++)
    //    {
    //        numbers.Add(i); // Add indices directly (0, 1, 2, ..., 11)
    //    }

    //    numbers = ShuffleList(numbers);

    //    return numbers;
    //}

    //private List<int> ShuffleList(List<int> inputList)
    //{
    //    Random rng = new Random();
    //    int n = inputList.Count;
    //    while (n > 1)
    //    {
    //        n--;
    //        int k = rng.Next(n + 1);
    //        int value = inputList[k];
    //        inputList[k] = inputList[n];
    //        inputList[n] = value;
    //    }

    //    return inputList;
    //}

    public List<int> AssignRandomIndices(List<Sprite> imageArray, List<Sprite> logoArray, int count)
    {
        List<int> indices = new List<int>();

        // Create a list of all possible indices
        List<int> allIndices = Enumerable.Range(0, imageArray.Count).ToList();

        // Shuffle the list of all possible indices
        System.Random rand = new System.Random();
        allIndices = allIndices.OrderBy(x => rand.Next()).ToList();

        // Select the first 'count' indices from the shuffled list
        indices = allIndices.Take(count).ToList();

        return indices;
    }

    // Shuffle the list of images
    public List<Sprite> ShuffleList(List<Sprite> list)
    {
        System.Random rand = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            Sprite value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}
