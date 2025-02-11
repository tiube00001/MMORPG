﻿using MMORPG.Common.Tool;
using GameServer.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.BuffSystem;

namespace GameServer.Manager
{
    public class DataManager : Singleton<DataManager>
    {
        private string dirPath = "";
        
        public Dictionary<int, MapDefine> MapDict;
        public Dictionary<int, UnitDefine> UnitDict;
        public Dictionary<int, SpawnDefine> SpawnDict;
        public Dictionary<int, ItemDefine> ItemDict;
        public Dictionary<int, SkillDefine> SkillDict;
        public Dictionary<int, NpcDefine> NpcDict;
        public Dictionary<int, DialogueDefine> DialogueDict;
        public Dictionary<int, TaskDefine> TaskDict;
        public Dictionary<int, RewardDefine> RewardDict;
        public Dictionary<int, BuffDefine> BuffDict;

        private DataManager() { }

        public void SetDirPath(string dp)
        {
            dirPath = dp;
        }

        public void Start()
        {
            MapDict = Load<Dictionary<int, MapDefine>>("Data/Json/MapDefine.json");
            UnitDict = Load<Dictionary<int, UnitDefine>>("Data/Json/UnitDefine.json");
            SpawnDict = Load<Dictionary<int, SpawnDefine>>("Data/Json/SpawnDefine.json");
            ItemDict = Load<Dictionary<int, ItemDefine>>("Data/Json/ItemDefine.json");
            SkillDict = Load<Dictionary<int, SkillDefine>>("Data/Json/SkillDefine.json");
            NpcDict = Load<Dictionary<int, NpcDefine>>("Data/Json/NpcDefine.json");
            DialogueDict = Load<Dictionary<int, DialogueDefine>>("Data/Json/DialogueDefine.json");
            TaskDict = Load<Dictionary<int, TaskDefine>>("Data/Json/TaskDefine.json");
            RewardDict = Load<Dictionary<int, RewardDefine>>("Data/Json/RewardDefine.json");
            BuffDict = Load<Dictionary<int, BuffDefine>>("Data/Json/BuffDefine.json");
        }

        public void Update() { }

        private T Load<T>(string jsonPath)
        {
            jsonPath = Path.Join(dirPath, jsonPath);
            var content = ResourceHelper.LoadFile(jsonPath);
            Debug.Assert(content != null);
            var obj = JsonConvert.DeserializeObject<T>(content);
            Debug.Assert(obj != null);
            return obj;
        }
    }
}
