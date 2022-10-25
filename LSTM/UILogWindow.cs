﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Steamworks;
using static UnityEngine.EventSystems.EventTrigger;
using System.Security.Policy;
using static System.Collections.Specialized.BitVector32;
using Compressions;


namespace LSTMMod
{
    public class UILogWindow : ManualBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler, MyWindow, TrafficLogDelegate
    {

        public RectTransform windowTrans;
        public RectTransform contentTrans;

        public MyListView listView;
        public Text countText;
        public int entryCount;
        public float newest;
        public float oldest;

        public int targetStationGid;
        //public int targetIndex;
        public int targetItemId;
        public int targetPlanetId;
        public int targetStarId;

        internal bool _eventLock;
        public bool isPointEnter;
        private bool focusPointEnter;

        //public Text itemText;
        public UIButton itemButton;
        public UIButton itemResetButton;
        public Image itemImage;
        public Image itemCircle;
        public static Sprite defaultItemSprite = null;


        int entryMax = 9999;
        List<TrafficLogData> _logList = new List<TrafficLogData>(2004);

        public static UILogWindow CreateInstance()
        {
            UILogWindow win = MyWindowCtl.CreateWindow<UILogWindow>("LSTMLogWindow", "Traffic Log");

            return win;
        }

        public void BeginGame()
        {
            if (DSPGame.IsMenuDemo)
            {
                return;
            }
            targetItemId = 0;
            targetStationGid = 0;
        }

        //public void SetUpAndOpenItem(int _itemId)
        //{
        //    targetItemId = _itemId;
        //    targetStationGid = 0;
        //    SetUpData();
        //    //UIRoot.instance.uiGame.ShutPlayerInventory();
        //    MyWindowCtl.OpenWindow(this);
        //}

        public void SetUpAndOpenStationSlot(int itemId, int stationGid = 0, int planetId = 0, int StarId = 0)
        {
            targetItemId = itemId;
            targetStationGid = stationGid;
            //targetIndex = stationIndex;
            targetPlanetId = planetId;
            targetStarId = StarId;
            SetUpData();
            //UIRoot.instance.uiGame.ShutPlayerInventory();
            MyWindowCtl.OpenWindow(this);
        }

        public bool isFunctionWindow()
        {
            return false;
        }

        public Vector2 WindowSize()
        {
            float rows = Mathf.Round(10);
            if (rows < 4f)
            {
                rows = 4f;
            }
            if (rows > 16f)
            {
                rows = 16f;
            }
            return new Vector2(640, 174 + 28 * rows - 2);
        }

        private void PopulateItem(MonoBehaviour item, int rowIndex)
        {
            var child = item as UILogListItem;
            child.Init(_logList[rowIndex], this);
        }

