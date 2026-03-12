using Content.Shared.DoAfter;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.NanoChat;

[Serializable, NetSerializable]
public sealed partial class PdaPhotoPrintToFaxDoAfterEvent(EntityUid cardUid, NanoChatPhotoData photo) : SimpleDoAfterEvent
{
    public EntityUid CardUid = cardUid;
    public NanoChatPhotoData Photo = photo;

    public PdaPhotoPrintToFaxDoAfterEvent() : this(EntityUid.Invalid, default)
    {
    }

    public override DoAfterEvent Clone() => this;
}
