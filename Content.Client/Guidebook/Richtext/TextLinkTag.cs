// SPDX-FileCopyrightText: 2023 Nemanja
// SPDX-FileCopyrightText: 2025 sleepyyapril
// SPDX-FileCopyrightText: 2026 little-meow-meow
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Client.Guidebook.RichText;

[UsedImplicitly]
public sealed class TextLinkTag : IMarkupTag
{
    [Dependency] private readonly IUriOpener _uriOpener = default!; // Den

    public string Name => "textlink";

    public Control? Control;

    /// <inheritdoc/>
    public bool TryGetControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        if (!node.Value.TryGetString(out var text)
            || !node.Attributes.TryGetValue("link", out var linkParameter)
            || !linkParameter.TryGetString(out var link))
        {
            control = null;
            return false;
        }

        var label = new Label();
        label.Text = text;

        label.MouseFilter = Control.MouseFilterMode.Stop;
        label.FontColorOverride = Color.CornflowerBlue;
        label.DefaultCursorShape = Control.CursorShape.Hand;

        label.OnMouseEntered += _ => label.FontColorOverride = Color.LightSkyBlue;
        label.OnMouseExited += _ => label.FontColorOverride = Color.CornflowerBlue;
        label.OnKeyBindDown += args => OnKeybindDown(args, link);

        control = label;
        Control = label;
        return true;
    }

    private void OnKeybindDown(GUIBoundKeyEventArgs args, string link)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (Control == null)
            return;

        var current = Control;
        while (current != null)
        {
            current = current.Parent;

            if (current is not ILinkClickHandler handler)
                continue;

            // Den begin: return if click was consumed
            if (handler.HandleClick(link))
                return;
            // Den end
        }

        // Den add: handle web links
        if (link.StartsWith("http://") || link.StartsWith("https://"))
        {
            _uriOpener.OpenUri(link);
            return;
        }
        // Den end

        Logger.Warning($"Warning! No valid ILinkClickHandler found.");
    }
}

public interface ILinkClickHandler
{
    /// <summary>
    /// Fired when a link nested inside a control is clicked.
    /// </summary>
    /// <param name="link"></param>
    /// <returns><value>true</value> to consume the event</returns>
    bool HandleClick(string link); // Den: void -> bool, implicit access modifier
}
