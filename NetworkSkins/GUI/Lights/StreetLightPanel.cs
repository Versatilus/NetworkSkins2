﻿using NetworkSkins.GUI.Abstraction;
using UnityEngine;

namespace NetworkSkins.GUI.Lights
{
    public class StreetLightPanel : ListPanelBase<StreetLightList, PropInfo>
    {
        private DistancePanel distancePanel;

        public override void Build(PanelType panelType, Layout layout) {
            base.Build(panelType, layout);
            distancePanel = AddUIComponent<DistancePanel>();
            distancePanel.Build(panelType, new Layout(new Vector2(390.0f, 100.0f), true, ColossalFramework.UI.LayoutDirection.Vertical, ColossalFramework.UI.LayoutStart.TopLeft, 5));
        }

        protected override void OnPanelBuilt() {
            laneTabstripContainer.isVisible = false;
            pillarTabstrip.isVisible = false;
            Refresh();
        }

        protected override void OnItemClick(string itemID)
        {
            NetworkSkinPanelController.StreetLight.SetSelectedItem(itemID);
        }
    }

    public class StreetLightList : ListBase<PropInfo> { }
}
