using System;
using System.Collections.Generic;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace Aerolt.UI.Sections
{
    public class WorldSection : IAeroltSection
    {
        private Slider _gameSpeed = null!;
        private Text _gameSpeedValue = null!;
        private Dropdown _difficultyDropdown = null!;
        private readonly Dictionary<ArtifactIndex, Toggle> _artifactToggles = new();
        private bool _suppress;

        public string Title => "World";
        public GameObject Root { get; private set; } = null!;

        public void Build(GameObject contentParent)
        {

            Root = UIFactory.CreateVerticalGroup(contentParent, "WorldSection", false, false, true, true, 6,
                new Vector4(4, 4, 4, 4), new Color(0.14f, 0.14f, 0.14f));
            UIFactory.SetLayoutElement(Root, flexibleWidth: 9999, minHeight: 10, flexibleHeight: 0);

            var content = Root;

            BuildGameSpeed(content);
            BuildStageActions(content);
            BuildRunTimer(content);
            BuildStagesCleared(content);
            BuildDifficulty(content);
            BuildDirector(content);
            BuildArtifacts(content);
        }

        public void OnShown()
        {
            SyncDifficulty();
            SyncArtifacts();
            if (_gameSpeed) _gameSpeed.value = Time.timeScale;
        }

        private void BuildGameSpeed(GameObject parent)
        {
            Header(parent, "Game Speed (local)");
            var row = Row(parent);
            UIFactory.CreateSlider(row, "GameSpeedSlider", out _gameSpeed);
            UIFactory.SetLayoutElement(_gameSpeed.gameObject, minHeight: 25, minWidth: 200, flexibleWidth: 9999);
            _gameSpeed.minValue = 0f;
            _gameSpeed.maxValue = 5f;
            _gameSpeed.value = Time.timeScale;
            _gameSpeedValue = UIFactory.CreateLabel(row, "GameSpeedValue", "1.00x", TextAnchor.MiddleCenter);
            UIFactory.SetLayoutElement(_gameSpeedValue.gameObject, minWidth: 60, flexibleWidth: 0);
            _gameSpeed.onValueChanged.AddListener(v =>
            {
                Time.timeScale = v;
                _gameSpeedValue.text = $"{v:0.00}x";
            });
            var reset = UIFactory.CreateButton(row, "GameSpeedReset", "Reset");
            UIFactory.SetLayoutElement(reset.Component.gameObject, minHeight: 25, minWidth: 60, flexibleWidth: 0);
            reset.OnClick += () =>
            {
                Time.timeScale = 1f;
                _gameSpeed.value = 1f;
            };
        }


        private void BuildStageActions(GameObject parent)
        {
            Header(parent, "Stage");
            var row = Row(parent);
            Button(row, "Charge Teleporter", () => new TeleporterChargeMessage().SendToServer());
            Button(row, "Next Stage", () => new SceneChangeMessage().SendToServer());
            Button(row, "Restart Stage", RestartStage);
        }

        private static void RestartStage()
        {
            var current = SceneCatalog.GetSceneDefForCurrentScene();
            if (current)
                new SceneChangeMessage(current.sceneDefIndex).SendToServer();
        }


        private void BuildRunTimer(GameObject parent)
        {
            Header(parent, "Run Timer (seconds)");
            var row = Row(parent);
            var input = UIFactory.CreateInputField(row, "RunTimerInput", "seconds...");
            UIFactory.SetLayoutElement(input.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            input.Component.contentType = InputField.ContentType.DecimalNumber;
            Button(row, "Set", () =>
            {
                if (float.TryParse(input.Text, out var v))
                    new RunControlMessage(RunControlType.SetStopwatch, v).SendToServer();
            });
        }

        private void BuildStagesCleared(GameObject parent)
        {
            Header(parent, "Stages Cleared");
            var row = Row(parent);
            var input = UIFactory.CreateInputField(row, "StagesInput", "count...");
            UIFactory.SetLayoutElement(input.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            input.Component.contentType = InputField.ContentType.IntegerNumber;
            Button(row, "Set", () =>
            {
                if (int.TryParse(input.Text, out var v))
                    new RunControlMessage(RunControlType.SetStagesCleared, v).SendToServer();
            });
        }

        private void BuildDifficulty(GameObject parent)
        {
            Header(parent, "Difficulty");
            var row = Row(parent);

            var names = new List<string>();
            for (var i = 0; i < DifficultyCatalog.standardDifficultyCount; i++)
            {
                var def = DifficultyCatalog.GetDifficultyDef((DifficultyIndex)i);
                names.Add(def != null ? Language.GetString(def.nameToken) : $"Difficulty {i}");
            }

            UIFactory.CreateDropdown(row, "DifficultyDropdown", out _difficultyDropdown, "Difficulty", 14,
                OnDifficultyChanged, names.ToArray());
            UIFactory.SetLayoutElement(_difficultyDropdown.gameObject, minHeight: 28, flexibleWidth: 9999);
        }

        private void OnDifficultyChanged(int index)
        {
            if (_suppress) return;
            new RunControlMessage(RunControlType.SetDifficulty, index).SendToServer();
        }

        private void SyncDifficulty()
        {
            if (!Run.instance || _difficultyDropdown == null) return;
            _suppress = true;
            var idx = (int)Run.instance.selectedDifficulty;
            if (idx >= 0 && idx < _difficultyDropdown.options.Count)
                _difficultyDropdown.value = idx;
            _suppress = false;
        }

        private void BuildDirector(GameObject parent)
        {
            Header(parent, "Combat Director");
            var row = Row(parent);
            Button(row, "Stop Spawns", () => new DirectorControlMessage(DirectorControlType.StopSpawns).SendToServer());
            Button(row, "Resume Spawns",
                () => new DirectorControlMessage(DirectorControlType.ResumeSpawns).SendToServer());
            Button(row, "Start Boss", () => new DirectorControlMessage(DirectorControlType.StartBoss).SendToServer());

            var creditRow = Row(parent);
            var input = UIFactory.CreateInputField(creditRow, "CreditsInput", "monster credits...");
            UIFactory.SetLayoutElement(input.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            input.Component.contentType = InputField.ContentType.DecimalNumber;
            Button(creditRow, "Set Credits", () =>
            {
                if (float.TryParse(input.Text, out var v))
                    new DirectorControlMessage(DirectorControlType.SetCredits, v).SendToServer();
            });
        }

        private void BuildArtifacts(GameObject parent)
        {
            Header(parent, "Artifacts");
            for (var i = 0; i < ArtifactCatalog.artifactCount; i++)
            {
                var index = (ArtifactIndex)i;
                var def = ArtifactCatalog.GetArtifactDef(index);
                if (!def) continue;

                var name = !string.IsNullOrEmpty(def.nameToken) ? Language.GetString(def.nameToken) : def.cachedName;
                var captured = index;
                var obj = UIFactory.CreateToggle(parent, $"Artifact_{def.cachedName}", out var toggle, out var text);
                UIFactory.SetLayoutElement(obj, minHeight: 26, flexibleWidth: 9999);
                text.text = name;
                toggle.onValueChanged.AddListener(on =>
                {
                    if (_suppress) return;
                    new ArtifactMessage(captured, on).SendToServer();
                });
                _artifactToggles[index] = toggle;
            }
        }

        private void SyncArtifacts()
        {
            if (!RunArtifactManager.instance) return;
            _suppress = true;
            foreach (var kvp in _artifactToggles)
                kvp.Value.isOn = RunArtifactManager.instance.IsArtifactEnabled(kvp.Key);
            _suppress = false;
        }

        private static void Header(GameObject parent, string text)
        {
            var label = UIFactory.CreateLabel(parent, "Header", text, TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(label.gameObject, minHeight: 24, flexibleWidth: 9999);
            label.fontStyle = FontStyle.Bold;
        }

        private static GameObject Row(GameObject parent)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, "Row", false, false, true, true, 4);
            UIFactory.SetLayoutElement(row, minHeight: 30, flexibleHeight: 0, flexibleWidth: 9999);
            return row;
        }

        private static void Button(GameObject parent, string label, Action onClick)
        {
            var btn = UIFactory.CreateButton(parent, $"Btn_{label}", label);
            UIFactory.SetLayoutElement(btn.Component.gameObject, minHeight: 28, flexibleWidth: 9999);
            btn.OnClick += onClick;
        }
    }
}
