using Robust.Shared.Audio;
using Robust.Shared.GameStates;
namespace Content.Shared._Pirate.Weapons.Melee.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class TimedDeflectBlockComponent : Component
{
    [DataField]
    public float DeflectWindow = 0.5f;

    [DataField]
    public float BlockStaminaDamageFraction = 0.1f;

    [DataField]
    public float DeflectStaminaRegenFraction;

    [DataField]
    public SoundSpecifier DeflectSound = new SoundCollectionSpecifier("PirateBladeDeflects");

    [DataField]
    public SoundSpecifier BlockSound = new SoundCollectionSpecifier("PirateBladeBlocks");

    [DataField]
    public float PowerGainOnDeflect = 10f;

    [DataField]
    public float PowerDecayDelay = 30f;

    [DataField]
    public float PowerDecayPerSecond = 0.5f;

    [DataField]
    public float MinPower;

    [DataField]
    public float MaxPower = 100f;

    [DataField]
    public float PowerPerLevel = 15f;

    [DataField]
    public int MaxLevel = 6;

    [DataField]
    public float DamageBonusPerLevel = 3f;

    [DataField]
    public float BlockStaminaDamageReductionPerLevel = 0.01f;

    [DataField]
    public float DeflectStaminaRegenBonusPerLevel = 0.01f;

    [DataField]
    public float DeflectWindowBonusPerLevel = 0.1f;

    [DataField]
    public float DeflectLagCompensationMultiplier = 1.5f;

    [DataField]
    public float MaxDeflectLagCompensation = 0.75f;

    [DataField]
    public string BonusDamageType = "Slash";

    [DataField]
    public string BaseVisualState = "dormant";

    [DataField]
    public string LevelVisualStatePrefix = "level-";

    [AutoNetworkedField]
    public float CurrentPower;
    public TimeSpan DeflectWindowEnd = TimeSpan.Zero;
    public TimeSpan LastDeflectTime = TimeSpan.Zero;
}
