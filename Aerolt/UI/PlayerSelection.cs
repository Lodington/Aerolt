using System;
using System.Collections.Generic;
using Aerolt.Managers;
using RoR2;

namespace Aerolt.UI
{
    public static class PlayerSelection
    {
        private static readonly Dictionary<NetworkUser, PlayerConfigBinding> Bindings = new();

        public static NetworkUser? Selected { get; private set; }

        public static PlayerConfigBinding? SelectedBinding =>
            Selected != null ? GetOrCreateBinding(Selected) : null;

        public static event Action? SelectionChanged;

        public static void Select(NetworkUser? user)
        {
            if (Selected == user) return;
            Selected = user;
            SelectionChanged?.Invoke();
        }

        public static PlayerConfigBinding GetOrCreateBinding(NetworkUser user)
        {
            if (!Bindings.TryGetValue(user, out var binding) || binding == null)
            {
                binding = new PlayerConfigBinding(user);
                Bindings[user] = binding;
            }

            return binding;
        }

        public static IReadOnlyList<NetworkUser> GetPlayers()
        {
            var players = NetworkUser.readOnlyInstancesList;

            if ((Selected == null || !Selected) && players.Count > 0)
            {
                NetworkUser? localUser = null;
                foreach (var user in players)
                    if (user && user.localUser != null)
                    {
                        localUser = user;
                        break;
                    }

                Select(localUser ?? players[0]);
            }

            return players;
        }
    }
}
