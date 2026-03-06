// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Photo;

[Serializable, NetSerializable]
public sealed class PhotoCardUiState : BoundUserInterfaceState
{
    public byte[]? ImageData { get; }
    public string? CustomName { get; }
    public string? Caption { get; }

    public PhotoCardUiState(byte[]? imageData, string? customName, string? caption)
    {
        ImageData = imageData;
        CustomName = customName;
        Caption = caption;
    }
}

[Serializable, NetSerializable]
public enum PhotoCardUiKey : byte
{
    Key
}
