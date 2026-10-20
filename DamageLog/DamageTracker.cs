using System;
using System.Collections.Generic;
using RoR2;

namespace DamageLog
{
    public static class DamageTracker
    {
        public struct Entry
        {
            public float Time;
            public string Attacker;
            public string Victim;
            public float Amount;
            public bool Crit;
            public float ProcCoefficient;
            public string DamageType;
            public bool AttackerIsSelected;
            public bool VictimIsSelected;
        }

        private const int MaxEntries = 200;

        private static readonly LinkedList<Entry> Entries = new();
        private static bool _hooked;
        private static bool _paused;

        public static bool Paused
        {
            get => _paused;
            set => _paused = value;
        }

        public static void Init()
        {
            if (_hooked) return;
            _hooked = true;
            GlobalEventManager.onServerDamageDealt += OnDamage;
        }

        public static void Clear()
        {
            Entries.Clear();
        }

        public static Entry[] Snapshot()
        {
            var arr = new Entry[Entries.Count];
            Entries.CopyTo(arr, 0);
            return arr;
        }

        private static void OnDamage(DamageReport report)
        {
            if (_paused || report == null) return;

            var selected = Aerolt.API.AeroltAPI.SelectedPlayer;
            var selectedMaster = selected && selected!.master ? selected.master : null;

            var entry = new Entry
            {
                Time = UnityEngine.Time.time,
                Attacker = NameOf(report.attackerBody, report.attackerMaster),
                Victim = NameOf(report.victimBody, report.victimMaster),
                Amount = report.damageDealt,
                Crit = report.damageInfo != null && report.damageInfo.crit,
                ProcCoefficient = report.damageInfo?.procCoefficient ?? 0f,
                DamageType = report.damageInfo != null ? report.damageInfo.damageType.damageType.ToString() : "",
                AttackerIsSelected = selectedMaster && report.attackerMaster == selectedMaster,
                VictimIsSelected = selectedMaster && report.victimMaster == selectedMaster
            };

            Entries.AddFirst(entry);
            while (Entries.Count > MaxEntries)
                Entries.RemoveLast();
        }

        private static string NameOf(CharacterBody? body, CharacterMaster? master)
        {
            if (body)
                try
                {
                    return body!.GetDisplayName();
                }
                catch
                {
                }

            if (master && master!.name != null)
                return master.name;
            return "?";
        }
    }
}