        protected override void _OnCreate()
        {
            _eventLock = true;

            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = WindowSize();

            GameObject go = new GameObject("content");
            contentTrans = go.AddComponent<RectTransform>();
            Util.NormalizeRectWithMargin(contentTrans, 60f, 28f, 20f, 28f, windowTrans);

            //listview
            Image bgImage = Util.CreateGameObject<Image>("list-bg", 100f, 100f);
            bgImage.color = new Color(0f, 0f, 0f, 0.56f);
            Util.NormalizeRectWithMargin(bgImage, 70f, 0f, 20f, 16f, contentTrans);

            listView = MyListView.CreateListView(UILogListItem.CreateListViewPrefab(), this.PopulateItem, "log-list-view");
            //listView = MyListView.CreateListView2(UILogListItem.CreateListViewPrefab(), "log-list-view", this.PopulateItem);
            Util.NormalizeRectWithMargin(listView.transform, 0f, 0f, 0f, 0f, bgImage.transform);
            listView.m_ScrollRect.scrollSensitivity = 28f;
            //これを広めに取っておかないと上部が描画されない
            //listView.recyclingListView.PreAllocHeight = 280f*1.55f;

            //ここでサイズ調整…

            //(logListView.m_ItemRes.com_data.transform as RectTransform).sizeDelta = new Vector2(600f, 24f);

            ////scope buttons
            //float scopex_ = 4f;
            //UIButton AddScope(string label, int data)
            //{
            //    UIDESwarmPanel swarmPanel = UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel;
            //    UIButton src = swarmPanel.orbitButtons[0];
            //    UIButton btn = GameObject.Instantiate<UIButton>(src);
            //    // btn.transitions[0] btn btn.transitions[1]==text btn.transitions[2]==frame
            //    if (btn.transitions.Length >= 2)
            //    {
            //        btn.transitions[0].normalColor = new Color(0.1f, 0.1f, 0.1f, 0.68f);
            //        btn.transitions[0].highlightColorOverride = new Color(0.9906f, 0.5897f, 0.3691f, 0.4f);
            //        btn.transitions[1].normalColor = new Color(1f, 1f, 1f, 0.6f);
            //        btn.transitions[1].highlightColorOverride = new Color(0.2f, 0.1f, 0.1f, 0.9f);
            //    }
            //    Text btnText = btn.transform.Find("Text").GetComponent<Text>();
            //    btnText.text = label;
            //    if (btnText.font.name == "MPMK85") //JP MOD
            //    {
            //        btnText.fontSize = 16;
            //    }
            //    else // "DIN"
            //    {
            //        btnText.fontSize = 14;
            //    }
            //    btn.data = data;

            //    RectTransform btnRect = Util.NormalizeRectWithTopLeft(btn, scopex_, 0f, contentTrans);
            //    btnRect.sizeDelta = new Vector2(btnText.preferredWidth + 14f, 22f);
            //    btn.transform.Find("frame").gameObject.SetActive(false);
            //    //(btn.transform.Find("frame").transform as RectTransform).sizeDelta = btnRect.sizeDelta;
            //    scopex_ += btnRect.sizeDelta.x + 0f;

            //    return btn;
            //}
            //scopeButtons = new List<UIButton>(8);
            ////scopeButtons.Add(AddScope("Star", (int)Scope.Star));
            //scopeButtons.Add(AddScope("All Planet", (int)Scope.Planet));
            //scopeButtons.Add(AddScope("Current Star", (int)Scope.CurrentStar));
            //scopeButtons.Add(AddScope("Has Factory", (int)Scope.HasFactory));
            ////scopeButtons.Add(AddScope("★", (int)Scope.Fav));
            //scopeButtons.Add(AddScope("Recent", (int)Scope.Recent));
            //scope = Scope.Planet;
            //foreach (var btn in scopeButtons)
            //{
            //    btn.highlighted = (btn.data == (int)scope);
            //}

            countText = Util.CreateText("", 14, "result-count");
            Util.NormalizeRectWithBottomLeft(countText, 2f, 0f, contentTrans);

            if (defaultItemSprite == null)
            {
                defaultItemSprite = Util.LoadSpriteResource("Icons/Tech/1414");
            }

            Transform bgTrans;
            RectTransform rect;
            UIAssemblerWindow assemblerWindow = UIRoot.instance.uiGame.assemblerWindow;

            //icon
            bgTrans = assemblerWindow.resetButton.transform.parent; //circle-back
            if (bgTrans != null)
            {
                go = GameObject.Instantiate(bgTrans.gameObject);
                Transform btnTrans = go.transform.Find("product-icon");
                if (btnTrans != null)
                {
                    itemResetButton = go.transform.Find("stop-btn")?.GetComponent<UIButton>();
                    go.transform.Find("cnt-text")?.gameObject.SetActive(false);
                    //GameObject.Destroy(go.GetComponent<EventTrigger>());
                    GameObject.Destroy(go.transform.Find("circle-fg-1")?.gameObject);
                    GameObject.Destroy(go.transform.Find("product-icon-1")?.gameObject);
                    GameObject.Destroy(go.transform.Find("cnt-text-1")?.gameObject);

                    itemButton = btnTrans.GetComponent<UIButton>();
                    itemButton.tips.tipTitle = "Select Item".Translate();
                    itemButton.tips.tipText = "Select item to display".Translate();
                    itemButton.tips.corner = 3;
                    itemButton.tips.offset = new Vector2(16, 16);
                    itemCircle = go.transform.Find("circle-fg")?.GetComponent<Image>();
                    itemCircle.color = Util.DSPBlue;
                    itemImage = btnTrans.GetComponent<Image>();
                    itemImage.sprite = defaultItemSprite;
                    rect = Util.NormalizeRectD(go);
                    //rect.localScale = new Vector3(1f, 1f, 1f);
                    rect.SetParent(windowTrans, false);
                    rect.anchoredPosition = new Vector2(30f, -54f);
                    go.name = "item-button";
                    go.SetActive(true);
                }

            }

            //reload btn
            UIButton btn = Util.MakeSmallTextButton("Reload", 44f, 20f);
            btn.gameObject.name = "reload-btn";
            Util.NormalizeRectWithTopLeft(btn, 538f, 40f, contentTrans);
            btn.transform.SetParent(contentTrans, false);
            btn.onClick += OnReloadButtonClick;
            btn.tips.delay = 0.8f;
            btn.tips.tipTitle = "Reload".Translate();
            btn.tips.tipText = "Click to reload data".Translate();
            btn.tips.corner = 3;
            btn.tips.offset = new Vector2(6, 38);
            btn.gameObject.SetActive(true);

            //LSTM btn
            btn = Util.MakeSmallTextButton("LSTM", 44f, 20f);
            btn.gameObject.name = "lstm-btn";
            Util.NormalizeRectWithTopLeft(btn, 538f, 0f, contentTrans);
            btn.transform.SetParent(contentTrans, false);
            btn.onClick += OnLSTMButtonClick;
            btn.tips.delay = 0.8f;
            btn.tips.tipTitle = "LSTM".Translate();
            btn.tips.tipText = "Open LSTM with current state".Translate();
            btn.tips.corner = 3;
            btn.tips.offset = new Vector2(6, 38);
            btn.gameObject.SetActive(true);


            //
            Text titleText = MyWindowCtl.GetTitleText(this);
            if (titleText != null)
            {
                go = GameObject.Instantiate(titleText.gameObject);
                go.name = "station-name";
                stationText = go.GetComponent<Text>();
                stationText.fontSize = 20;
                stationText.alignment = TextAnchor.MiddleCenter;

                rect = Util.NormalizeRectC(go);
                rect.SetParent(windowTrans, false);
                rect.sizeDelta = new Vector2(240f, rect.sizeDelta.y);
                rect.anchoredPosition = new Vector2(0f, 130f); //planetText
                go.SetActive(true);

                go = GameObject.Instantiate(go, windowTrans);
                rect.anchoredPosition = new Vector2(0f, 100f); //stationText
                go.name = "planet-name";
                planetText = go.GetComponent<Text>();
                //ContentSizeFitter?

            }

            Sprite s = itemResetButton.transform.Find("x")?.GetComponent<Image>()?.sprite;
            planetResetButton = Util.MakeIconButtonB(s, 22);
            if (planetResetButton != null)
            {
                planetResetButton.gameObject.name = "planet-reset-btn";
                rect = Util.NormalizeRectC(planetResetButton.gameObject);
                rect.SetParent(windowTrans, false);
                rect.anchoredPosition = new Vector2(150f, 130f);
            }
            stationResetButton = Util.MakeIconButtonB(s, 22);
            if (stationResetButton != null)
            {
                stationResetButton.gameObject.name = "station-reset-btn";
                rect = Util.NormalizeRectC(stationResetButton.gameObject);
                rect.SetParent(windowTrans, false);
                rect.anchoredPosition = new Vector2(180f, 100f);
            }

            //menu
            CreateMenuBox();

            _eventLock = false;
        }

