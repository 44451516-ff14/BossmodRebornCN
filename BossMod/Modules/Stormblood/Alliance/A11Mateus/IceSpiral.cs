namespace BossMod.Stormblood.Alliance.A11Mateus;

class IceSpiral(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(2f);
    public readonly List<AOEInstance> _aoes = new(228);
    private static readonly WPos[] coordinates =
    [   // Note: the spiral coordinates are slightly different (different from normal quantization  errors) in each replay I looked at. I assume they somehow get 
        // generated on the fly based on the actor movement along the spiral, which would have variations depending on the server tick
        // there seem to be exactly 228 hits
        new(-319.708f, 241.343f), new(-319.485f, 242.37f), new(-319.568f, 243.553f), new(-320.035f, 244.154f), new(-321.17f, 244.346f),
        new(-322.059f, 244.113f), new(-324.148f, 241.59f), new(-324.243f, 240.692f), new(-324.133f, 239.43f), new(-323.672f, 238.441f),
        new(-322.862f, 237.422f), new(-322.16f, 236.834f), new(-318.512f, 235.841f), new(-317.504f, 236.002f), new(-316.337f, 236.424f),
        new(-315.658f, 236.833f), new(-314.72f, 237.53f), new(-314.01f, 238.2f), new(-312.356f, 241.46f), new(-312.233f, 242.485f),
        new(-312.296f, 243.885f), new(-312.441f, 244.843f), new(-312.868f, 246.175f), new(-313.255f, 246.955f), new(-315.427f, 249.437f),
        new(-316.315f, 250.059f), new(-317.315f, 250.661f), new(-318.223f, 250.97f), new(-319.339f, 251.365f), new(-322.844f, 251.297f),
        new(-324.113f, 251.033f), new(-324.937f, 250.749f), new(-326.057f, 250.214f), new(-326.877f, 249.708f), new(-329.353f, 247.333f),
        new(-328.721f, 248.145f), new(-330.463f, 245.387f), new(-330.985f, 244.081f), new(-331.2f, 243.083f), new(-331.402f, 241.767f),
        new(-331.285f, 238.662f), new(-331.417f, 239.494f), new(-330.76f, 236.772f), new(-330.306f, 235.564f), new(-329.809f, 234.577f),
        new(-329.15f, 233.653f), new(-326.505f, 231.105f), new(-327.349f, 231.766f), new(-324.613f, 229.922f), new(-323.302f, 229.331f),
        new(-321.98f, 228.984f), new(-320.765f, 228.72f), new(-317.621f, 228.673f), new(-318.616f, 228.621f), new(-315.18f, 229.185f),
        new(-313.95f, 229.6f), new(-313.056f, 229.905f), new(-311.862f, 230.57f), new(-309.164f, 232.778f), new(-309.899f, 231.978f),
        new(-307.754f, 234.407f), new(-306.978f, 235.518f), new(-306.573f, 236.191f), new(-306.01f, 237.285f), new(-305.109f, 240.789f),
        new(-305.01f, 241.863f), new(-304.959f, 243.257f), new(-305.005f, 244.328f), new(-305.13f, 245.737f), new(-305.477f, 247.145f),
        new(-306.68f, 250.249f), new(-307.44f, 251.371f), new(-308.244f, 252.476f), new(-308.912f, 253.318f), new(-309.816f, 254.282f),
        new(-310.58f, 255.009f), new(-313.685f, 257.047f), new(-314.606f, 257.507f), new(-315.684f, 257.973f), new(-316.454f, 258.205f),
        new(-317.641f, 258.452f), new(-318.703f, 258.574f), new(-322.105f, 258.816f), new(-323.451f, 258.621f), new(-324.648f, 258.416f),
        new(-325.463f, 258.26f), new(-327.004f, 257.81f), new(-327.951f, 257.428f), new(-331.379f, 255.57f), new(-332.205f, 254.956f),
        new(-333.042f, 254.166f), new(-333.734f, 253.489f), new(-334.668f, 252.554f), new(-335.449f, 251.432f), new(-336.995f, 248.78f),
        new(-337.454f, 247.836f), new(-337.883f, 246.526f), new(-338.31f, 245.275f), new(-338.532f, 243.918f), new(-338.723f, 242.862f),
        new(-338.769f, 239.279f), new(-338.703f, 238.324f), new(-338.552f, 237.032f), new(-338.267f, 235.722f), new(-337.89f, 234.43f),
        new(-337.476f, 233.504f), new(-335.879f, 230.552f), new(-335.266f, 229.682f), new(-334.485f, 228.668f), new(-333.901f, 227.958f),
        new(-333.054f, 227.11f), new(-332.075f, 226.145f), new(-329.27f, 224.075f), new(-328.143f, 223.427f), new(-326.898f, 222.864f),
        new(-325.57f, 222.44f), new(-324.779f, 222.199f), new(-323.492f, 221.831f), new(-319.81f, 221.274f), new(-318.366f, 221.28f),
        new(-317.007f, 221.323f), new(-315.829f, 221.451f), new(-314.736f, 221.627f), new(-313.548f, 221.932f), new(-310.199f, 223.137f),
        new(-308.654f, 223.893f), new(-307.957f, 224.272f), new(-306.781f, 224.992f), new(-305.72f, 225.855f), new(-304.877f, 226.606f),
        new(-302.371f, 229.25f), new(-301.601f, 230.334f), new(-300.82f, 231.456f), new(-300.257f, 232.447f), new(-299.772f, 233.354f),
        new(-299.286f, 234.463f), new(-298.233f, 237.643f), new(-298.623f, 236.332f), new(-297.778f, 239.817f), new(-297.632f, 241.241f),
        new(-297.626f, 245.07f), new(-297.572f, 243.697f), new(-297.994f, 247.525f), new(-297.796f, 246.47f), new(-298.587f, 249.707f),
        new(-299.063f, 251.1f), new(-299.573f, 252.36f), new(-300.135f, 253.463f), new(-302.107f, 256.589f), new(-302.861f, 257.562f),
        new(-303.571f, 258.44f), new(-304.752f, 259.605f), new(-305.529f, 260.292f), new(-306.461f, 261.118f), new(-308.959f, 262.798f),
        new(-309.749f, 263.216f), new(-310.918f, 263.803f), new(-312.172f, 264.368f), new(-313.462f, 264.837f), new(-316.514f, 265.724f),
        new(-318.21f, 265.957f), new(-319.18f, 266.052f), new(-320.408f, 266.134f), new(-321.782f, 266.177f), new(-323.072f, 266.091f),
        new(-324.005f, 265.981f), new(-327.881f, 265.242f), new(-328.838f, 264.999f), new(-330.333f, 264.438f), new(-331.262f, 264.031f),
        new(-332.395f, 263.481f), new(-333.394f, 262.868f), new(-336.332f, 260.94f), new(-337.261f, 260.207f), new(-338.298f, 259.305f),
        new(-338.968f, 258.681f), new(-339.769f, 257.849f), new(-340.662f, 256.834f), new(-342.61f, 253.812f), new(-343.09f, 252.902f),
        new(-343.731f, 251.784f), new(-344.118f, 250.83f), new(-344.53f, 249.674f), new(-344.861f, 248.716f), new(-345.791f, 245.207f),
        new(-346.011f, 243.858f), new(-346.122f, 242.417f), new(-346.148f, 241.349f), new(-346.135f, 239.908f), new(-346.093f, 238.542f),
        new(-345.563f, 235.086f), new(-345.169f, 233.806f), new(-344.849f, 232.469f), new(-344.583f, 231.625f), new(-342.923f, 228.097f),
        new(-343.634f, 229.274f), new(-341.674f, 226.088f), new(-340.981f, 225.067f), new(-340.163f, 223.972f), new(-339.328f, 222.93f),
        new(-338.43f, 221.879f), new(-337.567f, 221.101f), new(-334.617f, 218.762f), new(-333.409f, 217.964f), new(-332.261f, 217.235f),
        new(-331.062f, 216.657f), new(-327.552f, 215.237f), new(-328.835f, 215.731f), new(-325.014f, 214.625f), new(-323.657f, 214.359f),
        new(-322.633f, 214.159f), new(-320.937f, 213.987f), new(-317.22f, 214.023f), new(-318.586f, 213.973f), new(-314.699f, 214.221f),
        new(-313.353f, 214.464f), new(-312.566f, 214.605f), new(-310.913f, 215.055f), new(-307.367f, 216.314f), new(-308.268f, 215.929f),
        new(-305.015f, 217.461f), new(-303.568f, 218.39f), new(-302.817f, 218.883f), new(-301.4f, 219.845f), new(-298.665f, 222.211f),
        new(-299.522f, 221.373f), new(-296.836f, 224.166f), new(-296.249f, 224.892f)
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 20 ? 20 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var maxC = Math.Min(max, count - NumCasts);
        if (NumCasts < 227)
            aoes[NumCasts].Color = Colors.Danger;
        return aoes.Slice(NumCasts, maxC);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (_aoes.Count == 0 && actor.OID == (uint)OID.AquaBubble)
        {
            var lastActivationTime = 8d;
            for (var i = 0; i < 228; ++i)
            {
                var additionalTime = ((i & 1) == 0) ? 0.16d : 0.36d;
                var activationTime = WorldState.FutureTime(lastActivationTime);
                _aoes.Add(new(circle, coordinates[i], default, activationTime));
                lastActivationTime += additionalTime;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BallOfIce)
        {
            if (++NumCasts == 228)
            {
                _aoes.Clear();
                NumCasts = 0;
            }
        }
    }
}
