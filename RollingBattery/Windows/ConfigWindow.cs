using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace RollingBattery.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base("RollingBattery Config###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(500, 200);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        /* 
        var configValue = Configuration.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
            Configuration.Save();
        }

        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }
        */

        // === Sliders for overlay text ===
        float scale = Configuration.TextScale;
        if (ImGui.SliderFloat("Text Scale", ref scale, 0.5f, 3.0f, "%.1f"))
        {
            Configuration.TextScale = scale;
            Configuration.Save();
        }

        float xpos = Configuration.TextPosX;
        if (ImGui.SliderFloat("Text X", ref xpos, 0f, 2560f, "%.0f"))
        {
            Configuration.TextPosX = xpos;
            Configuration.Save();
        }

        float ypos = Configuration.TextPosY;
        if (ImGui.SliderFloat("Text Y", ref ypos, 0f, 2560f, "%.0f"))
        {
            Configuration.TextPosY = ypos;
            Configuration.Save();
        }

        var textColor = Configuration.TextColor;
        if (ImGui.ColorEdit4("Text Color", ref textColor))
        {
            Configuration.TextColor = textColor;
            Configuration.Save();
        }

    }
}
