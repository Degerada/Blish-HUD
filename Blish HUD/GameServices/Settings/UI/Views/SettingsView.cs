﻿using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views {
    public class SettingsView : SettingView<SettingCollection> {

        private FlowPanel _settingFlowPanel;

        private readonly SettingCollection _settings;

        private bool _lockBounds = true;

        public bool LockBounds {
            get => _lockBounds;
            set {
                if (_lockBounds == value) return;

                _lockBounds = value;

                UpdateBoundsLocking(_lockBounds);
            }
        }

        private ViewContainer _lastSettingContainer;

        public SettingsView(SettingCollection settings, int definedWidth = -1)
            : base(new SettingEntry<SettingCollection>() { Value = settings }, definedWidth) {
            _settings = settings;
        }

        public SettingsView(SettingEntry<SettingCollection> setting, int definedWidth = -1) : base(setting, definedWidth) {
            _settings = setting.Value;
        }

        private void UpdateBoundsLocking(bool locked) {
            if (_settingFlowPanel == null) return;

            _settingFlowPanel.ShowBorder  = !locked;
            _settingFlowPanel.CanCollapse = !locked;

            this.ViewTarget.HeightSizingMode = SizingMode.AutoSize;
        }

        protected override void BuildSetting(Panel buildPanel) {
            _settingFlowPanel = new FlowPanel() {
                Size                = buildPanel.Size,
                FlowDirection       = ControlFlowDirection.SingleTopToBottom,
                ControlPadding      = new Vector2(5, 2),
                OuterControlPadding = new Vector2(0, 5),
                WidthSizingMode     = SizingMode.Fill,
                HeightSizingMode    = SizingMode.AutoSize,
                Parent              = buildPanel
            };

            foreach (var setting in _settings) {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null) {
                    _lastSettingContainer = new ViewContainer() {
                        WidthSizingMode = SizingMode.Fill,
                        Parent          = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView) {
                        subSettingsView.LockBounds = false;
                    }
                }
            }

            UpdateBoundsLocking(_lockBounds);
        }

        protected override void RefreshDisplayName(string displayName) {
            _settingFlowPanel.Title = displayName;
        }

        protected override void RefreshDescription(string description) {
            _settingFlowPanel.BasicTooltipText = description;
        }

        protected override void RefreshValue(SettingCollection value) { /* NOOP */ }

    }
}