        private void OnScrollRectChanged(Vector2 val)
        {
            if (listView.m_ScrollRect.verticalScrollbar.size < 0.1f)
            {
                listView.m_ScrollRect.verticalScrollbar.size = 0.1f;
            }
            else if (listView.m_ScrollRect.verticalScrollbar.size >= 0.99f)
            {
                listView.m_ScrollRect.verticalScrollbar.size = 0.001f;
            }
        }
        protected override void _OnDestroy()
        {

        }

        protected override bool _OnInit()
        {
            windowTrans.anchoredPosition = new Vector2(370f, -446f + (windowTrans.sizeDelta.y / 2)); // pivot=0.5 なので /2
            //PLFN.mainWindowSize.SettingChanged += (sender, args) => {
            //    windowTrans.sizeDelta = WindowSize();
            //};
            //menuTarget = null;
            return true;
        }

        protected override void _OnFree()
        {

        }

        protected override void _OnRegEvent()
        {
            itemButton.onClick += OnSelectItemButtonClick;
            itemResetButton.onClick += OnItemResetButtonClick;
            planetResetButton.onClick += OnPlanetResetButtonClick;
            stationResetButton.onClick += OnStationResetButtonClick;

            listView.m_ScrollRect.onValueChanged.AddListener(OnScrollRectChanged);

        }



