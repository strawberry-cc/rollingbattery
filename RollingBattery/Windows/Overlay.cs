using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Numerics;

namespace RollingBattery;

public class OverlayElement : IDisposable
{
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly Configuration config;
    private readonly IPlayerState playerState;
    private readonly ICondition condition;
    private readonly IFramework framework;

    private int? lastUsedBattery = null;
    private int? secondLastUsedBattery = null;
    private int previousBattery = 0;

    private readonly IFontHandle fontHandle;

    private bool lastInCombat = false; // track previous combat state

    public OverlayElement(
        IDalamudPluginInterface pluginInterface,
        Configuration config,
        IPlayerState playerState,
        ICondition condition,
        IFramework framework)
    {
        this.pluginInterface = pluginInterface;
        this.config = config;
        this.playerState = playerState;
        this.condition = condition;
        this.framework = framework;

        // Create the font handle
        fontHandle = pluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(
            new GameFontStyle(GameFontFamily.Axis, 32f));

        pluginInterface.UiBuilder.Draw += DrawOverlay;
        framework.Update += OnFrameworkUpdate; // poll combat state
    }

    public void Dispose()
    {
        pluginInterface.UiBuilder.Draw -= DrawOverlay;
        framework.Update -= OnFrameworkUpdate;
    }

    public void ResetBatteryHistory()
    {
        lastUsedBattery = null;
        secondLastUsedBattery = null;
        previousBattery = 0;
    }

    private void OnFrameworkUpdate(IFramework _)
    {
        // get current in-combat state
        bool inCombat = condition[ConditionFlag.InCombat];

        // detect leaving combat
        if (lastInCombat && !inCombat)
        {
            ResetBatteryHistory(); // reset gauge history
        }

        lastInCombat = inCombat;
    }

    private void DrawOverlay()
    {
        var player = playerState;
        if (player == null || player.ClassJob.RowId != 31) // MCH check
            return;

        var drawList = ImGui.GetBackgroundDrawList();
        float fontScale = config.TextScale;
        var basePos = new Vector2(config.TextPosX, config.TextPosY);
        float lineHeight = ImGui.GetFontSize() * fontScale + 12;

        var gauge = Plugin.JobGauge;
        if (gauge == null) return;

        var jobGauge = gauge.Get<MCHGauge>();
        if (jobGauge == null) return;

        if (condition[ConditionFlag.InCombat])
        {
            int battery = jobGauge.Battery;

            if (previousBattery > 0 && battery == 0)
            {
                secondLastUsedBattery = lastUsedBattery;
                lastUsedBattery = previousBattery;
            }

            previousBattery = battery;
        }

        string[] lines =
        {
            $"LAST: {(lastUsedBattery.HasValue ? lastUsedBattery.Value.ToString() : "---")}",
            $"   2ND: {(secondLastUsedBattery.HasValue ? secondLastUsedBattery.Value.ToString() : "---")}"
        };

        // Text colors
        uint textColor = ImGui.GetColorU32(config.TextColor);
        uint outlineColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f));

        fontHandle.Push();

        for (int i = 0; i < lines.Length; i++)
        {
            var pos = basePos + new Vector2(0, i * lineHeight);

            float currentScale = fontScale;
            if (i == 1) currentScale *= 0.8f;

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
