using System.Collections.Generic;
using UnityEngine;

public class MatchDetector : MonoBehaviour
{
    public List<Food> FindAllMatches(Food[,] grid, int width, int height)
    {
        HashSet<Food> matches = new HashSet<Food>();

        FindHorizontalMatches(grid, width, height, matches);
        FindVerticalMatches(grid, width, height, matches);

        return new List<Food>(matches);
    }

    private void FindHorizontalMatches(Food[,] grid, int width, int height, HashSet<Food> matches)
    {
        for (int y = 0; y < height; y++)
        {
            int matchCount = 1;

            for (int x = 1; x < width; x++)
            {
                Food current = grid[x, y];
                Food previous = grid[x - 1, y];

                if (current != null && previous != null && current.Type == previous.Type)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = 0; i < matchCount; i++)
                        {
                            matches.Add(grid[x - 1 - i, y]);
                        }
                    }

                    matchCount = 1;
                }
            }

            if (matchCount >= 3)
            {
                for (int i = 0; i < matchCount; i++)
                {
                    matches.Add(grid[width - 1 - i, y]);
                }
            }
        }
    }

    private void FindVerticalMatches(Food[,] grid, int width, int height, HashSet<Food> matches)
    {
        for (int x = 0; x < width; x++)
        {
            int matchCount = 1;

            for (int y = 1; y < height; y++)
            {
                Food current = grid[x, y];
                Food previous = grid[x, y - 1];

                if (current != null && previous != null && current.Type == previous.Type)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = 0; i < matchCount; i++)
                        {
                            matches.Add(grid[x, y - 1 - i]);
                        }
                    }

                    matchCount = 1;
                }
            }

            if (matchCount >= 3)
            {
                for (int i = 0; i < matchCount; i++)
                {
                    matches.Add(grid[x, height - 1 - i]);
                }
            }
        }
    }
}