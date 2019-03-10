#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;

#endregion

namespace Oblivion.HabboHotel.Users.Effects
{
    public sealed class EffectsComponent
    {
        /// <summary>
        ///     Effects stored by ID > Effect.
        /// </summary>
        private readonly ConcurrentDictionary<int, AvatarEffect> _effects =
            new ConcurrentDictionary<int, AvatarEffect>();

        private Habbo _habbo;

        public ICollection<AvatarEffect> GetAllEffects => _effects.Values;

        public int CurrentEffect { get; set; }

        /// <summary>
        ///     Initializes the EffectsComponent.
        /// </summary>
        /// <param name="UserId"></param>
        public bool Init(Habbo Habbo)
        {
            if (_effects.Count > 0)
                return false;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_effects` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", Habbo.Id);
                var GetEffects = dbClient.GetTable();

                if (GetEffects != null)
                    foreach (DataRow Row in GetEffects.Rows)
                        _effects.TryAdd(Convert.ToInt32(Row["id"]),
                            new AvatarEffect(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["user_id"]),
                                Convert.ToInt32(Row["effect_id"]), Convert.ToDouble(Row["total_duration"]),
                                OblivionServer.EnumToBool(Row["is_activated"].ToString()),
                                Convert.ToDouble(Row["activated_stamp"]), Convert.ToInt32(Row["quantity"])));
            }

            _habbo = Habbo;
            CurrentEffect = 0;
            return true;
        }

        public bool TryAdd(AvatarEffect Effect) => _effects.TryAdd(Effect.Id, Effect);

        /// <summary>
        /// </summary>
        /// <param name="SpriteId"></param>
        /// <param name="ActivatedOnly"></param>
        /// <param name="UnactivatedOnly"></param>
        /// <returns></returns>
        public bool HasEffect(int SpriteId, bool ActivatedOnly = false, bool UnactivatedOnly = false)
            => GetEffectNullable(SpriteId, ActivatedOnly, UnactivatedOnly) != null;

        /// <summary>
        /// </summary>
        /// <param name="SpriteId"></param>
        /// <param name="ActivatedOnly"></param>
        /// <param name="UnactivatedOnly"></param>
        /// <returns></returns>
        public AvatarEffect GetEffectNullable(int SpriteId, bool ActivatedOnly = false, bool UnactivatedOnly = false)
            =>
                _effects.Values.ToList()
                    .FirstOrDefault(
                        Effect =>
                            !Effect.HasExpired && Effect.SpriteId == SpriteId && (!ActivatedOnly || Effect.Activated) &&
                            (!UnactivatedOnly || !Effect.Activated));

        /// <summary>
        /// </summary>
        /// <param name="Habbo"></param>
        public void CheckEffectExpiry(Habbo Habbo)
        {
            foreach (var Effect in _effects.Values.ToList().Where(Effect => Effect.HasExpired))
                Effect.HandleExpiration(Habbo);
        }

        public void ApplyEffect(int EffectId)
        {
            var User = _habbo?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(_habbo.Id);
            if (User == null)
                return;

            CurrentEffect = EffectId;

            if (User.IsDancing)
                _habbo.CurrentRoom.SendMessage(new DanceComposer(User, 0));
            _habbo.CurrentRoom.SendMessage(new AvatarEffectComposer(User.VirtualId, EffectId));
        }

        /// <summary>
        ///     Disposes the EffectsComponent.
        /// </summary>
        public void Dispose() => _effects.Clear();
    }
}