using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchGroup
{
    public List<Food> Foods = new List<Food>();
    public bool IsHorizontal;
    public bool IsVertical;

    public int Length => Foods.Count;
}

public class MatchDetector : MonoBehaviour
{
    public List<Food> FindAllMatches(Food[,] grid, int width, int height)
    {
        List<MatchGroup> groups = FindMatchGroups(grid, width, height);
        HashSet<Food> uniqueMatches = new HashSet<Food>();

        foreach (MatchGroup group in groups)
        {
            foreach (Food food in group.Foods)
            {
                if (food != null)
                    uniqueMatches.Add(food);
            }
        }

        return new List<Food>(uniqueMatches);
    }

    public List<MatchGroup> FindMatchGroups(Food[,] grid, int width, int height)
    {
        List<MatchGroup> groups = new List<MatchGroup>();

        // Horizontal
        for (int y = 0; y < height; y++)
        {
            int startX = 0;

            while (startX < width)
            {
                Food startFood = grid[startX, y];
                if (startFood == null)
                {
                    startX++;
                    continue;
                }

                FoodType type = startFood.Type;
                int length = 1;
                int x = startX + 1;

                while (x < width && grid[x, y] != null && grid[x, y].Type == type)
                {
                    length++;
                    x++;
                }

                if (length >= 3)
                {
                    MatchGroup group = new MatchGroup
                    {
                        IsHorizontal = true,
                        IsVertical = false
                    };

                    for (int i = 0; i < length; i++)
                    {
                        group.Foods.Add(grid[startX + i, y]);
                    }

                    groups.Add(group);
                }

                startX = x;
            }
        }

        // Vertical
        for (int x = 0; x < width; x++)
        {
            int startY = 0;

            while (startY < height)
            {
                Food startFood = grid[x, startY];
                if (startFood == null)
                {
                    startY++;
                    continue;
                }

                FoodType type = startFood.Type;
                int length = 1;
                int y = startY + 1;

                while (y < height && grid[x, y] != null && grid[x, y].Type == type)
                {
                    length++;
                    y++;
                }

                if (length >= 3)
                {
                    MatchGroup group = new MatchGroup
                    {
                        IsHorizontal = false,
                        IsVertical = true
                    };

                    for (int i = 0; i < length; i++)
                    {
                        group.Foods.Add(grid[x, startY + i]);
                    }

                    groups.Add(group);
                }

                startY = y;
            }
        }

        return groups;
    }

    public List<Food> FindMatchesAt(Food[,] grid, int width, int height, int x, int y)
    {
        HashSet<Food> matches = new HashSet<Food>();

        if (x < 0 || x >= width || y < 0 || y >= height) return new List<Food>();

        Food center = grid[x, y];
        if (center == null) return new List<Food>();

        FoodType type = center.Type;

        // Horizontal around center
        List<Food> horizontal = new List<Food> { center };

        int left = x - 1;
        while (left >= 0 && grid[left, y] != null && grid[left, y].Type == type)
        {
            horizontal.Add(grid[left, y]);
            left--;
        }

        int right = x + 1;
        while (right < width && grid[right, y] != null && grid[right, y].Type == type)
        {
            horizontal.Add(grid[right, y]);
            right++;
        }

        if (horizontal.Count >= 3)
        {
            foreach (Food food in horizontal)
                matches.Add(food);
        }

        // Vertical around center
        List<Food> vertical = new List<Food> { center };

        int down = y - 1;
        while (down >= 0 && grid[x, down] != null && grid[x, down].Type == type)
        {
            vertical.Add(grid[x, down]);
            down--;
        }

        int up = y + 1;
        while (up < height && grid[x, up] != null && grid[x, up].Type == type)
        {
            vertical.Add(grid[x, up]);
            up++;
        }

        if (vertical.Count >= 3)
        {
            foreach (Food food in vertical)
                matches.Add(food);
        }

        return new List<Food>(matches);
    }
}