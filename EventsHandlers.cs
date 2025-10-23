using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace xp_plugin
{
    public class EventHandlers
    {
        private readonly xp_plugin plugin;
        private readonly RoleTypeId[] ScpRoles = { RoleTypeId.Scp049, RoleTypeId.Scp0492, RoleTypeId.Scp173, RoleTypeId.Scp096, RoleTypeId.Scp106, RoleTypeId.Scp079, RoleTypeId.Scp939, RoleTypeId.Scp3114, RoleTypeId.ChaosConscript, RoleTypeId.ChaosMarauder, RoleTypeId.AlphaFlamingo, RoleTypeId.ChaosRepressor, RoleTypeId.ChaosRifleman, RoleTypeId.FacilityGuard, RoleTypeId.Flamingo, RoleTypeId.NtfCaptain, RoleTypeId.NtfPrivate, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.ZombieFlamingo };

        public EventHandlers(xp_plugin plugin) => this.plugin = plugin;

        private Player lastWarheadActivator = null;
        private readonly Dictionary<Player, DateTime> lastScp1344Use = new Dictionary<Player, DateTime>();

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            Log.Info($"Player {ev.Player.Nickname} ({ev.Player.UserId}) has connected.");
        }

        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null || ev.Attacker == ev.Player)
                return;

            if (ev.Player.Role.Team == Team.SCPs)
            {
                plugin.AddXp(ev.Attacker, plugin.Config.on_scp_kill_xp, plugin.Config.on_scp_kill_message, ev.Player);
            }
            else
            {
                plugin.AddXp(ev.Attacker, plugin.Config.on_kill_xp, plugin.Config.on_kill_message, ev.Player);
            }
        }

        public void OnEscaping(EscapingEventArgs ev)
        {
            if (!ScpRoles.Contains(ev.Player.Role.Type))
            {
                plugin.AddXp(ev.Player, plugin.Config.on_escape_xp, plugin.Config.on_escape_message);
            }
            else
            {
                return;
            }

            if ((ev.Player.Role.Type == RoleTypeId.ClassD || ev.Player.Role.Type == RoleTypeId.Scientist) && ev.Player.IsCuffed)
            {
                Player cuffer = Player.List.FirstOrDefault(p => p.Id == ev.Player.Cuffer.Id);
                if (cuffer != null && (cuffer.Role.Team == Team.FoundationForces))
                {
                    plugin.AddXp(cuffer, plugin.Config.on_player_escort_xp, plugin.Config.on_player_escort_message, ev.Player);
                }
            }
        }

        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (ev.Target.IsCuffed) return;
            if (ev.Player.Role.Team == Team.FoundationForces && (ev.Target.Role.Type == RoleTypeId.ClassD || ev.Target.Role.Type == RoleTypeId.Scientist))
            {
            }
        }

        public async void OnUsedItem(UsedItemEventArgs ev)
        {
            switch (ev.Item.Type)
            {
                case ItemType.SCP018:
                case ItemType.SCP207:
                case ItemType.AntiSCP207:
                case ItemType.SCP244a:
                case ItemType.SCP244b:
                case ItemType.SCP1576:
                case ItemType.SCP1344:
                    if (CanUseScp1344(ev.Player))
                    {
                        if (ev.Item.Type == ItemType.SCP1344)
                        {
                            await System.Threading.Tasks.Task.Delay(7500);
                        }
                        plugin.AddXp(ev.Player, plugin.Config.on_scp_item_use_xp, plugin.Config.on_scp_item_use_message);
                        lastScp1344Use[ev.Player] = DateTime.UtcNow;
                    }
                    break;
                case ItemType.SCP1853:
                case ItemType.SCP268:
                case ItemType.SCP500:
                case ItemType.SCP2176:
                    plugin.AddXp(ev.Player, plugin.Config.on_scp_item_use_xp, plugin.Config.on_scp_item_use_message);
                    break;
            }
        }

        private bool CanUseScp1344(Player player)
        {
            if (lastScp1344Use.TryGetValue(player, out DateTime lastUse))
            {
                return (DateTime.UtcNow - lastUse).TotalSeconds >= 5;
            }
            return true;
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                plugin.AddXp(ev.Player, plugin.Config.on_warhead_panel_unlock_xp, plugin.Config.on_warhead_panel_unlock_message);
            }
        }

        public void OnWarheadStarting(StartingEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                lastWarheadActivator = ev.Player;
            }
        }

        public void OnWarheadDetonated()
        {
            if (lastWarheadActivator != null)
            {
                plugin.AddXp(lastWarheadActivator, plugin.Config.on_warhead_detonation_xp, plugin.Config.on_warhead_detonation_message);
                lastWarheadActivator = null;
            }
        }
    }
}