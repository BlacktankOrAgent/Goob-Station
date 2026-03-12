// SPDX-FileCopyrightText: 2026 CyberLanos <cyber.lanos00@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.NanoChat;

[Serializable, NetSerializable]
public sealed partial class PdaPhotoUploadDoAfterEvent(EntityUid cardUid, NanoChatPhotoData photo) : SimpleDoAfterEvent
{
    #region Pirate: freeze nano chat upload payload
    public EntityUid CardUid = cardUid; // Pirate: freeze nano chat upload target
    public NanoChatPhotoData Photo = photo; // Pirate: freeze nano chat uploaded photo

    public PdaPhotoUploadDoAfterEvent() : this(EntityUid.Invalid, default) // Pirate: serialization ctor for frozen upload payload
    {
    }

    public override DoAfterEvent Clone() => this; // Pirate: payload is immutable for do-after completion
    #endregion
}
