using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace HviKMod.Modifiers.Guesser;

/// <summary>
/// Rollen-Auswahl fuer den Guesser. Nutzt das Shapeshifter-Menue des Spiels als Vorlage
/// (gleiches Aussehen wie die Spielerauswahl), zeigt aber Rollennamen statt Spieler.
/// </summary>
[RegisterInIl2Cpp]
public class GuessMenu(IntPtr ptr) : Minigame(ptr)
{
    public ShapeshifterPanel panelPrefab = null!;
    public float xStart;
    public float yStart;
    public float xOffset;
    public float yOffset;
    public UiElement backButton = null!;

    public static GuessMenu Create()
    {
        var shapeshifter = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter).TryCast<ShapeshifterRole>()!;
        var original = Object.Instantiate(shapeshifter.ShapeshifterMenu);
        var menu = original.gameObject.AddComponent<GuessMenu>();

        menu.panelPrefab = original.PanelPrefab;
        menu.xStart = original.XStart;
        menu.yStart = original.YStart;
        menu.xOffset = original.XOffset;
        menu.yOffset = original.YOffset;
        menu.backButton = original.BackButton;
        menu.CloseSound = original.CloseSound;
        menu.OpenSound = original.OpenSound;
        menu.logger = original.logger;

        var back = menu.backButton.GetComponent<PassiveButton>();
        back.OnClick.RemoveAllListeners();
        back.OnClick.AddListener((UnityAction)menu.Close);

        original.DestroyImmediate();

        menu.transform.SetParent(Camera.main!.transform, false);
        menu.transform.localPosition = new Vector3(0f, 0f, -50f);
        return menu;
    }

    public override void Begin(PlayerTask task) => throw new NotSupportedException("Begin(roles, onPick) benutzen.");

    [HideFromIl2Cpp]
    public void Begin(List<RoleBehaviour> roles, Action<RoleBehaviour> onPick)
    {
        MinigameStubs.Begin(this, null);

        var buttons = new Il2CppSystem.Collections.Generic.List<UiElement>();
        for (var i = 0; i < roles.Count; i++)
        {
            var role = roles[i];
            var panel = Object.Instantiate(panelPrefab, transform);
            panel.transform.localPosition = new Vector3(xStart + i % 3 * xOffset, yStart + i / 3 * yOffset, -1f);
            SetupPanel(panel, role, () =>
            {
                onPick(role);
                Close();
            });
            buttons.Add(panel.Button);
        }

        ControllerManager.Instance.OpenOverlayMenu(name, backButton, null, buttons);

        // Stimmfelder ausblenden, damit das Menue lesbar ist
        if (MeetingHud.Instance) MeetingHud.Instance.playerStates.Do(p => p.gameObject.SetActive(false));
    }

    public override void Close()
    {
        if (MeetingHud.Instance) MeetingHud.Instance.playerStates.Do(p => p.gameObject.SetActive(true));
        ControllerManager.Instance.CloseOverlayMenu(name);
        MinigameStubs.Close(this);
    }

    private static void SetupPanel(ShapeshifterPanel panel, RoleBehaviour role, Action onClick)
    {
        panel.shapeshift = onClick;
        panel.PlayerIcon.gameObject.SetActive(false);
        panel.LevelNumberText.transform.parent.gameObject.SetActive(false);

        var icon = panel.transform.Find("Nameplate/Highlight/ShapeshifterIcon");
        if (icon) icon.gameObject.SetActive(false);

        panel.NameText.text = GuessRoles.DisplayName(role);
        panel.NameText.color = GuessRoles.DisplayColor(role);
        panel.NameText.alignment = TextAlignmentOptions.Center;
    }
}
