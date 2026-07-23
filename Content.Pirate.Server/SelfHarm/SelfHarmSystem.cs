using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.SelfHarm;

/// <summary>
/// Allows a character to harm themselves with their natural damage types.
/// Uses claws for slash damage, punch for brute damage only.
/// Includes a 3 second do-after with messages visible to nearby entities.
/// </summary>
public sealed partial class SelfHarmSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    /// <summary>
    /// Time in seconds for self-harm action to complete
    /// </summary>
    private const float SelfHarmDuration = 3f;

    /// <summary>
    /// Species with claws that deal slash damage
    /// </summary>
    private static readonly HashSet<string> ClawSpecies = new()
    {
        "Shadowkin",
        "Tajaran",
        "Resomi",
        "Avali",
        "Reptilian",
        "Vulpkanin",
        "Harpy",
        "Hydrakin",
        "Rodentia",
        "Felinid"
    };

    /// <summary>
    /// Species with bare hands that deal brute damage
    /// </summary>
    private static readonly HashSet<string> BareHandsSpecies = new()
    {
        "Owyie",
        "Dwarf",
        "Human",
        "Oni",
        "Diona",
        "IPC",
        "Moth",
        "Slime",
        "Thaven"
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SelfHarmComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SelfHarmComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SelfHarmComponent, SelfHarmActionEvent>(OnSelfHarm);
        SubscribeLocalEvent<SelfHarmComponent, SelfHarmDoAfterEvent>(OnSelfHarmComplete);
    }

    private void OnMapInit(Entity<SelfHarmComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, "ActionSelfHarm");
        Dirty(ent.Owner, ent.Comp);
    }

    private void OnShutdown(Entity<SelfHarmComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ActionEntity != null)
            _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    private void OnSelfHarm(Entity<SelfHarmComponent> ent, ref SelfHarmActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var performer = args.Performer;
        var damage = DetermineDamageType(performer);

        if (damage == null)
        {
            _popup.PopupClient("You cannot harm yourself!", performer, performer);
            return;
        }

        // Determine attack type for messages
        var attackType = damage.Value.DamageDict.ContainsKey("Slash") ? "claws" : "fist";

        // Show startup message to nearby entities
        _popup.PopupEntity($"[color=red]{performer} begins to harm themselves with their {attackType}![/color]", performer, PopupType.MediumCaution);

        // Start the do-after delay
        var doAfterArgs = new DoAfterArgs(EntityManager, performer, TimeSpan.FromSeconds(SelfHarmDuration), new SelfHarmDoAfterEvent(), performer)
        {
            BreakOnDamage = false,
            BreakOnMove = false,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnSelfHarmComplete(Entity<SelfHarmComponent> ent, ref SelfHarmDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var performer = ent.Owner;
        var damage = DetermineDamageType(performer);

        if (damage == null)
        {
            _popup.PopupClient("You cannot harm yourself!", performer, performer);
            return;
        }

        // Apply the damage to self
        _damageable.TryChangeDamage(performer, damage.Value, true, origin: performer);

        // Show completion feedback to everyone nearby
        var attackType = damage.Value.DamageDict.ContainsKey("Slash") ? "claws" : "fist";
        _popup.PopupEntity($"[color=red]{performer} harms themselves with their {attackType}![/color]", performer, PopupType.MediumCaution);
    }

    /// <summary>
    /// Determines what damage type to use based on the character's species.
    /// </summary>
    private DamageSpecifier? DetermineDamageType(EntityUid uid)
    {
        // Check if entity has a humanoid component and get their species
        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            var speciesId = humanoid.Species.ToString();

            // Check if this species has claws
            if (ClawSpecies.Contains(speciesId))
            {
                // Slash damage for creatures with claws
                return new DamageSpecifier(_protoManager.Index<DamageTypePrototype>("Slash"), 10);
            }

            // Check if this species has bare hands
            if (BareHandsSpecies.Contains(speciesId))
            {
                // Brute damage for creatures with bare hands
                return new DamageSpecifier(_protoManager.Index<DamageGroupPrototype>("Brute"), 8);
            }
        }

        // Default fallback for unknown species - bare hands
        return new DamageSpecifier(_protoManager.Index<DamageGroupPrototype>("Brute"), 8);
    }
}

/// <summary>
/// Event raised when a self-harm action is performed.
/// </summary>
public sealed partial class SelfHarmActionEvent : InstantActionEvent
{
}

/// <summary>
/// Event raised when the do-after for self-harm completes.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SelfHarmDoAfterEvent : SimpleDoAfterEvent
{
}
