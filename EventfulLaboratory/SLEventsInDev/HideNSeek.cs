using System.Collections.Generic;
using System.Linq;
using EventfulLaboratory.structs;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Server = Exiled.Events.Handlers.Server;

namespace EventfulLaboratory.SLEvents
{
    public class HideNSeek : AEvent
    {
        private bool _roundStarted;
        private Random _rng;

        private Vector3 _seekerSpawnPoint = Vector3.zero;

        public override void OnRoundStart()
        {

            Exiled.Events.Handlers.Player.Joined += OnPlayerJoinProxy;
            Server.RoundStarted += ServerOnRoundStarted;
        }

        private void ServerOnRoundStarted()
        {

            List<Player> players = Player.List.ToList();

            Player seeker = players[_rng.NextInt(players.Count)];

            foreach (var player in players)
            {
                if (player.UserId == seeker.UserId)
                {
                    seeker.SetRole(RoleType.Tutorial);

                }
                else
                {
                    Timing.RunCoroutine(SpawnHider(player));
                }
            }
        }

        #region Proxies :(

        private void OnPlayerJoinProxy(JoinedEventArgs ev) => Timing.RunCoroutine(OnPlayerJoined(ev));

        #endregion

        #region Exiled Events

        private IEnumerator<float> OnPlayerJoined(JoinedEventArgs ev)
        {
            yield return Timing.WaitForSeconds(0.1f);

            //Allow spawning 5 sec after roundstart
            if (_roundStarted)
            {
                ev.Player.SetRole(RoleType.Spectator);
                ev.Player.Broadcast(5, "Sorry, you have joined too late and was moved to Spectator.");
            }
            else
            {
                yield return Timing.WaitUntilDone(SpawnSeeker(ev.Player));
            }
        }

        #endregion

        #region Helpers

        private IEnumerator<float> SpawnHider(Player ply)
        {
            yield return Timing.WaitForSeconds(0.1f);

            ply.Scale = new Vector3(0.3f, 0.3f, 0.3f);
            yield return Timing.WaitForSeconds(0.1f);

            ply.SetRole(RoleType.ClassD);
            ply.IsInvisible = true;
            ply.IsBypassModeEnabled = true;

            yield return Timing.WaitForSeconds(0.3f);

            ply.MaxHealth = 1;
            ply.Health = 1;
            ply.ShowHint("HIDE! People will search for you!");

            if (_seekerSpawnPoint == Vector3.zero) _seekerSpawnPoint = ply.Position;
        }

        private IEnumerator<float> SpawnSeeker(Player ply)
        {
            yield return Timing.WaitForSeconds(0.1f);

            ply.SetRole(RoleType.FacilityGuard);
            ply.ClearInventory();
            Firearm newGun = (Firearm) Item.Create(ItemType.GunCOM15, ply);
            newGun.Ammo = 255;
            // TODO: ????
            // newGun.AddAttachment(
            //     new FirearmAttachment
            //     {
            //         Name = AttachmentNameTranslation.AmmoCounter,
            //         Settings = new AttachmentSettings
            //         {
            //             Weight = 2000f,
            //             AdditionalCons = AttachmentDescriptiveDownsides.Laser,
            //             AdditionalPros = AttachmentDescriptiveAdvantages.AmmoCounter,
            //             PhysicalLength = 30
            //         },
            //         Slot = AttachmentSlot.Sight
            //     });
            ply.AddItem(newGun);
            ply.AddItem(Item.Create(ItemType.Radio, ply));
            ply.IsBypassModeEnabled = true;
            ply.ShowHint("Search for them. First 4 kills will make them Seekers too!");
        }

        #endregion
    }
}
