﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SuperScrollView
{

    public class TopToBottomDemoScript : MonoBehaviour
    {
        public LoopListView2 mLoopListView;
        Button mScrollToButton;
        Button mAddItemButton;
        Button mSetCountButton;
        InputField mScrollToInput;
        InputField mAddItemInput;
        InputField mSetCountInput;
        Button mBackButton;

        // Use this for initialization
        void Start()
        {
            mLoopListView.InitListView(30, OnGetItemByIndex);

            mSetCountButton = GameObject.Find("ButtonPanel/buttonGroup1/SetCountButton").GetComponent<Button>();
            mScrollToButton = GameObject.Find("ButtonPanel/buttonGroup2/ScrollToButton").GetComponent<Button>();
            mAddItemButton = GameObject.Find("ButtonPanel/buttonGroup3/AddItemButton").GetComponent<Button>();
            mSetCountInput = GameObject.Find("ButtonPanel/buttonGroup1/SetCountInputField").GetComponent<InputField>();
            mScrollToInput = GameObject.Find("ButtonPanel/buttonGroup2/ScrollToInputField").GetComponent<InputField>();
            mAddItemInput = GameObject.Find("ButtonPanel/buttonGroup3/AddItemInputField").GetComponent<InputField>();
            mScrollToButton.onClick.AddListener(OnJumpBtnClicked);
            mAddItemButton.onClick.AddListener(OnAddItemBtnClicked);
            mSetCountButton.onClick.AddListener(OnSetItemCountBtnClicked);
            mBackButton = GameObject.Find("ButtonPanel/BackButton").GetComponent<Button>();
            mBackButton.onClick.AddListener(OnBackBtnClicked);



            mLoopListView.SetListItemCountNew(30);

            //mLoopListView.MovePanelToItemIndex(10000-1, 0);
            //mLoopListView.ForceUpdate();
    
        }

        void OnBackBtnClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }


        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (index < 0 || index >= DataSourceMgr.Get.TotalItemCount)
            {
                return null;
            }

            ItemData itemData = DataSourceMgr.Get.GetItemDataByIndex(index);
            if(itemData == null)
            {
                return null;
            }
            //get a new item. Every item can use a different prefab, the parameter of the NewListViewItem is the prefab’name. 
            //And all the prefabs should be listed in ItemPrefabList in LoopListView2 Inspector Setting
            LoopListViewItem2 item = listView.NewListViewItem("ItemPrefab1");
            ListItem2 itemScript = item.GetComponent<ListItem2>();
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                itemScript.Init();
            }
            itemScript.SetItemData(itemData,index);
            return item;
        }

        void OnJumpBtnClicked()
        {
            int itemIndex = 0;
            if (int.TryParse(mScrollToInput.text, out itemIndex) == false)
            {
                return;
            }
            //mLoopListView.MovePanelToItemIndex(itemIndex, 0);
            mLoopListView.SetListItemCountNew(itemIndex);
            //mLoopListView.RefreshAllShownItemNew();

            mLoopListView.MovePanelToItemIndex(itemIndex - 1, 0);
        }

        void OnAddItemBtnClicked()
        {
            if (mLoopListView.ItemTotalCount < 0)
            {
                return;
            }
            int count = 0;
            if (int.TryParse(mAddItemInput.text, out count) == false)
            {
                return;
            }
            count = mLoopListView.ItemTotalCount + count;
            if (count < 0 || count > DataSourceMgr.Get.TotalItemCount)
            {
                return;
            }
            mLoopListView.SetListItemCount(count, false);
        }

        void OnSetItemCountBtnClicked()
        {
            int count = 0;
            if (int.TryParse(mSetCountInput.text, out count) == false)
            {
                return;
            }
            //if (count < 0 || count > DataSourceMgr.Get.TotalItemCount)
            //{
            //    return;
            //}
            mLoopListView.SetListItemCountNew(count);
            //mLoopListView.RefreshAllShownItemNew();

            mLoopListView.MovePanelToItemIndex(count - 1, 0);
            //mLoopListView.ForceUpdate();
        }

    }

}