        protected override void _OnUnregEvent()
        {
            itemButton.onClick -= OnSelectItemButtonClick;
            itemResetButton.onClick -= OnItemResetButtonClick;
            planetResetButton.onClick -= OnPlanetResetButtonClick;
            stationResetButton.onClick -= OnStationResetButtonClick;

            listView.m_ScrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
        }

        protected override void _OnOpen()
        {

        }
        protected override void _OnClose()
        {
            isPointEnter = false;
        }
        protected override void _OnUpdate()
        {
            if (VFInput.escape && !UIRoot.instance.uiGame.starmap.active && !VFInput.inputing)
            {
                VFInput.UseEscape();
                if (LSTM._configWin.active)
                {
                    LSTM._configWin._Close();
                }
                //else if (menuComboBox.isDroppedDown)
                //{
                //    menuComboBox.isDroppedDown = false;
                //}
                else
                {
                    base._Close();
                }
            }
            if (_eventLock)
            {
                return;
            }

            bool valid = true;
            int step = Time.frameCount % 30;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                //int current = targetItemId;
                //targetItemId = itemSelection.NextTargetItemId(targetItemId, PLFN.showPowerState.Value);
                //if (targetItemId != current)
                //{
                //    valid = RefreshListView(logListView);
                //}
            }
            else if (step == 0)
            {
                //valid = RefreshListView(logListView);
                //UIListViewのStart()で設定されるのでその後に呼ぶ必要がある
                ////listView.m_ScrollRect.scrollSensitivity = 28f;
            }
            else
            {
                //RefreshListView(logListView, true);
            }
            if (!valid)
            {
                SetUpData();
            }

            //if (!menuComboBox.isDroppedDown && menuTarget != null)
            //{
            //    menuTarget.UnlockAppearance();
            //    menuTarget = null;
            //}
        }

        public void TryClose()
        {
            base._Close();
        }
        //ホイールズーム対策用 UITutorialWindowなどの真似
        public void OnPointerEnter(PointerEventData _eventData)
        {
            this.isPointEnter = true;
        }
        public void OnPointerExit(PointerEventData _eventData)
        {
            this.isPointEnter = false;
        }

