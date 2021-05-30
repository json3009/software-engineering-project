using System;
using System.Collections.Generic;
using System.Linq;
using OdinSerializer;

namespace CombatSystem.SpellSystem
{
    [Serializable]
    public class SpellArchetypeLevels
    {
        [OdinSerialize] private List<SpellArchetypeLevelData> levels = new List<SpellArchetypeLevelData>();
        private Dictionary<int, SpellArchetypeLevelData> cachedLevels = new Dictionary<int, SpellArchetypeLevelData>();

        public List<SpellArchetypeLevelData> Levels { get => levels; }

        public List<SpellArchetypeLevelData> CachedLevels { get { return cachedLevels.Values.ToList(); } }

        public SpellArchetypeLevels() { }

        public void RecalculateCache()
        {
            if (cachedLevels == null)
                cachedLevels = new Dictionary<int, SpellArchetypeLevelData>();
            else
                cachedLevels.Clear();

            SpellArchetypeLevelData data = null;

            for (int i = 0; i < levels.Count - 1; i++)
            {
                SpellArchetypeLevelData currentLevel = levels[i];
                SpellArchetypeLevelData nextLevel = levels[i + 1];

                if (data == null)
                {
                    cachedLevels.Add(currentLevel.Level, (SpellArchetypeLevelData)SerializationUtility.CreateCopy(currentLevel));
                    data = currentLevel.Combine(nextLevel);
                }
                else
                {
                    data = data.Combine(nextLevel);
                }

                try
                {
                    cachedLevels.Add(nextLevel.Level, data);
                }
                catch (ArgumentException) { }
            }

        }

        public SpellArchetypeLevelData GetLevelData(int level)
        {
            if (cachedLevels == null)
                RecalculateCache();

            if (cachedLevels.TryGetValue(level, out SpellArchetypeLevelData data))
                return data;

            int chosenLevel = level;
            foreach (var item in levels)
            {
                if (item.Level > level)
                    break;

                chosenLevel = item.Level;
            }

            if (!cachedLevels.ContainsKey(chosenLevel))
                RecalculateCache();

            if (cachedLevels.ContainsKey(chosenLevel))
                return cachedLevels[chosenLevel];
            else
                return new SpellArchetypeLevelData();
        }

    }
}