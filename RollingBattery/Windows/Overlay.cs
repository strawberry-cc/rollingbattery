using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace RollingBattery;

public class OverlayElement : IDisposable
{
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly Configuration config;

    private int? lastUsedBattery = null;
    private int? secondLastUsedBattery = null;
    private int previousBattery = 0;

    private readonly IFontHandle fontHandle;

    public OverlayElement(IDalamudPluginInterface pluginInterface, Configuration config)
    {
        this.pluginInterface = pluginInterface;
        this.config = config;

        // Create the font handle once here:
        fontHandle = pluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(
            new GameFontStyle(GameFontFamily.Axis, 32f));

        pluginInterface.UiBuilder.Draw += DrawOverlay;
    }

    public void Dispose()
    {
        pluginInterface.UiBuilder.Draw -= DrawOverlay;
        // You can Dispose fontHandle here if needed, but usually not required
    }
    public void ResetBatteryHistory()
    {
        lastUsedBattery = null;
        secondLastUsedBattery = null;
    }

    private void DrawOverlay()
    {
        var drawList = ImGui.GetBackgroundDrawList();
        float fontScale = config.TextScale;
        var basePos = new Vector2(config.TextPosX, config.TextPosY);
        float lineHeight = ImGui.GetFontSize() * fontScale + 12;

        var gauge = Plugin.JobGauge;
        if (gauge == null) return;

        var jobGauge = gauge.Get<MCHGauge>();
        if (jobGauge == null) return;

        int battery = jobGauge.Battery;

        if (previousBattery > 0 && battery == 0)
        {
            secondLastUsedBattery = lastUsedBattery;
            lastUsedBattery = previousBattery;
        }

        previousBattery = battery;

        string[] lines =
        {
            $"LAST: {(lastUsedBattery.HasValue ? lastUsedBattery.Value.ToString() : "---")}",
            $"   2ND: {(secondLastUsedBattery.HasValue ? secondLastUsedBattery.Value.ToString() : "---")}"
        };

        // Use config color
        uint textColor = ImGui.GetColorU32(config.TextColor);
        uint outlineColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f)); // Keep outline black

        fontHandle.Push();

        for (int i = 0; i < lines.Length; i++)
        {
            var pos = basePos + new Vector2(0, i * lineHeight);

            float currentScale = fontScale;
            if (i == 1) currentScale *= 0.8f; // scale down the "2ND" line

            float outlineOffset = 1.5f;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize() * currentScale,
                        pos + new Vector2(dx * outlineOffset, dy * outlineOffset),
                        outlineColor, lines[i]);
                }
            }

            drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize() * currentScale,
                pos, textColor, lines[i]);
        }

        fontHandle.Pop();
    }
}