        public void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                this.focusPointEnter = this.isPointEnter;
                this.isPointEnter = false;
            }
            if (focus)
            {
                this.isPointEnter = this.focusPointEnter;
            }
        }

        //internal List<TrafficLogData> _logList = new List<TrafficLogData>(2000);

        public bool sholdShowLogData(TrafficLogData logData)
        {
            if (targetStarId != 0)
            {
                if ((logData.fromPlanet / 100 != targetStarId) && (logData.toPlanet / 100 != targetStarId))
                {
                    return false;
                }
            }
            if (targetPlanetId != 0)
            {
                if ((logData.fromPlanet != targetPlanetId) && (logData.toPlanet != targetPlanetId))
                {
                    return false;
                }
            }
            if (targetItemId != 0)
            {
                if (logData.itemId != targetItemId)
                {
                    return false;
                }
            }
            if (targetStationGid != 0)
            {
                if ((logData.toStationGid != targetStationGid)
                    && (logData.fromStationGid != targetStationGid))
                {
                    return false;
                }
            }
            return true;
        }

        public void TrafficLogReseted()
        {
            _eventLock = true;
            listView?.Clear();
            //_logList.Clear();
            _eventLock = false;
        }
        //public void TrafficLogAdded(TrafficLogData logData)
        //{
        //    AddStore(logData);
        //}

        public void SetUpData()
        {
            _eventLock = true;
            //targetItemId = itemSelection.lastSelectedItemId;

            SetUpItemList();
            SetUpItemUI();
            //_logList.Sort((a, b) => a.distanceForSort - b.distanceForSort);

            _eventLock = false;

            //RefreshListView(logListView);

        }
        public Text planetText;
        public UIButton planetResetButton;
        public Text stationText;
        public UIButton stationResetButton;

        internal void SetUpItemUI()
        {
            int itemId = targetItemId;
            if (itemId <= 0)
            {
                itemCircle.fillAmount = 0f;
                itemResetButton.gameObject.SetActive(false);
                itemImage.sprite = defaultItemSprite;
                //itemText.text = "";
            }
            else
            {
                itemCircle.fillAmount = 1f;
                itemResetButton.gameObject.SetActive(true);
                ItemProto itemProto = LDB.items.Select(itemId);
                if (itemProto != null)
                {
                    itemImage.sprite = itemProto.iconSprite;
                    //itemText.text = itemProto.name;
                }
            }


            string planetName = "";
            string stationName = "";

            planetResetButton.gameObject.SetActive(false);
            if (targetStarId != 0)
            {
                planetName = GameMain.galaxy.StarById(targetStarId).displayName + "空格行星系".Translate();
                planetResetButton.gameObject.SetActive(true);
            }
            else if (targetPlanetId > 0)
            {
                planetResetButton.gameObject.SetActive(true);
                planetName = GameMain.galaxy.PlanetById(targetPlanetId).displayName;
            }
            else
            {
                if (targetItemId != 0)
                {
                    planetName = LDB.items.Select(targetItemId).name;
                }
                else
                {
                    planetName = "All Traffic Log";
                }
                planetResetButton.gameObject.SetActive(false);
            }
            if (targetStationGid != 0)
            {
                GalacticTransport galacticTransport = UIRoot.instance.uiGame.gameData.galacticTransport;
                StationComponent station = galacticTransport.stationPool[targetStationGid];
                if (station.gid == targetStationGid)
                {
                    stationName = string.IsNullOrEmpty(station.name) ? (station.isStellar ? ("星际站点号".Translate() + station.gid.ToString()) : ("本地站点号".Translate() + station.id.ToString())) : station.name;
                    planetName = "(" + GameMain.galaxy.PlanetById(station.planetId).displayName + ")";
                    stationResetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                stationResetButton.gameObject.SetActive(false);
            }

            if (planetText != null)
            {
                planetText.text = planetName;
                RectTransform rect = (RectTransform)planetResetButton.gameObject.transform;
                rect.anchoredPosition = new Vector2(planetText.preferredWidth / 2 + 18f, rect.anchoredPosition.y);
            }
            if (stationText != null)
            {
                stationText.text = stationName;
                RectTransform rect = (RectTransform)stationResetButton.gameObject.transform;
                rect.anchoredPosition = new Vector2(stationText.preferredWidth / 2 + 18f, rect.anchoredPosition.y);
            }
        }

        public void SetUpItemList()
        {
            entryCount = 0;
            countText.text = "";
            newest = 0f;
            oldest = Time.realtimeSinceStartup;

            _logList.Clear();
            listView.Clear();
            int displayMax = LSTM.trafficLogDisplayMax.Value;
            foreach (TrafficLogData item in TrafficLog.AllTrafficLogData()) 
            { 
                if (item == null)
                {
                    break;
                }
                if (sholdShowLogData(item) && displayMax >= entryCount)
                {
                    entryCount++;
                    if (newest < item.realtimeSinceStartup)
                    {
                        newest = item.realtimeSinceStartup;
                    }
                    if (oldest > item.realtimeSinceStartup)
                    {
                        oldest = item.realtimeSinceStartup;
                    }
                    item.fetchedTime = item.time + "(" + entryCount + ")";
                    _logList.Add(item);
                }
                if (entryCount > entryMax)
                {
                    break;
                }
            }
            listView.SetItemCount(entryCount);

            RefleshCountText();
        }

        public void RefleshCountText()
        {
            string tpmString = "";
            int dosplayCount = entryCount;
            int displayMax = LSTM.trafficLogDisplayMax.Value;

            if (dosplayCount > 1)
            {
                float duration = newest - oldest;
                if (duration > 30)
                {
                    float tpm = (float)dosplayCount / (duration / 60f);
                    tpmString = "    (" + tpm.ToString("F1") + " per min)";
                }

            }
            if (displayMax < dosplayCount)
            {
                tpmString = "+" + tpmString;
                dosplayCount--;
            }
            countText.text = "Result: " + dosplayCount.ToString() + tpmString;
        }

        internal bool RefreshListView(UIListView listView, bool onlyNewlyEmerged = false)
        {
            return true;
        }

        private void OnLSTMButtonClick(int obj)
        {
            LSTM.OpenBalanceWindow(targetItemId, targetPlanetId, targetStarId);
        }

        private void OnReloadButtonClick(int obj)
        {
            _eventLock = true;
            SetUpItemList();
            _eventLock = false;
        }

        private void OnStarResetButtonClick(int obj)
        {
            targetStarId = 0;
            targetPlanetId = 0;
            targetStationGid = 0;
            SetUpData();
        }

        private void OnPlanetResetButtonClick(int obj)
        {
            targetStarId = 0;
            targetPlanetId = 0;
            targetStationGid = 0;
            SetUpData();
        }


        private void OnStationResetButtonClick(int obj)
        {
            targetStationGid = 0;
            SetUpData();
        }

        private void OnItemResetButtonClick(int obj)
        {
            targetItemId = 0;
            targetStationGid = 0;
            SetUpData();
        }

        private void OnSelectItemButtonClick(int obj)
        {
            if (UIItemPicker.isOpened)
            {
                UIItemPicker.Close();
                return;
            }
            UIItemPicker.Popup(windowTrans.anchoredPosition + new Vector2(-220f, 180f), new Action<ItemProto>(this.OnItemPickerReturn));
        }

        private void OnItemPickerReturn(ItemProto itemProto)
        {
            if (itemProto == null)
            {
                return;
            }

            targetItemId = itemProto.ID;
            targetStationGid = 0;
            SetUpData();
        }

        //menu
        public UIComboBox menuComboBox;
        UILogListItem menuTarget;
        public enum EMenuCommand
        {
            LocateDemand = 0,
            LocateSupply,
        }

        public void ShowMenu(UILogListItem item)
        {
            if (menuComboBox.isDroppedDown)
            {
                menuComboBox.isDroppedDown = false;
                return;
            }

            RectTransform rect = menuComboBox.m_DropDownList;

            UIRoot.ScreenPointIntoRect(Input.mousePosition, rect.parent as RectTransform, out Vector2 pos);
            pos.x = pos.x + 20f;
            pos.y = pos.y + 30f;
            menuComboBox.m_DropDownList.anchoredPosition = pos;

            menuTarget = item;
            //menuTarget.LockAppearance();
            RefreshMenuBox(item);
            if (menuComboBox.DropDownCount > 0)
            {
                menuComboBox.OnPopButtonClick();
            }
        }

        internal void RefreshMenuBox(UILogListItem item)
        {
            List<string> items = menuComboBox.Items;
            List<int> itemsData = menuComboBox.ItemsData;
            items.Clear();
            itemsData.Clear();
            int itemCount = 0;
            if (item == null)
            {
                menuComboBox.DropDownCount = 0;
                return;
            }

            {
                items.Add("Locate Demand Station");
                itemsData.Add((int)EMenuCommand.LocateDemand);
                itemCount++;
                items.Add("Locate Supply Station");
                itemsData.Add((int)EMenuCommand.LocateSupply);
                itemCount++;
            }


            //if ()
            //{
            //    items.Add("label");
            //    itemsData.Add((int)EMenuCommand.);
            //    itemCount++;
            //}

            menuComboBox.DropDownCount = itemCount;

        }


        public void OnMenuBoxItemIndexChange()
        {
            if (_eventLock)
            {
                return;
            }
            int num = menuComboBox.itemIndex;
            if (num < 0) //recursion
            {
                return;
            }
            if (menuTarget != null)
            {
                EMenuCommand itemData = (EMenuCommand)menuComboBox.ItemsData[num];
                switch (itemData)
                {
                    case EMenuCommand.LocateDemand:
                        {
                            StationComponent sc = menuTarget.DemandStation();
                            if (sc != null)
                            {
                                LSTM.LocateStation(sc, sc.planetId);
                            }
                        }
                        break;
                    case EMenuCommand.LocateSupply:
                        {
                            StationComponent sc = menuTarget.SupplyStation();
                            if (true)
                            {
                                if (sc != null)
                                {
                                    LSTM.LocateStation(sc, sc.planetId);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                //menuTarget.UnlockAppearance();
                menuTarget = null;
            }

            menuComboBox.itemIndex = -1; //recursion
        }

        internal void CreateMenuBox()
        {
            // Main Button : Image,Button
            // -Pop sign : Image
            // -Text : Text
            // Dropdown List ScrollBox : ScrollRect

            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIComboBox src = statisticsWindow.productAstroBox;
            UIComboBox box = GameObject.Instantiate<UIComboBox>(src, windowTrans);
            box.gameObject.name = "menu-box";

            RectTransform boxRect = Util.NormalizeRectWithTopLeft(box, 20f, 20f, windowTrans);

            RectTransform btnRect = box.transform.Find("Main Button")?.transform as RectTransform;
            if (btnRect != null)
            {
                btnRect.pivot = new Vector2(1f, 0f);
                btnRect.anchorMax = Vector2.zero;
                btnRect.anchorMin = Vector2.zero;
                btnRect.anchoredPosition = new Vector2(boxRect.sizeDelta.x, 0f);
                btnRect.sizeDelta = new Vector2(20, boxRect.sizeDelta.y);

                Button btn = btnRect.GetComponent<Button>();
                btnRect.Find("Text")?.gameObject.SetActive(false);
                btnRect.gameObject.SetActive(false);
            }

            box.onItemIndexChange.AddListener(OnMenuBoxItemIndexChange);
            menuComboBox = box;

            //Dropdown List ScrollBox
            RectTransform vsRect = menuComboBox.m_Scrollbar.transform as RectTransform;
            vsRect.sizeDelta = new Vector2(0, vsRect.sizeDelta.y);

            RefreshMenuBox(null);
        }
    }
}
