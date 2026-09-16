namespace BossMod.Endwalker.Unreal.Un4Zurvan;

sealed class P2ExecratedWill(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWill); // hard-hitting add
sealed class P2ExecratedWit(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWit); // high-priority add (casts comets and meteor)
sealed class P2ExecratedWile(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWile); // low-priority add (casts fear, then magical autos)
sealed class P2ExecratedThew(BossModule module) : Components.Adds(module, (uint)OID.ExecratedThew); // small add

sealed class P2Comet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Comet, 4f);
sealed class P2MeracydianFear(BossModule module) : Components.CastGaze(module, (uint)AID.MeracydianFear);
