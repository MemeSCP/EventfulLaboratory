using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using EventfulLaboratory.structs;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;
using MapEvents = Exiled.Events.Handlers.Map;
using PlayerEvent = Exiled.Events.Handlers.Player;
using ServerEvent = Exiled.Events.Handlers.Server;

namespace EventfulLaboratory.slevents
{
    internal sealed class AmazingRace : AEvent
    {
        
        #region vars
        private static readonly int _secondTilRoundStart = 10;
        private bool _roundStarted = false;
        private int _roundStartCountdown = _secondTilRoundStart;

        private static readonly int _roundMinutes = 10;
        private int _roundRemainingSeconds = _roundMinutes * 60;

        private List<Player> _escapees = new List<Player>();
        
        #endregion
        
        #region Overrides
        public override void OnNewRound()
        {
            Common.LockRound();
            PlayerEvent.Joined += PlayerJoinedProxy;
            ServerEvent.RespawningTeam += Common.PreventRespawnEvent;
            PlayerEvent.DroppingItem += PreventDropping;
            Timing.RunCoroutine(StartRound());
        }

        public override void Disable()
        {
            OnRoundEnd();
        }

        public override void OnRoundEnd()
        {
            PlayerEvent.Joined -= PlayerJoinedProxy;
            PlayerEvent.DroppingItem -= PreventDropping;
            ServerEvent.RespawningTeam -= Common.PreventRespawnEvent;
            if (!_roundStarted) return;
            
            PlayerEvent.PickingUpItem -= PlayerPickup;
            MapEvents.SpawningItem -= MapItemSpawn;
            PlayerEvent.Hurting -= PlayerHurt;
            PlayerEvent.Escaping -= EscapingEventProxy;
        }
        
        #endregion

        #region Proxies

        void PlayerJoinedProxy(JoinedEventArgs ev) => Timing.RunCoroutine(PlayerJoined(ev));

        void EscapingEventProxy(EscapingEventArgs ev) => Timing.RunCoroutine(OnPlayerEscape(ev));
        
        #endregion
        
        #region Events

        IEnumerator<float> StartRound()
        {
            while (true)
            {
                _roundStartCountdown--;
                if (_roundStartCountdown < 1)
                {
                    Common.Broadcast(2, "Round is starting!", true);
                    Cassie.Message("Go");
                    
                    yield return Timing.WaitForSeconds(1f);
                    foreach (var player in Player.List)
                    {
                        player.SetRole(RoleType.ClassD);
                        player.Inventory.AddNewItem(ItemType.KeycardJanitor);
                        player.ShowHint(
                            "Find all the Keycards. You need to through every step.\n<color=red>You need an O5 to escape.</color>\nClock is ticking. Good luck."
                        );
                    }
                    
                    //TODO: Spawn Keycards

                    Timing.RunCoroutine(CountDown());

                    PlayerEvent.PickingUpItem += PlayerPickup;
                    MapEvents.SpawningItem += MapItemSpawn;
                    PlayerEvent.Hurting += PlayerHurt;
                    PlayerEvent.Escaping += EscapingEventProxy;
                    break;
                }

                Common.Broadcast(1, $"Round starts in {_roundStartCountdown} seconds.", true);
                if (_roundStartCountdown <= 3) 
                    Cassie.Message(_roundStartCountdown.ToString(), false, false);
                
                yield return Timing.WaitForSeconds(1);
            }
            
        }

