using System;
using System.Collections.Generic;
using Exiled.API.Features;
using HarmonyLib;
using MEC;

namespace EventfulLaboratory.Handler
{
    public class EvenTeamSplitHandler
    {
        private readonly RoleType _team1;
        private readonly RoleType _team2;
        private readonly bool _autobalance;
        private readonly bool _clearInventory;

        private bool _dirty = false;

        private int _team1Count = 0;
        private int _team2Count = 0;

        private Dictionary<string, RoleType> _userToRoles;
        
        public EvenTeamSplitHandler(RoleType team1, RoleType team2, bool autobalance = false, bool clearInventory = true)
        {
            _team1 = team1;
            _team2 = team2;
            _autobalance = autobalance;
            _clearInventory = clearInventory;
            _userToRoles = new Dictionary<string, RoleType>();
        }

        public IEnumerator<float> SpawnPlayer(Player player)
        {
            player.SetRole(GetSetRole(player));
            
            if (!_clearInventory) yield break;
            
            yield return Timing.WaitForSeconds(0.3F);
            player.Inventory.Clear();
        }

        public RoleType GetSetRole(Player player)
        {
            if (_userToRoles.ContainsKey(player.UserId))
                return _userToRoles[player.UserId];

            if (_dirty) ReCount();
            
            //New Player, add role
            var nextRole = DetermineNextRoleFromCount();
            _userToRoles[player.UserId] = nextRole;
            IncrementCounter(nextRole);
            return nextRole;
        }

        public void SetDirty() => _dirty = true;

        public void Rebalance()
        {
            //TODO
        }
        
        #region Events
        
        #endregion
        
        #region RoleCounter

        private bool ShouldRebalance() => _autobalance && Math.Abs(_team1Count - _team2Count) > 1;

        private void ReCount()
        {
            _team1Count = 0;
            _team2Count = 0;
            
            _userToRoles.Values.Do(IncrementCounter);
            _dirty = false;
        }

        private RoleType DetermineNextRoleFromCount() => _team2Count > _team1Count ? _team1 : _team2;

        private void IncrementCounter(RoleType type) => IncrementCounter(type, 1);
        private void IncrementCounter(RoleType type, int amount)
        {
            if (_team1 == type) _team1Count += amount;
            else if (_team2 == type) _team2Count += amount;
        }
        
        #endregion
    }
}