using System;
using System.ComponentModel;
using System.Data.Common;
using System.Security.Principal;
using System.Text;
using System.Xml.Schema;
using Microsoft.VisualBasic;
//this is the same general idea as ComplicatedComparer except it uses c#'s abilities and more efficient algorithms, with some added features
//My main issue to deal with was that this time i was storing the two lists in a 2d array, so I needed to use .GetLength(0) and .GetLength(1)
//differently as 0 is the number of rows, and 1 is the length of each row while before I would use 0 as the length of each list.
class CleanComparer
{
    static void Main(string[] args)
    {
        int Length = 20;
        int NoLists = 2;
        int[,] lists = new int[NoLists, Length];
        for (int i =0; i < Length; i++)
        {
            lists[0, i] = i;
        }
        lists = RandomizedCopy(lists, 1, 0);
        ToCSVListConverter(lists);
        int [,] arrays = FromCSVListConverter(NoLists, Length);
        double FinalScore=Similarity(arrays);
        if (FinalScore!=-1)
            Console.WriteLine($"The ordering similarity of list1 and list2 is {FinalScore}%");
    }
    static double Similarity(int[,]list)
    {
        int Length = list.GetLength(1);//number of items in each row
        if (list.GetLength(0)>2)//more than two rows
        {
            Console.WriteLine("Only two lists please");
            return -1;
        }
        else
        {
            double scores = 0;
            int[] List1 = GetRow(list, 0);
            int[] List2 = GetRow(list, 1);
            for (int i = 0; i < Length; i++)
            {
                int Index1 = Array.IndexOf(List1, i);
                int Index2 = Array.IndexOf(List2, i);
                if (Index1 !=-1 && Index2 !=-1)// i is in both lists
                    scores+=1 - Math.Abs(Index1-Index2)/(double)Length;//i - index of list1[i] in list 2, therefore is the difference of the same items index
            }
            return scores/Length*100;
        }
    }
    static int[] GetRow(int[,] list, int row)
    {
        int Length = list.GetLength(1);
        int[] listRow = new int[Length];
        for (int i = 0; i < Length; i++)
        {
            listRow[i] = list[row, i];
        }
        return listRow;
    }
    static int[,] FromCSVListConverter(int NoLists, int Length)
    {
        int[,] lists = new int[NoLists, Length];
        var rows = File.ReadAllLines($"lists.csv").Skip(1);
        rows.Count();
        int count = 0;
        foreach (var row in rows)//look through each row in file
        {
            int[] list = Array.ConvertAll(row.Split(","), int.Parse);//makes a 1d array of that row of ints
            for (int i = 0; i < NoLists; i++)
            {
                lists[i, count]=list[i];//count is the number of rows, so should be the length and therefore how far into lists[i] to place the item list[i]
            }
            count++;
        }
        return lists;
    }
    static void ToCSVListConverter(int[,] listsArray)
    {
        var listsSB = new StringBuilder();
        int NoLists = listsArray.GetLength(0);
        int Length = listsArray.GetLength(1);//assuming each list to be a permutation
        for (int i = -1; i < Length; i++)
        {
            if (i>-1)
                listsSB.AppendLine();
            for (int j = 0; j < NoLists; j++)
            {
                if(i==-1)//title
                {
                    if (i<NoLists-1)
                        listsSB.Append($"List{j},");
                    else
                    listsSB.Append($"List{j}");//makes top row entirely titles
                }
                else if (j!=NoLists-1)//not the end of that row
                {
                   listsSB.Append($"{listsArray[j,i]},");
                }
                else//end of that row
                {
                    listsSB.Append($"{listsArray[j,i]}");
                }
            }
        }
        File.WriteAllText("lists.csv", listsSB.ToString());
    }
    static int[,] RandomizedCopy(int[,] list, int shiftedRow, int initialRow)//i found this other algorithm 'Fisher-Yates' from chatGPT and its pretty cool
    //Randomly pick a number (j) from 0-i, swap the item from j with item at i, this randomly picks numbers to pile at the end.
    //This way there is no need to worry about collisions
    {
        var rand = Random.Shared;//this will produce a new number each time it is called
        int Length = list.GetLength(1);
        int[] listCache = new int[Length];
        for (int i = 0; i < Length; i++)
        {
            listCache[i] = list[initialRow, i];
        }
        for (int i = 0; i< Length; i++)
        {
            int j = rand.Next(i, Length);// [i, Length)
            int intCache = listCache[i];//original item in that place of initial row
            listCache[i]=listCache[j];
            listCache[j] = intCache;
        }
        for (int i =0; i< Length; i++)
        {
            list[shiftedRow, i] = listCache[i];
        }
        return list;
    }
    static int[,] ShiftHalfCopy(int[,] list, int ShiftedRow, int InitialRow)//x, 1, 0
    // it is pretty cool, as I intuitively expected this gives a similarity of 50%. Meaning that many mid length distances are better than longest and shortest distances
    //Idea here is to make a list where every item is Length/2 away from itself
    {
        int Length = list.GetLength(1);//length of each row
        int[] cache = new int[Length];//15 in length
        for (int i = 0; i < Length; i++)//untik i = 14
        {
            cache[i] = list[InitialRow, i];
        }
        for (int i = 0; i < Length; i++)
        {
            int ShiftIndex = (i+Length/2) % Length;//remainder should just be i+Length/2 until i+Length/2=Length, and then it will remaind the 0 -> length/2. Cool maths from the ol' 'GPT
            list[ShiftedRow, i] = cache[ShiftIndex];
        }
        return list;
    }

}
