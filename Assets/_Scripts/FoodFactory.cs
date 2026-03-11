using System;
using System.Collections.Generic;
using UnityEngine;

public class FoodFactory : MonoBehaviour
{
    [Serializable]
    public class FoodPrefabItem
    {
        public FoodType type;
        public Food prefab;
    }

    [SerializeField] private List<FoodPrefabItem> FoodPrefabs = new();

    private Dictionary<FoodType, Food> prefabMap;

    private void Awake()
    {
        prefabMap = new Dictionary<FoodType, Food>();

        foreach (var item in FoodPrefabs)
        {
            if (item.prefab == null) continue;

            if (!prefabMap.ContainsKey(item.type))
            {
                prefabMap.Add(item.type, item.prefab);
            }
        }
    }

    public Food Create(FoodType type, Vector3 position, Transform parent)
    {
        if (!prefabMap.TryGetValue(type, out Food prefab))
        {
            Debug.LogError($"Missing prefab for Food type: {type}");
            return null;
        }

        return Instantiate(prefab, position, Quaternion.identity, parent);
    }

    public FoodType GetRandomType()
    {
        Array values = Enum.GetValues(typeof(FoodType));
        return (FoodType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}