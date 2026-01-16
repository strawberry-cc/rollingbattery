using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace RollingBattery.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private Configuration config;

    public ConfigWindow(Plugin plugin) : base("RollingBattery Config###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(500, 200);
        SizeCondition = ImGuiCond.Always;

        this.plugin = plugin;
        config = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        if (config.IsConfigWindowMovable)
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
        if (ImGui.Button("Open Debug Window"))
        {
            plugin.OpenDebugWindow();
        }

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
        float scale = config.TextScale;
        if (ImGui.SliderFloat("Text Scale", ref scale, 0.5f, 3.0f, "%.1f"))
        {
            config.TextScale = scale;
            config.Save();
        }

        float xpos = config.TextPosX;
        if (ImGui.SliderFloat("Text X", ref xpos, 0f, 2560f, "%.0f"))
        {
            config.TextPosX = xpos;
            config.Save();
        }

        float ypos = config.TextPosY;
        if (ImGui.SliderFloat("Text Y", ref ypos, 0f, 2560f, "%.0f"))
        {
            config.TextPosY = ypos;
            config.Save();
        }

        var textColor = config.TextColor;
        if (ImGui.ColorEdit4("Text Color", ref textColor))
        {
            config.TextColor = textColor;
            config.Save();
        }

    }
}
