using System;
using System.ComponentModel;
using System.Data.Common;
using System.Text;

using Microsoft.VisualBasic;

class CleanComparer
{
    static void Main(string[] args)
    {
        int length = 6;
        int NoLists = 3;
        CSVListGenerator(length, NoLists);

        //Reading from a CSV
        int[][] lists = new int[NoLists][];
        var rows = File.ReadAllLines($"lists.csv").Skip(1);
        int count = 0;
        foreach (var row in rows)//look through each row in file
        {
            lists[count++] = Array.ConvertAll(row.Split(","), int.Parse);//makes a 1d array of that row of ints
        }

        //Normal stuff

    }
    static void CSVListGenerator(int length, int NoLists)
    {
        var lists = new StringBuilder();
        int[] OGlist = new int[length];
        for (int i = 0; i < length; i++)
        {
            OGlist[i] = i;
        }
        for (int i = 0; i < NoLists; i++)
        {
            if (i<NoLists-1)
                lists.AppendLine($"List{i}, ");
            else
            lists.AppendLine($"List{i}");
            lists.Append(RandomizedCopy(OGlist));
        }

        File.WriteAllText("lists.csv", lists.ToString());
    }
    static int[] RandomizedCopy(int[] list)
    {   
        int[] Copy=list;
        List<int> Used = new List<int>{};
        int[] RandCopy = new int[list.Length];
        for (int i = 0; i < list.Length; i++)//swap every item in list1 for a random one from its copy
        {
            int index = ExclusiveRandomIndex(Used, list, i);
            if (index==-1)
                Console.WriteLine("RandomIndex for loop did not return index");
            RandCopy[i] = Copy[index];//index should be different each time
        }
        return RandCopy;
    }
    static int ExclusiveRandomIndex(List<int> used, int[] list, int i)
    {
        Random rand = new Random();
        int RandomInt = rand.Next(0, list.Length);
        bool FoundInUsed=false;//assume it is not yet found
        for (int j = 0; j < used.Count; j++)//cycle through used indeces, if it is in there then it is not used
        {
            if (used[j] == RandomInt)
                FoundInUsed=true;//index is already used
        }
        if (FoundInUsed)
            return ExclusiveRandomIndex(used, list, i);//find another index
        else
        {
            used.Add(RandomInt);
            return RandomInt;
        }
    }
}
