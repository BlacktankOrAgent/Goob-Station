using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.SelfHarm;

/// <summary>
/// Allows a character to harm themselves with their natural damage types.
/// Uses claws for slash damage, punch for brute damage only.
/// </summary>
public sealed partial class SelfHarmSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

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

        // Apply the damage to self
        _damageable.TryChangeDamage(performer, damage.Value, true, origin: performer);

        // Show feedback
        var attackType = damage.Value.DamageDict.ContainsKey("Slash") ? "claws" : "fist";
        _popup.PopupEntity($"You slash yourself with your {attackType}!", performer, performer, PopupType.MediumCaution);
        _popup.PopupEntity($"{performer} slashes themselves with their {attackType}!", performer, PopupType.MediumCaution);
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