        IEnumerator<float> PlayerJoined(JoinedEventArgs ev)
        {
            yield return Timing.WaitForSeconds(0.1f);
            if (!_roundStarted)
            {
                if (_roundStartCountdown > 3)
                    _roundStartCountdown++;
                ev.Player.Broadcast(3, "The race starts in a few seconds.");
                yield return Timing.WaitForSeconds(0.1f);
                ev.Player.SetRole(RoleType.Tutorial);
                yield return Timing.WaitForSeconds(0.1f);
                ev.Player.Position = Common.GetEvacuationZone().Position + new Vector3(0, 1f, 0);
                ev.Player.ShowHint(
                    $"<size=50>Welcome to the Amazing Race!</size>\nYour goal is to escape the facility.\nNo 914, Only ClassD\nFind the keycards\n<size=50><color=red>ONLY WHO HAS O5 CAN ESCAPE!</color></size>\n\nGood Luck. You have {_roundMinutes} minutes.",
                    10F);
            }
            else
            {
                if (ev.Player.Role != RoleType.Spectator) ev.Player.SetRole(RoleType.Spectator);
                ev.Player.ShowHint("Sorry, you joined too late for the race.");
            }
        }

        void PlayerPickup(PickingUpItemEventArgs ev)
        {
            ev.IsAllowed = false;
            if (ev.Pickup.itemId.IsKeycard())
            {
                Inventory.SyncItemInfo curCard = CurrentKeycard(ev.Player);
                if (ev.Pickup.ItemId <= curCard.id)
                {
                    ev.Player.ShowHint("Your keycard is already a higher grade!");
                    return;
                }

                if (ev.Pickup.ItemId - 1 > curCard.id)
                {
                    ev.Player.ShowHint("You need to find a lower tier card first!");
                    return;
                }

                curCard.id++;
                ev.Player.ShowHint($"Upgraded! Next one is: {curCard.id+1.ToString()}");
            }
            else if (ev.Pickup.ItemId == ItemType.SCP207)
            {
                if (ev.Player.GetEffectActive<Scp207>())
                {
                    ev.Player.ShowHint("Speedboost already enabled!");
                    return;
                }
                
                ev.Player.ShowHint("Speedboost enabled!");
                ev.Player.EnableEffect<Scp207>();
                ev.Player.ChangeEffectIntensity<Scp207>(1);
            }
            else
            {
                ev.Player.ShowHint("Sorry, picking up that item is not allowed.");
            }
        }

        IEnumerator<float> OnPlayerEscape(EscapingEventArgs ev)
        {
            ev.IsAllowed = false;
            yield return Timing.WaitForSeconds(0.1f);
            ev.Player.SetRole(RoleType.Tutorial);
            _escapees.Add(ev.Player);
            ev.Player.ShowHint($"You have successfully escaped from the facility! Your position: {_escapees.Count}");
        }

        IEnumerator<float> CountDown()
        {
            while (_roundRemainingSeconds > 0)
            {
                if (_roundRemainingSeconds > 10)
                {
                    if (_roundRemainingSeconds % 60 == 0)
                    {
                        Common.Broadcast(3, $"You have {_roundRemainingSeconds / 60} minutes left.");
                    }
                }
                else
                {
                    Common.Broadcast(1, _roundRemainingSeconds.ToString());
                    
                }

                _roundRemainingSeconds--;
                yield return Timing.WaitForSeconds(1f);
            }
            Common.Broadcast(3, "Times up!");
            foreach (var player in Player.List)
            {
                if (player.Role != RoleType.ClassD) continue;
                
                player.SetRole(RoleType.Spectator);
                player.ShowHint("You ran out of time!");
            }
            //TODO: Announce winners, round end
        }

        void MapItemSpawn(SpawningItemEventArgs ev)
        {
            if (ev.Id != ItemType.SCP207)
                ev.IsAllowed = false;
        }

        void PlayerHurt(HurtingEventArgs ev)
        {
            if (ev.DamageType.isScp || ev.DamageType.isWeapon || ev.DamageType == DamageTypes.Wall)
                ev.IsAllowed = false;
        }

        void PreventDropping(DroppingItemEventArgs ev) => ev.IsAllowed = false;

        #endregion

        Inventory.SyncItemInfo CurrentKeycard(Player ply) => ply.Inventory.items.First(kc => kc.id.IsKeycard());
    }
}