using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    public class InstanceService
    {
        private readonly string _databasePath;

        public InstanceService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "instances.db");
        }

        public InstanceDungeon CreateInstance(string owner, string name, int floors, int maxPlayers)
        {
            var inst = new InstanceDungeon
            {
                OwnerUsername = owner,
                Name = name,
                FloorCount = floors,
                MaxPlayers = maxPlayers,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                Status = InstanceStatus.Pending
            };

            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<InstanceDungeon>("instances");
            col.Insert(inst);
            return inst;
        }

        public InstanceDungeon? GetById(int id)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<InstanceDungeon>("instances");
            return col.FindById(id);
        }

        public List<InstanceDungeon> GetActiveInstances()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<InstanceDungeon>("instances");
            return col.Find(i => i.Status == InstanceStatus.Active || i.Status == InstanceStatus.Pending).ToList();
        }

        public void AddParticipant(int instanceId, string username)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<InstanceDungeon>("instances");
            var inst = col.FindById(instanceId);
            if (inst == null) return;
            if (!inst.Participants.Contains(username) && inst.Participants.Count < inst.MaxPlayers)
            {
                inst.Participants.Add(username);
                col.Update(inst);
            }
        }

        public void StartInstance(int instanceId)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<InstanceDungeon>("instances");
            var inst = col.FindById(instanceId);
            if (inst == null) return;
            inst.Status = InstanceStatus.Active;
            col.Update(inst);
        }

        public void CompleteInstance(int instanceId)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<InstanceDungeon>("instances");
            var inst = col.FindById(instanceId);
            if (inst == null) return;
            inst.Status = InstanceStatus.Completed;
            col.Update(inst);
        }
    }
}
