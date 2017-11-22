﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MineCase.Engine;
using MineCase.Server.Components;
using MineCase.Server.Game.Entities.Components;
using MineCase.Server.User;
using Orleans.Concurrency;

namespace MineCase.Server.Network.Play
{
    internal class ClientboundPacketComponent : Component, IHandle<BindToUser>, IHandle<KickPlayer>, IHandle<PacketForwardToPlayer>, IHandle<SpawnEntity>
    {
        private IClientboundPacketSink _sink;
        private IUser _user;

        public ClientboundPacketComponent(string name = "clientboundPacket")
            : base(name)
        {
        }

        public ClientPlayPacketGenerator GetGenerator()
            => new ClientPlayPacketGenerator(_sink);

        public async Task Kick()
        {
            await AttachedObject.Tell(Disable.Default);
            await _user.Kick();
        }

        async Task IHandle<BindToUser>.Handle(BindToUser message)
        {
            _user = message.User;
            _sink = await message.User.GetClientPacketSink();
        }

        Task IHandle<KickPlayer>.Handle(KickPlayer message)
        {
            return Kick();
        }

        Task IHandle<PacketForwardToPlayer>.Handle(PacketForwardToPlayer message)
        {
            return _sink.SendPacket(message.PacketId, message.Data.AsImmutable());
        }

        Task IHandle<SpawnEntity>.Handle(SpawnEntity message)
        {
            // return AttachedObject.Tell(Enable.Default);
            return Task.CompletedTask;
        }
    }
}
