using Dalamud.Game.Command;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using RollingBattery.Windows;
using System;
using System.IO;

namespace RollingBattery;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IJobGauges JobGauge { get; private set; } = null!;
    private const string CommandName = "/rbc";
    // private const string ResetRollingBattery = "/rrb";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("RollingBattery");
    private ConfigWindow ConfigWindow { get; init; }
    private OverlayElement OverlayElement { get; init; }
    //private MainWindow MainWindow { get; init; }
    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        //MainWindow = new MainWindow(this, goatImagePath);
        OverlayElement = new OverlayElement(PluginInterface, Configuration, PlayerState, Condition, Framework);

        WindowSystem.AddWindow(ConfigWindow);
        //WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open config window."
        });

        CommandManager.AddHandler("/rrb", new CommandInfo(OnResetCommand)
        {
            HelpMessage = "Reset battery history display."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        // PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [RollingBattery] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        //MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler("/rrb");
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleConfigUI();
    }

    private void OnResetCommand(string command, string args)
    {
        OverlayElement.ResetBatteryHistory();
        Log.Information("Battery history reset.");
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    // public void ToggleMainUI() => MainWindow.Toggle();

    private MainWindow? debugWindow;
    public void OpenDebugWindow()
    {
        if (debugWindow == null)
        {
            var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            debugWindow = new MainWindow(this, goatImagePath);
        }
        WindowSystem.AddWindow(debugWindow);
        debugWindow.IsOpen = true;

    }
}

