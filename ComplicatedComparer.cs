using System;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
//This compares two permutations of a list where each list has no repeating items.
//It only makes the final score of comparison based on the distances between each pair of the same item from each list.
//A lot of the algorithms here assume the use of 2d arrays where the second spot stores the initial position index.
//A general case would be using two lists that are randomized versions of a sequential list.
//I found that comparing a list with a reversed version of itself will always give a similarity of 50.2%, which I have not worked out why
//I found that comparing a list with a version where each item is shifted half up the list's length always gives exactly 50%. This makes inuitive sense.
class ComplicatedComparer
{
    static void Main(string[] args)
    {
        int Length = 12;
        int[] list = new int[Length];
        for (int i = 0; i < Length; i++)
        {
            list[i] = i;
        }
        int[] list1 = list;//RandomizedCopy(list);
        int[] list2 = ReversedCopy(list1);
        int[,] SortedList2 = MergeSort(AddInitialIndex(list2));
        int searchFor=2;
        Console.WriteLine($"List1={PrintArray(AddInitialIndex(list1))}\nList2={PrintArray(AddInitialIndex(list2))}\nSorted list2={PrintArray(SortedList2)}\nA {searchFor} is in spot {BinaryIndexSearch(SortedList2, searchFor, 0, SortedList2.GetLength(0)-1)} of list 2");
        //Console.WriteLine($"Reverse of list2 = {PrintArray(AddInitialIndex(ReversedCopy(list2)))}");
        double FinalScore=Similarity(list1, list2);
        if (FinalScore!=-1)
            Console.WriteLine($"The ordering similarity of list1 and list2 is {FinalScore}%");

    }
    static double Similarity(int[] list1, int[] list2)
    {
        if (list1.Length != list2.Length)
        {
            Console.WriteLine("Lists are not the same length");
            return -1;
        }
        int Length=list1.Length;
        int[,] IndexedList1 = AddInitialIndex(list1);
        int[,] IndexedList2 = AddInitialIndex(list2);
        int[,] sorted1 = MergeSort(IndexedList1);
        int[,] sorted2 = MergeSort(IndexedList2);//only make a sorted and indexed version of one of the lists, as we search for each item in list1 in the sorted list 2
        int PairCount=0;
        double scores = 0;
        bool match = false;//not yet a match at the start of every loop through list2
        for (int i = 0; i < Length; i++)
        {
            int Index1 = BinaryIndexSearch(sorted1, i, 0, Length);
            int Index2 = BinaryIndexSearch(sorted2, i, 0, Length);     
            if (Index1 != -1 || Index2 != -1)
            {
                scores+=1-Math.Abs(Index1-Index2)/(double)Length;//i - index of list1[i] in list 2, therefore is the difference of the same items index
                PairCount++;
                match=true;//new pair so count can go back to 0                        
            } 
        }
        if (!match)//when it has cycled through all values in list2 and there are no pairs with list1 even though list1 still has more possible pairs
        {
            Console.WriteLine("List1 contains items that are not within list2");
            return -1;
        }
        if (PairCount<Length)
            Console.WriteLine("Something has gone wrong, there are more values in the lists than pairs made");
        else if (PairCount>Length)
            Console.WriteLine("something is very very wrong, more pairs than items in the lists");
        return scores/Length*100;
    }
    static int[] ReversedCopy(int[] list)// when list is compared to ReversedCopy(list) it gives a similarity of 50.2% every time, interesting that it is not 50%, though I have not analyzed my algorithm to see the minimum possible yet
    {
        int Length=list.Length;
        int[] Reversed = new int[Length];
        for (int i = 0; i<Length; i++)
        {
            Reversed[Length-1-i]=list[i];//since highest index is Length-1
        }
        return Reversed;
    }
    static int[] ShiftHalfCopy(int[] list)
    // it is pretty cool, as I intuitively expected this gives a similarity of 50%. Meaning that many mid length distances are better than longest and shortest distances
    //Idea here is to make a list where every item is Length/2 away from itself
    {
        int Length = list.Length;
        int[] Shifted = new int[Length];
        for (int i = 0; i< Length; i++)
        {
            try
            {
                Shifted[i] = list[i+Length/2];
            }
            catch//if i+Length/2>Length
            {
                Shifted[i] = list[i-Length/2];
            }
        }
        return Shifted;
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
    static int FindIndex(int[] list, int item)//neds to make a dictionary of the list before it gets sorted so the data of initial index is not lost
    {
        //since the final search will look for the data value in the dictionary
        int[,] IndexedList = AddInitialIndex(list);
        return BinaryIndexSearch(MergeSort(IndexedList), item, 0, IndexedList.GetLength(0));
    }
    static int[,] AddInitialIndex(int[] list)
    {
        int Length = list.GetLength(0);
        int[,] IndexedList = new int[Length, 2];
        for (int i=0; i<Length;i++)
        {   // The second digit is what defines whether it is value or initial index, 0 - value, 1 - initial index
            IndexedList[i, 0] = list[i];
            IndexedList[i, 1] = i;
        }
        return IndexedList;
    }
    static int BinaryIndexSearch(int[,] list, int item, int bottom, int top)//using copy this time bcs idw be special
    {
        if (bottom>top)//this base condition is different to that in the version that halves the array multiple times.
            return -1;
        int mid = (top+bottom)/2;// i initially though to do top-bottom, but that just gives the length of mid for a half array, we want the position of mid for the whole
        if (list[mid, 0]==item)
        {
            return list[mid, 1];//should return the intial index from before sorting
        }
        else if (item>list[mid, 0])
        {
            return BinaryIndexSearch(list, item, mid+1, top);
        }
        else
        {
            return BinaryIndexSearch(list, item, bottom, mid-1);
        }
    }
    //With binary search i was not thinking in recursion once again, I should have thought about what info you need for one recurse and put that into the method
    //Furthermore, we don't need to set top and bottom as bounds, just use them to move mid until it reaches item
    static int[,] merge(int[,] list1, int[,] list2)
    {
        int Length1 = list1.GetLength(0);
        int Length2 = list2.GetLength(0);
        int[,] Combination = new int[Length1+Length2, 2];        
        int k = 0;
        int i = 0;
        int j = 0;
        while (i<Length1 && j<Length2)//cannot use for as j or i should only increase when their respective array is used
        {
            if (list1[i, 0]<=list2[j, 0])
            {
                Combination[k, 0]=list1[i, 0];//k must go higher than i or j
                Combination[k, 1]=list1[i, 1];
                i++;
            }
            else 
            {
                Combination[k, 0]=list2[j, 0];
                Combination[k, 1]=list2[j, 1];
                j++;
            }
            k++;
        }// but there may still be one left over of one of the lists (as the while loop stops when either reaches their limit)
        // this one element must be added, it should already be the largest since the smallest are already in combination
        while (i<Length1)
        {
            Combination[k, 0]=list1[i, 0];//k must go higher than i or j
            Combination[k, 1]=list1[i, 1];// must also move og index
            i++; k++;       
        }
        while (j<Length2)
        {
            Combination[k, 0]=list2[j, 0];
            Combination[k, 1]=list2[j, 1];
            j++; k++;
        }
        return Combination;
    }
    static int[,] MergeSort(int[,] list)
    {
        int Length = list.GetLength(0);//still number of items, despite 2 dimensions
        if (Length<=1)
            return list;
        
        int[,] Half1 = new int[Length/2, 2];
        int[,] Half2 = new int[Length-Length/2, 2];

        for (int i = 0; i < Length/2; i++)// these two split up the array
        {
            Half1[i, 0] = list[i, 0];
            Half1[i, 1] = list[i, 1];
        }
        for (int i = 0; i < Length-Length/2; i++)
        {
            Half2[i, 0] = list[i+Length/2, 0];
            Half2[i, 1] = list[i+Length/2, 1];
        }
        
        //The array is split in two, but we must repeat this below, then in each
        //recursion of mergesort it will also merge until it bubbles back up the chain
        //The trick is that the merge sort function is just base case, split, recurse, merge
        //and the recurse will continue until the final merge
        Half1 = MergeSort(Half1);
        Half2 = MergeSort(Half2);

        return merge(Half1, Half2);
    }
    static string PrintArray(int[,] list)
    {
        int Length=list.GetLength(0);
        var Text = new System.Text.StringBuilder();
        int i;
        for (i = 0; i< Length-1; i++)
        {
            Text.Append($"{list[i, 0]}, ");        
        }
        Text.Append(list[i++, 0]);
        return Text.ToString();
    }
}
