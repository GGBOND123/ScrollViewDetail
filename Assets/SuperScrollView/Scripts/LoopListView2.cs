using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SuperScrollView
{

    public class ItemPool
    {
        GameObject mPrefabObj;
        string mPrefabName;
        int mInitCreateCount = 1;
        float mPadding = 0;
        float mStartPosOffset = 0;
        bool mAnimationCancle = false;
        string mAnimatorPath = string.Empty;

        /// <summary>
        ///  每个Item对应的 LoopListViewItem2 的池子List，LoopListView2 的 RecycleItem(LoopListViewItem2 item) 方法会将参数Item入池
        /// </summary>
        List<LoopListViewItem2> mTmpPooledItemList = new List<LoopListViewItem2>();
        List<LoopListViewItem2> mPooledItemList = new List<LoopListViewItem2>();
        /// <summary>
        /// NewListViewItem()时，从池子中GetItem()的累加次数。
        /// </summary>
        static int  mCurItemIdCount = 0;
        /// <summary>
        /// 滚轮的congtent
        /// </summary>
        RectTransform mItemParent = null;
        public ItemPool()
        {
           
        }
        public void Init(ItemPrefabConfData data, RectTransform parent)
        {
            mPrefabObj = data.mItemPrefab;
            mPrefabName = mPrefabObj.name;
            mInitCreateCount = data.mInitCreateCount;
            mPadding = data.mPadding;
            mStartPosOffset = data.mStartPosOffset;
            mItemParent = parent;
            mAnimationCancle = data.mAnimationCancle;
            mAnimatorPath = data.mAnimatorPath;
            mPrefabObj.SetActive(false);
            for (int i = 0; i < mInitCreateCount; ++i)
            {
                LoopListViewItem2 tViewItem = CreateItem();
                RecycleItemReal(tViewItem);
            }
        }

        /// <summary>
        /// 先从mTmpPooledItemList池中拿，再从mPooledItemList池中拿
        /// </summary>
        /// <returns></returns>
        public LoopListViewItem2 GetItem()
        {
            mCurItemIdCount++;
            LoopListViewItem2 tItem = null;
            if(mTmpPooledItemList.Count > 0)
            {
                int count = mTmpPooledItemList.Count;
                tItem = mTmpPooledItemList[count - 1];
                mTmpPooledItemList.RemoveAt(count - 1);
                tItem.gameObject.SetActive(true);
            }
            else
            {
                int count = mPooledItemList.Count;
                if (count == 0)
                {
                    tItem = CreateItem();
                }
                else
                {
                    tItem = mPooledItemList[count - 1];
                    mPooledItemList.RemoveAt(count - 1);
                    tItem.gameObject.SetActive(true);
                }
            }
            tItem.Padding = mPadding;
            tItem.ItemId = mCurItemIdCount;
            return tItem;

        }

        public void DestroyAllItem()
        {
            ClearTmpRecycledItem();
            int count = mPooledItemList.Count;
            for (int i = 0;i<count;++i)
            {
                GameObject.Destroy(mPooledItemList[i].gameObject);
            }
            mPooledItemList.Clear();
        }
        /// <summary>
        /// Instantiate预制，并且重置其状态，绑上LoopListViewItem2
        /// </summary>
        /// <returns></returns>
        public LoopListViewItem2 CreateItem()
        {

            GameObject go = GameObject.Instantiate<GameObject>(mPrefabObj, Vector3.zero,Quaternion.identity, mItemParent);
            go.SetActive(true);
            RectTransform rf = go.GetComponent<RectTransform>();
            rf.localScale = Vector3.one;
            rf.localPosition = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            LoopListViewItem2 tViewItem = go.GetComponent<LoopListViewItem2>();
            tViewItem.ItemPrefabName = mPrefabName;
            tViewItem.StartPosOffset = mStartPosOffset;
            tViewItem.ItemAnimatorPath = mAnimatorPath;
            tViewItem.ItemCancleAnimation = mAnimationCancle;
            return tViewItem;
        }
        void RecycleItemReal(LoopListViewItem2 item)
        {
            item.gameObject.SetActive(false);
            mPooledItemList.Add(item);
        }

        /// <summary>
        /// 当前预制对应的 ItemPool 类中，回收该 Item 的 LoopListViewItem2 脚本到临时的数组中：mTmpPooledItemList
        /// </summary>
        /// <param name="item"></param>
        public void RecycleItem(LoopListViewItem2 item)
        {
            mTmpPooledItemList.Add(item);
        }

        /// <summary>
        /// 当前预制对应的 ItemPool 类中，针对其 mTmpPooledItemList 中的LoopListViewItem2，处理并添加到 mPooledItemList
        /// </summary>
        public void ClearTmpRecycledItem()
        {
            int count = mTmpPooledItemList.Count;
            if(count == 0)
                return;
            
            for(int i = 0;i<count;++i)
                RecycleItemReal(mTmpPooledItemList[i]);
            mTmpPooledItemList.Clear();
        }
    }
    [System.Serializable]
    public class ItemPrefabConfData
    {
        public GameObject mItemPrefab = null;
        /// <summary>
        /// 上一个Item底，距离下一个Item顶的距离
        /// </summary>
        public float mPadding = 0;
        /// <summary>
        /// 初始化数量
        /// </summary>
        public int mInitCreateCount = 0;
        /// <summary>
        /// Item的Rect根据不同的ArrangeType，距离边缘的偏移值
        /// </summary>
        public float mStartPosOffset = 0;   
        public float mItemHeight = 0;           //??dw
        public bool mAnimationCancle = false;
        public string mAnimatorPath = string.Empty;
    }


    public class LoopListViewInitParam
    {
        /// <summary>
        /// 当项从视口中滚动超出此距离时会被回收。
        /// </summary>
        public float mDistanceForRecycle0 = 300; //mDistanceForRecycle0 should be larger than mDistanceForNew0
        /// <summary>
        /// 当视口内需要显示新的项时，达到此距离会触发加载。
        /// </summary>
        public float mDistanceForNew0 = 200;
        public float mDistanceForRecycle1 = 300;//mDistanceForRecycle1 should be larger than mDistanceForNew1
        public float mDistanceForNew1 = 200;
        /// <summary>
        /// 平滑滚动的速率。
        /// </summary>
        public float mSmoothDumpRate = 0.3f;
        public float mSnapFinishThreshold = 0.01f;
        public float mSnapVecThreshold = 145;
        public float mItemDefaultWithPaddingSize = 20;

        public static LoopListViewInitParam CopyDefaultInitParam()
        {
            return new LoopListViewInitParam();
        }
    }    

    public class LoopListView2 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
    {

        class SnapData
        {
            public SnapStatus mSnapStatus = SnapStatus.NoTargetSet;
            public int mSnapTargetIndex = 0;
            public float mTargetSnapVal = 0;
            public float mCurSnapVal = 0;
            public bool mIsForceSnapTo = false;
            public void Clear()
            {
                mSnapStatus = SnapStatus.NoTargetSet;
                mIsForceSnapTo = false;
            }
        }
        /// <summary>
        /// <prefabName, 绑定的预制对应的对象池>，初始化时添加
        /// </summary>
        Dictionary<string, ItemPool> mItemPoolDict = new Dictionary<string, ItemPool>();
        /// <summary>
        /// 所有绑定的预制的对象池List，初始化时添加
        /// </summary>
        List<ItemPool> mItemPoolList = new List<ItemPool>();

        /// <summary>
        /// 节点界面上拖过去的Item预制
        /// </summary>
        [SerializeField]
        List<ItemPrefabConfData> mItemPrefabDataList = new List<ItemPrefabConfData>();

        /// <summary>
        /// 节点界面上的列表生成方向
        /// </summary>
        [SerializeField]
        private ListItemArrangeType mArrangeType = ListItemArrangeType.TopToBottom;
        public ListItemArrangeType ArrangeType { get { return mArrangeType; } set { mArrangeType = value; } }

        [SerializeField]
        bool mSupportScrollBar = true;
        [SerializeField]
        bool mItemInterruptAnim = false;


        /// <summary>
        /// 该属性定义 Item 自身的吸附枢轴点，采用 归一化比例坐标：(0,0) 表示 Item 的左下角。(1,1) 表示 Item 的右上角。(0.5, 0.5) 表示吸附在 Item 的中心。
        /// </summary>
        [SerializeField]
        Vector2 mItemSnapPivot = Vector2.zero;

        /// <summary>
        /// 该属性定义 ScrollRect Viewport 的吸附枢轴点，决定 Item 吸附时在 Viewport 中的位置。(0,0) 表示 Viewport 的左下角。(1,1) 表示 Viewport 的右上角。
        /// </summary>
        [SerializeField]
        Vector2 mViewPortSnapPivot = Vector2.zero;



        /// <summary>
        /// 屏幕上待显示的所有 LoopListViewItem2。  扩展方式：在UpdateListView中，每次while循环都会判断数组第一个or最后一个是否在显示上下范围内，从而进行增减。
        /// </summary>
        List<LoopListViewItem2> mItemList = new List<LoopListViewItem2>();
        /// <summary>
        /// 滚轮的content Trans
        /// </summary>
        RectTransform mContainerTrans;
        ScrollRect mScrollRect = null;
        /// <summary>
        /// 滚轮的Trans
        /// </summary>
        RectTransform mScrollRectTransform = null;
        /// <summary>
        /// 滚轮的viewport Trans
        /// </summary>
        RectTransform mViewPortRectTransform = null;
        float mItemDefaultWithPaddingSize = 20;
        /// <summary>
        /// SetListItemCount中设置Item的数据总数。  值为 -1：表示 Item 数量是无限的，此时不支持滚动条（Scrollbar）。ItemIndex 的范围从 负无穷到正无穷。值为 >= 0：表示 Item 数量是有限的，ItemIndex 的范围从 0 到 itemTotalCount - 1。
        /// </summary>
        int mItemTotalCount = 0;
        bool mIsVertList = false;
        Func<LoopListView2, int, LoopListViewItem2> mOnGetItemByIndex;
        Func<LoopListView2, int, string> mOnGetItemNameByIndex;

        /// <summary>
        /// 单个Item获取四个边角的世界坐标，多地使用
        /// </summary>
        Vector3[] mItemWorldCorners = new Vector3[4];

        /// <summary>
        /// Viewport的四个边角的局部坐标
        /// </summary>
        Vector3[] mViewPortRectLocalCorners = new Vector3[4];

        /// <summary>
        /// UpdateListView()中，mItemList数组尾部增加时，上一次While循环中的增加的尾部数据Index，最小的数据Index。
        /// </summary>
        int mCurReadyMinItemIndex = 0;
        /// <summary>
        /// UpdateListView()中，mItemList数组头部增加时，上一次While循环中的增加的头部数据Index，最大的数据Index。
        /// </summary>
        int mCurReadyMaxItemIndex = 0;
        
        bool mNeedCheckNextMinItem = true;
        bool mNeedCheckNextMaxItem = true;
        ItemPosMgr mItemPosMgr = null;
        float mDistanceForRecycle0 = 300;
        float mDistanceForNew0 = 200;
        float mDistanceForRecycle1 = 300;
        float mDistanceForNew1 = 200;

        bool mIsDraging = false;
        PointerEventData mPointerEventData = null;
        public System.Action<PointerEventData> mOnBeginDragAction = null;
        public System.Action<PointerEventData> mOnDragingAction = null;
        public System.Action<PointerEventData> mOnEndDragAction = null;
        public System.Action mOnListClickAction = null;

        public void SetOnBeginDragAction(System.Action<PointerEventData> callback)
        {
            mOnBeginDragAction = callback;
        }

        public void SetOnDragingAction(System.Action<PointerEventData> callback)
        {
            mOnDragingAction = callback;
        }

        public void SetOnEndDragAction(System.Action<PointerEventData> callback)
        {
            mOnEndDragAction = callback;
        }

        public void SetOnListClickAction(System.Action callback)
        {
            mOnListClickAction = callback;
        }
        
        public void SetOnSnapItemFinished(System.Action<LoopListView2, LoopListViewItem2> callback)
        {
            mOnSnapItemFinished = callback;
        }
        
        public void SetOnSnapNearestChanged(System.Action<LoopListView2, LoopListViewItem2> callback)
        {
            mOnSnapNearestChanged = callback;
        }

        /// <summary>
        /// 当某个 Item 进入 ScrollRect 的 Viewport时，系统会调用该回调函数.
        /// </summary>
        public System.Func<LoopListView2, int, LoopListViewItem2> OnGetItemByIndex
        {
            set => mOnGetItemByIndex = value;
        }

        public System.Func<LoopListView2, int, string> OnGetItemNameByIndex
        {
            set => mOnGetItemNameByIndex = value;
        }

        /// <summary>
        /// SetItemSize()更新高度时，设置最新设置的那个Item的Index
        /// </summary>
        int mLastItemIndex = 0;
        /// <summary>
        /// SetItemSize()更新高度时，设置最新设置的那个Item的Padding
        /// </summary>
        float mLastItemPadding = 0;
        float mSmoothDumpVel = 0;
        float mSmoothDumpRate = 0.3f;
        float mSnapFinishThreshold = 0.1f;
        float mSnapVecThreshold = 145;


        /// <summary>
        /// 每帧更新Content的localPosition
        /// </summary>
        Vector3 mLastFrameContainerPos = Vector3.zero;
        public System.Action<LoopListView2,LoopListViewItem2> mOnSnapItemFinished = null;
        public System.Action<LoopListView2, LoopListViewItem2> mOnSnapNearestChanged = null;
        int mCurSnapNearestItemIndex = -1;
        /// <summary>
        /// 调用UpdateAllShownItemsPos()时，当前帧Content的localPosition  和 上一帧Content的localPosition 的差值。
        /// </summary>
        Vector2 mAdjustedVec;
        /// <summary>
        /// 是否需要在Update中调节滑动速度。  在拖动状态下，UpdateAllShownItemsPos 函数会刷新该值
        /// </summary>
        bool mNeedAdjustVec = false;

        /// <summary>
        /// 是否需要额外调一次刷新snap的数值
        /// </summary>
        int mLeftSnapUpdateExtraCount = 1;

        ClickEventListener mScrollBarClickEventListener = null;

        /// <summary>
        /// Update中，当前帧Content的localPosition  和 上一帧Content的localPosition 的差值。
        /// </summary>
        Vector3 mLastSnapCheckPos = Vector3.zero;
        int mListUpdateCheckFrameCount = 0;
        public bool IsVertList
        {
            get
            {
                return mIsVertList;
            }
        }
        public int ItemTotalCount
        {
            get
            {
                return mItemTotalCount;
            }
        }

        public RectTransform ContainerTrans
        {
            get
            {
                return mContainerTrans;
            }
        }

        public ScrollRect ScrollRect
        {
            get
            {
                return mScrollRect;
            }
        }

        public bool IsDraging
        {
            get
            {
                return mIsDraging;
            }
        }


        public bool SupportScrollBar
        {
            get { return mSupportScrollBar; }
            set { mSupportScrollBar = value; }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.LogErrorFormat("111 {0}", eventData);
            mOnListClickAction?.Invoke();
        }

        public ItemPrefabConfData GetItemPrefabConfData(string prefabName)
        {
            foreach (ItemPrefabConfData data in mItemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }
                if (prefabName == data.mItemPrefab.name)
                {
                    return data;
                }

            }
            return null;
        }

        public void OnItemPrefabChanged(string prefabName)
        {
            ItemPrefabConfData data = GetItemPrefabConfData(prefabName);
            if(data == null)
            {
                return;
            }
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(prefabName, out pool) == false)
            {
                return;
            }
            int firstItemIndex = -1;
            Vector3 pos = Vector3.zero;
            if(mItemList.Count > 0)
            {
                firstItemIndex = mItemList[0].ItemIndex;
                pos = mItemList[0].CachedRectTransform.localPosition;
            }
            RecycleAllItem();
            ClearAllTmpRecycledItem();
            pool.DestroyAllItem();
            pool.Init(data, mContainerTrans);
            if(firstItemIndex >= 0)
            {
                RefreshAllShownItemWithFirstIndexAndPos(firstItemIndex, pos);
            }
        }


        void SetScrollbarListener()
        {
            mScrollBarClickEventListener = null;
            Scrollbar curScrollBar = null;
            if (mIsVertList && mScrollRect.verticalScrollbar != null)
            {
                curScrollBar = mScrollRect.verticalScrollbar;

            }
            if (!mIsVertList && mScrollRect.horizontalScrollbar != null)
            {
                curScrollBar = mScrollRect.horizontalScrollbar;
            }
            if(curScrollBar == null)
            {
                return;
            }
            ClickEventListener listener = ClickEventListener.Get(curScrollBar.gameObject);
            mScrollBarClickEventListener = listener;
            listener.SetPointerUpHandler(OnPointerUpInScrollBar);
            listener.SetPointerDownHandler(OnPointerDownInScrollBar);
        }

        void OnPointerDownInScrollBar(GameObject obj)
        {
        }

        void OnPointerUpInScrollBar(GameObject obj)
        {
        }

        public void ResetListView()
        {
            mViewPortRectTransform.GetLocalCorners(mViewPortRectLocalCorners);
            mContainerTrans.localPosition = Vector3.zero;
        }


      

        //To get the visible item by itemIndex. If the item is not visible, then this method return null.
        public LoopListViewItem2 GetShownItemByItemIndex(int itemIndex)
        {
            int count = mItemList.Count;
            if (count == 0)
            {
                return null;
            }
            if (itemIndex < mItemList[0].ItemIndex || itemIndex > mItemList[count - 1].ItemIndex)
            {
                return null;
            }
            int i = itemIndex - mItemList[0].ItemIndex;
            return mItemList[i];
        }
       

        public int ShownItemCount
        {
            get
            {
                return mItemList.Count;
            }
        }

        /// <summary>
        /// ViewPort的布局方式是锚点四散开来的，top、bottom类似的布局，所以这里的高度和宽度为世界空间的距离
        /// </summary>
        public float ViewPortSize
        {
            get
            {
                if (mIsVertList)
                {
                    return mViewPortRectTransform.rect.height;
                }
                else
                {
                    return mViewPortRectTransform.rect.width;
                }
            }
        }

        public float ViewPortWidth
        {
            get { return mViewPortRectTransform.rect.width; }
        }
        public float ViewPortHeight
        {
            get { return mViewPortRectTransform.rect.height; }
        }


        /*
         All visible items is stored in a List<LoopListViewItem2> , which is named mItemList;
         this method is to get the visible item by the index in visible items list. The parameter index is from 0 to mItemList.Count.
        */
        public LoopListViewItem2 GetShownItemByIndex(int index)
        {
            int count = mItemList.Count;
            if(index < 0 || index >= count)
            {
                return null;
            }
            return mItemList[index];
        }

        public LoopListViewItem2 GetShownItemByIndexWithoutCheck(int index)
        {
            return mItemList[index];
        }

        public int GetIndexInShownItemList(LoopListViewItem2 item)
        {
            if(item == null)
            {
                return -1;
            }
            int count = mItemList.Count;
            if (count == 0)
            {
                return -1;
            }
            for (int i = 0; i < count; ++i)
            {
                if (mItemList[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }


        public void DoActionForEachShownItem(System.Action<LoopListViewItem2,object> action,object param)
        {
            if(action == null)
            {
                return;
            }
            int count = mItemList.Count;
            if(count == 0)
            {
                return;
            }
            for (int i = 0; i < count; ++i)
            {
                action(mItemList[i],param);
            }
        }


        public LoopListViewItem2 NewListViewItem(string itemPrefabName)
        {
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(itemPrefabName, out pool) == false)
            {
                return null;
            }
            LoopListViewItem2 item = pool.GetItem();
            RectTransform rf = item.GetComponent<RectTransform>();
            rf.SetParent(mContainerTrans);
            rf.localScale = Vector3.one;
            rf.localPosition = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            item.ParentListView = this;
            return item;
        }

        /*
        For a vertical scrollrect, when a visible item’s height changed at runtime, then this method should be called to let the LoopListView2 component reposition all visible items’ position.
        For a horizontal scrollrect, when a visible item’s width changed at runtime, then this method should be called to let the LoopListView2 component reposition all visible items’ position.
        */
        public void OnItemSizeChanged(int itemIndex)
        {
            LoopListViewItem2 item = GetShownItemByItemIndex(itemIndex);
            if (item == null)
            {
                return;
            }
            if (mSupportScrollBar)
            {
                if (mIsVertList)
                {
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.height, item.Padding);
                }
                else
                {
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.width, item.Padding);
                }
            }
            UpdateContentSize();
            UpdateAllShownItemsPos();
        }


        /*
        To update a item by itemIndex.if the itemIndex-th item is not visible, then this method will do nothing.
        Otherwise this method will first call onGetItemByIndex(itemIndex) to get a updated item and then reposition all visible items'position. 
        */
        public void RefreshItemByItemIndex(int itemIndex)
        {
            int count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            if (itemIndex < mItemList[0].ItemIndex || itemIndex > mItemList[count - 1].ItemIndex)
            {
                return;
            }
            int firstItemIndex = mItemList[0].ItemIndex;
            int i = itemIndex - firstItemIndex;
            LoopListViewItem2 curItem = mItemList[i];
            Vector3 pos = curItem.CachedRectTransform.localPosition;
            RecycleItemTmp(curItem);
            LoopListViewItem2 newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                RefreshAllShownItemWithFirstIndex(firstItemIndex);
                return;
            }
            mItemList[i] = newItem;
            if(mIsVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }
            newItem.CachedRectTransform.localPosition = pos;
            OnItemSizeChanged(itemIndex);
            ClearAllTmpRecycledItem();
        }

        public void FinishSnapImmediately()
        {
        }

        /// <summary>
        /// 获取当前显示的所有index列表
        /// </summary>
        /// <returns></returns>
        public List<int> GetCurShowIndexes()
        {
            if (null == mItemList || mItemList.Count == 0)
                return null;
            List<int> list = new List<int>(4);
            for(int i = 0; i < mItemList.Count; ++i)
            {
                if(null != mItemList[i])
                {
                    list.Add(mItemList[i].ItemIndex);
                }
            }
            return list;
        }

        /// <summary>
        /// 获取滚动到指定行的content的localposition
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Vector3 GetMovePanelToIndexPos(int itemIndex, float offset)
        {
            Vector3 pos = mContainerTrans.localPosition;
            if (null == mOnGetItemNameByIndex)
                return pos;
            
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                //暂时我只用到这个 后面的有用到再加吧。没考虑多个rowTemplate的问题 慎用
                if (itemIndex == 0 || mItemTotalCount == 0)
                {
                    pos = new Vector3(pos.x, 0, 0);
                    return pos;
                }
                if(mItemTotalCount > 0 && itemIndex >= mItemTotalCount)
                {
                    itemIndex = mItemTotalCount - 1;
                }
                
                float y = 0; string newItemName = string.Empty;
                y = -offset;
                ItemPrefabConfData data = null;
                for(int i = 0; i < itemIndex; ++ i)
                {
                    newItemName = mOnGetItemNameByIndex(this, itemIndex);
                    if (!string.IsNullOrEmpty(newItemName))
                    {
                        for (int j = 0; j < mItemPrefabDataList.Count; ++j)
                        {
                            if (null != mItemPrefabDataList[j] && mItemPrefabDataList[j].mItemPrefab.name.Equals(newItemName))
                            {
                                data = mItemPrefabDataList[j];
                                break;
                            }
                        }
                        if(data.mItemHeight <= 0)
                        {
                            RectTransform rectTrans = data.mItemPrefab.GetComponent<RectTransform>();
                            y += rectTrans.rect.height;
                        }
                        else
                        {
                            
                            y += data.mItemHeight;
                            Debug.Log("Item高度变成：" + y);
                        }
                        y += data.mPadding;
                    }

                }
                pos = new Vector3(pos.x, y, pos.z);
            }
            return pos;
        }

       


        public void RefreshAllShownItemWithFirstIndexAndPos(int firstItemIndex,Vector3 pos)
        {
            RecycleAllItem();
            LoopListViewItem2 newItem = GetNewItemByIndex(firstItemIndex);
            if (newItem == null)
            {
                return;
            }
            if (mIsVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }
            newItem.CachedRectTransform.localPosition = pos;
            if (mSupportScrollBar)
            {
                if (mIsVertList)
                {
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                }
                else
                {
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }
            }
            mItemList.Add(newItem);
            UpdateContentSize();
            UpdateAllShownItemsPos();
            UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
            ClearAllTmpRecycledItem();
        }


        void RecycleItemTmp(LoopListViewItem2 item)
        {
            if (item == null)
                return;
            if (string.IsNullOrEmpty(item.ItemPrefabName))
                return;
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(item.ItemPrefabName, out pool) == false)
                return;
            
            pool.RecycleItem(item);

        }

        /// <summary>
        /// 回收所有ItemPool
        /// </summary>
        void ClearAllTmpRecycledItem()
        {
            int count = mItemPoolList.Count;
            for(int i = 0;i<count;++i)
                mItemPoolList[i].ClearTmpRecycledItem();
        }


        void RecycleAllItem()
        {
            foreach (LoopListViewItem2 item in mItemList)
                RecycleItemTmp(item);
            
            mItemList.Clear();
        }


        void AdjustContainerPivot(RectTransform rtf)
        {
            Vector2 pivot = rtf.pivot;
            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                pivot.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                pivot.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                pivot.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                pivot.x = 1;
            }
            rtf.pivot = pivot;
        }


        void AdjustPivot(RectTransform rtf)
        {
            Vector2 pivot = rtf.pivot;

            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                pivot.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                pivot.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                pivot.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                pivot.x = 1;
            }
            rtf.pivot = pivot;
        }

        void AdjustContainerAnchor(RectTransform rtf)
        {
            Vector2 anchorMin = rtf.anchorMin;
            Vector2 anchorMax = rtf.anchorMax;
            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }
            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }


        void AdjustAnchor(RectTransform rtf)
        {
            Vector2 anchorMin = rtf.anchorMin;
            Vector2 anchorMax = rtf.anchorMax;
            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }
            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }

        void InitItemPool()
        {
            foreach (ItemPrefabConfData data in mItemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }
                string prefabName = data.mItemPrefab.name;
                if (mItemPoolDict.ContainsKey(prefabName))
                {
                    // Debug.LogError("A item prefab with name " + prefabName + " has existed!");
                    continue;
                }
                RectTransform rtf = data.mItemPrefab.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    Debug.LogError("RectTransform component is not found in the prefab " + prefabName);
                    continue;
                }
                AdjustAnchor(rtf);
                AdjustPivot(rtf);
                LoopListViewItem2 tItem = data.mItemPrefab.GetComponent<LoopListViewItem2>();
                if (tItem == null)
                {
                    data.mItemPrefab.AddComponent<LoopListViewItem2>();
                }
                ItemPool pool = new ItemPool();
                pool.Init(data, mContainerTrans);
                mItemPoolDict.Add(prefabName, pool);
                mItemPoolList.Add(pool);
            }
        }



        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = true;
            CacheDragPointerEventData(eventData);
            if(mOnBeginDragAction != null)
            {
                mOnBeginDragAction(eventData);
            }
            if(mItemInterruptAnim)
                CanCleItemClickAnimation();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = false;
            mPointerEventData = null;
            if (mOnEndDragAction != null)
            {
                mOnEndDragAction(eventData);
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            CacheDragPointerEventData(eventData);
            if (mOnDragingAction != null)
            {
                mOnDragingAction(eventData);
            }
            
            //if(mite)
        }

        void CacheDragPointerEventData(PointerEventData eventData)
        {
            if (mPointerEventData == null)
            {
                mPointerEventData = new PointerEventData(EventSystem.current);
            }
            mPointerEventData.button = eventData.button;
            mPointerEventData.position = eventData.position;
            mPointerEventData.pointerPressRaycast = eventData.pointerPressRaycast;
            mPointerEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
        }

        LoopListViewItem2 GetNewItemByIndex(int index)
        {
            if(mSupportScrollBar && index < 0)
            {
                return null;
            }
            if(mItemTotalCount > 0 && index >= mItemTotalCount)
            {
                return null;
            }
            LoopListViewItem2 newItem = mOnGetItemByIndex(this, index);
            if (newItem == null)
            {
                return null;
            }
            newItem.ItemIndex = index;
            newItem.ItemCreatedCheckFrameCount = mListUpdateCheckFrameCount;

            return newItem;
        }

        /// <summary>
        /// 设置ItemMgr中存储的数据层中第itemIndex个Item的的高度 
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="itemSize"></param>
        /// <param name="padding"></param>
        void SetItemSize(int itemIndex, float itemSize,float padding)
        {
            mItemPosMgr.SetItemSize(itemIndex, itemSize + padding);
            if(itemIndex >= mLastItemIndex)
            {
                mLastItemIndex = itemIndex;
                mLastItemPadding = padding;
            }
        }

        void GetPlusItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
        {
            mItemPosMgr.GetItemIndexAndPosAtGivenPos(pos, ref index, ref itemPos);
        }


        /// <summary>
        /// 更新item在content中的本地Pos，数据层的高度
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        float GetItemPos(int itemIndex)
        {
            return mItemPosMgr.GetItemPos(itemIndex);
        }

        /// <summary>
        /// 计算该Item在ViewPort内的局部坐标位置
        /// </summary>
        /// <param name="item"></param>
        /// <param name="corner"></param>
        /// <returns></returns>
        public Vector3 GetItemCornerPosInViewPort(LoopListViewItem2 item, ItemCornerEnum corner = ItemCornerEnum.LeftBottom)
        {
            item.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
            return mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[(int)corner]);
        }
       

        void AdjustPanelPos()
        {
            int count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            UpdateAllShownItemsPos();
            float viewPortSize = ViewPortSize;
            float contentSize = GetContentPanelSize();
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                if (contentSize <= viewPortSize)
                {
                    //Vector3 pos = mContainerTrans.localPosition;
                    //pos.y = 0;
                    //mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);
                    
                    mItemList[0].CachedRectTransform.localPosition = new Vector3(mItemList[0].StartPosOffset,0,0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                if (topPos0.y < mViewPortRectLocalCorners[1].y)
                {
                    // Vector3 pos = mContainerTrans.localPosition;
                    // pos.y = 0;
                    // mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);

                    mItemList[0].CachedRectTransform.localPosition = new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                float d = downPos1.y - mViewPortRectLocalCorners[0].y;
                if (d > 0)
                {
                    Vector3 pos = mItemList[0].CachedRectTransform.localPosition;
                    pos.y = pos.y - d;
                    mItemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                if (contentSize <= viewPortSize)
                {
                    // Vector3 pos = mContainerTrans.localPosition;
                    // pos.y = 0;
                    // mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);
                    
                    mItemList[0].CachedRectTransform.localPosition = new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (downPos0.y > mViewPortRectLocalCorners[0].y)
                {
                    // Vector3 pos = mContainerTrans.localPosition;
                    // pos.y = 0;
                    // mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);
                    
                    mItemList[0].CachedRectTransform.localPosition = new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                float d = mViewPortRectLocalCorners[1].y - topPos1.y;
                if (d > 0)
                {
                    Vector3 pos = mItemList[0].CachedRectTransform.localPosition;
                    pos.y = pos.y + d;
                    mItemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                if (contentSize <= viewPortSize)
                {
                    // Vector3 pos = mContainerTrans.localPosition;
                    // pos.x = 0;
                    // mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);

                    mItemList[0].CachedRectTransform.localPosition = new Vector3(0,mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                if (leftPos0.x > mViewPortRectLocalCorners[1].x)
                {
                    // Vector3 pos = mContainerTrans.localPosition;
                    // pos.x = 0;
                    // mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);

                    mItemList[0].CachedRectTransform.localPosition = new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                float d = mViewPortRectLocalCorners[2].x - rightPos1.x;
                if (d > 0)
                {
                    Vector3 pos = mItemList[0].CachedRectTransform.localPosition;
                    pos.x = pos.x + d;
                    mItemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                if (contentSize <= viewPortSize)
                {
                    // Vector3 pos = mContainerTrans.localPosition;
                    // pos.x = 0;
                    // mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);

                    mItemList[0].CachedRectTransform.localPosition = new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (rightPos0.x < mViewPortRectLocalCorners[2].x)
                {
                    // Vector3 pos = mContainerTrans.localPosition;
                    // pos.x = 0;
                    // mContainerTrans.localPosition = pos;
                    SetContainerLocalPosY(0);

                    mItemList[0].CachedRectTransform.localPosition = new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                float d = leftPos1.x - mViewPortRectLocalCorners[1].x;
                if (d > 0)
                {
                    Vector3 pos = mItemList[0].CachedRectTransform.localPosition;
                    pos.x = pos.x - d;
                    mItemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }



        }

        private bool _isOccurError = false;



        void UpdateCurSnapData()
        {
          

        }
        //Clear current snap target and then the LoopScrollView2 will auto snap to the CurSnapNearestItemIndex.
        public void ClearSnapData()
        {
        }

        public void SetSnapTargetItemIndex(int itemIndex)
        {
        }

        //Get the nearest item index with the viewport snap point.
        public int CurSnapNearestItemIndex
        {
            get{ return mCurSnapNearestItemIndex; }
        }


       
        bool CanSnap()
        {
            if (mIsDraging)
            {
                return false;
            }
            if (mScrollBarClickEventListener != null)
            {
                if (mScrollBarClickEventListener.IsPressd)
                {
                    return false;
                }
            }

            if (mIsVertList)
            {
                if(mContainerTrans.rect.height <= ViewPortHeight)
                {
                    return false;
                }
            }
            else
            {
                if (mContainerTrans.rect.width <= ViewPortWidth)
                {
                    return false;
                }
            }

            float v = 0;
            if (mIsVertList)
            {
                v = Mathf.Abs(mScrollRect.velocity.y);
            }
            else
            {
                v = Mathf.Abs(mScrollRect.velocity.x);
            }
            if (v > mSnapVecThreshold)
            {
                return false;
            }
            if (v < 2)
            {
                return true;
            }
            float diff = 3;
            Vector3 pos = mContainerTrans.localPosition;
            if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                float minX = mViewPortRectLocalCorners[2].x - mContainerTrans.rect.width;
                if (pos.x < (minX - diff) || pos.x > diff)
                {
                    return false;
                }
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                float maxX = mViewPortRectLocalCorners[1].x + mContainerTrans.rect.width;
                if (pos.x > (maxX + diff) || pos.x < -diff)
                {
                    return false;
                }
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                float maxY = mViewPortRectLocalCorners[0].y + mContainerTrans.rect.height;
                if (pos.y > (maxY + diff) || pos.y < -diff)
                {
                    return false;
                }
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                float minY = mViewPortRectLocalCorners[1].y - mContainerTrans.rect.height;
                if (pos.y < (minY - diff) || pos.y > diff)
                {
                    return false;
                }
            }
            return true;
        }


        public void UpdateListView(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1)
        {
            mListUpdateCheckFrameCount++;
            //Debug.LogError(mListUpdateCheckFrameCount);
            if (mIsVertList)
            {
                bool needContinueCheck = true;
                int checkCount = 0;
                int maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if(checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView Vertical while loop " + checkCount + " times! something is wrong!");
                        break;
                    }
                    needContinueCheck = UpdateForVertList(distanceForRecycle0, distanceForRecycle1, distanceForNew0, distanceForNew1);
                }
            }
            else
            {
                bool needContinueCheck = true;
                int checkCount = 0;
                int maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView  Horizontal while loop " + checkCount + " times! something is wrong!");
                        break;
                    }
                    needContinueCheck = UpdateForHorizontalList(distanceForRecycle0, distanceForRecycle1, distanceForNew0, distanceForNew1);
                }
            }

        }


        bool UpdateForVertList(float distanceForRecycle0,float distanceForRecycle1,float distanceForNew0, float distanceForNew1)
        {
            if (mItemTotalCount == 0)
            {
                if(mItemList.Count > 0)
                {
                    RecycleAllItem();
                }
                return false;
            }





            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                int itemListCount = mItemList.Count;

                //第一个Item走这里
                if (itemListCount == 0)
                {
                    float curY = mContainerTrans.localPosition.y;
                    if (curY < 0)
                    {
                        curY = 0;
                    }
                    int index = 0;
                    float pos = -curY;
                    if (mSupportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(curY, ref index, ref pos);
                        pos = -pos;
                    }
                    LoopListViewItem2 newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();

                    return true;
                }
//                if(mScrollRectTransform.anchorMax == Vector2.one && mScrollRectTransform.anchorMin == Vector2.zero)
                    mViewPortRectTransform.GetLocalCorners(mViewPortRectLocalCorners);


                //判断显示List中第一个是否需要回收
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                //获取Item的左上、左下的世界坐标
                Vector3 topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                
                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount && downPos0.y - mViewPortRectLocalCorners[1].y > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }


                //判断显示List中最后一个是否需要回收
                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount && mViewPortRectLocalCorners[0].y - topPos1.y > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }



                //该Item在Viewport下方 distanceForNew1 范围内就显示
                if (mViewPortRectLocalCorners[0].y - downPos1.y < distanceForNew1)
                {
                    //如果切换数据位置，最后一个显示的数据Index比mCurReadyMaxItemIndex大，更新mCurReadyMaxItemIndex值
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    //判断下一个数据Index的状态
                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            //尾部新增的Item的位置设为上一个的尾部。
                            float y = tViewItem1.CachedRectTransform.localPosition.y - tViewItem1.CachedRectTransform.rect.height - tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }
                        
                    }

                }

                if (topPos0.y - mViewPortRectLocalCorners[1].y < distanceForNew0)
                {
                    if(tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            float y = tViewItem0.CachedRectTransform.localPosition.y + newItem.CachedRectTransform.rect.height + newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }
                        
                    }

                }

            }
            else
            {
                
                if (mItemList.Count == 0)
                {
                    float curY = mContainerTrans.localPosition.y;
                    if (curY > 0)
                    {
                        curY = 0;
                    }
                    int index = 0;
                    float pos = -curY;
                    if (mSupportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(-curY, ref index, ref pos);
                    }
                    LoopListViewItem2 newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();
                    return true;
                }
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);

                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && mViewPortRectLocalCorners[0].y - topPos0.y > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                     && downPos1.y - mViewPortRectLocalCorners[1].y > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                if (topPos1.y - mViewPortRectLocalCorners[1].y < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            float y = tViewItem1.CachedRectTransform.localPosition.y + tViewItem1.CachedRectTransform.rect.height + tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }
                        
                    }

                }


                if (mViewPortRectLocalCorners[0].y - downPos0.y < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mNeedCheckNextMinItem = false;
                            return false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            float y = tViewItem0.CachedRectTransform.localPosition.y - newItem.CachedRectTransform.rect.height - newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }
                        
                    }
                }


            }

            return false;

        }


        bool UpdateForHorizontalList(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1)
        {
            if (mItemTotalCount == 0)
            {
                if (mItemList.Count > 0)
                {
                    RecycleAllItem();
                }
                return false;
            }
            if (mArrangeType == ListItemArrangeType.LeftToRight)
            {

                if (mItemList.Count == 0)
                {
                    float curX = mContainerTrans.localPosition.x;
                    if (curX > 0)
                    {
                        curX = 0;
                    }
                    int index = 0;
                    float pos = -curX;
                    if (mSupportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(-curX, ref index, ref pos);
                    }
                    LoopListViewItem2 newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);

                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && mViewPortRectLocalCorners[1].x - rightPos0.x > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && leftPos1.x - mViewPortRectLocalCorners[2].x> distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }



                if (rightPos1.x - mViewPortRectLocalCorners[2].x < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            float x = tViewItem1.CachedRectTransform.localPosition.x + tViewItem1.CachedRectTransform.rect.width + tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }

                    }

                }

                if ( mViewPortRectLocalCorners[1].x - leftPos0.x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            float x = tViewItem0.CachedRectTransform.localPosition.x - newItem.CachedRectTransform.rect.width - newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }

                    }

                }

            }
            else
            {

                if (mItemList.Count == 0)
                {
                    float curX = mContainerTrans.localPosition.x;
                    if (curX < 0)
                    {
                        curX = 0;
                    }
                    int index = 0;
                    float pos = -curX;
                    if (mSupportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(curX, ref index, ref pos);
                        pos = -pos;
                    }
                    LoopListViewItem2 newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }
                LoopListViewItem2 tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);

                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && leftPos0.x - mViewPortRectLocalCorners[2].x > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                LoopListViewItem2 tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                Vector3 leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                Vector3 rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && mViewPortRectLocalCorners[1].x - rightPos1.x > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }



                if (mViewPortRectLocalCorners[1].x - leftPos1.x  < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            float x = tViewItem1.CachedRectTransform.localPosition.x - tViewItem1.CachedRectTransform.rect.width - tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }

                    }

                }

                if (rightPos0.x - mViewPortRectLocalCorners[2].x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        LoopListViewItem2 newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            float x = tViewItem0.CachedRectTransform.localPosition.x + newItem.CachedRectTransform.rect.width + newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }

                    }

                }

            }

            return false;

        }





        /// <summary>
        /// 根据Item的偏移啥的，计算Content的总Size，相当于Content的RectTransform的height 或 width
        /// </summary>
        /// <returns></returns>
        private float GetContentPanelSize()
        {
            if (mSupportScrollBar)
            {
                float tTotalSize = mItemPosMgr.mTotalSize > 0 ? (mItemPosMgr.mTotalSize - mLastItemPadding) : 0;
                if(tTotalSize < 0)
                    tTotalSize = 0;
                return tTotalSize;
            }

            int count = mItemList.Count;
            if (count == 0)
                return 0;
            if (count == 1)
                return mItemList[0].ItemSize;
            if (count == 2)
                return mItemList[0].ItemSizeWithPadding + mItemList[1].ItemSize;
            float s = 0;
            for (int i = 0; i < count - 1; ++i)
            {
                s += mItemList[i].ItemSizeWithPadding;
            }
            s += mItemList[count - 1].ItemSize;
            return s;
        }


        void CheckIfNeedUpdataItemPos()
        {
            int count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                LoopListViewItem2 firstItem = mItemList[0];
                LoopListViewItem2 lastItem = mItemList[mItemList.Count - 1];
                float viewMaxY = GetContentPanelSize();
                if (firstItem.TopY > 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.TopY != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if ((-lastItem.BottomY) > viewMaxY || (lastItem.ItemIndex == mCurReadyMaxItemIndex && (-lastItem.BottomY) != viewMaxY))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                LoopListViewItem2 firstItem = mItemList[0];
                LoopListViewItem2 lastItem = mItemList[mItemList.Count - 1];
                float viewMaxY = GetContentPanelSize();
                if (firstItem.BottomY < 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.BottomY != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if (lastItem.TopY > viewMaxY || (lastItem.ItemIndex == mCurReadyMaxItemIndex && lastItem.TopY != viewMaxY))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                LoopListViewItem2 firstItem = mItemList[0];
                LoopListViewItem2 lastItem = mItemList[mItemList.Count - 1];
                float viewMaxX = GetContentPanelSize();
                if (firstItem.LeftX < 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.LeftX != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if ((lastItem.RightX) > viewMaxX || (lastItem.ItemIndex == mCurReadyMaxItemIndex && lastItem.RightX != viewMaxX))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                LoopListViewItem2 firstItem = mItemList[0];
                LoopListViewItem2 lastItem = mItemList[mItemList.Count - 1];
                float viewMaxX = GetContentPanelSize();
                if (firstItem.RightX > 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.RightX != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if ((-lastItem.LeftX) > viewMaxX || (lastItem.ItemIndex == mCurReadyMaxItemIndex && (-lastItem.LeftX) != viewMaxX))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

            }

        }


        

        void UpdateAllShownItemsPos()
        {
            int count = mItemList.Count;
            if (count == 0)
            {
                return;
            }

            mAdjustedVec = (mContainerTrans.localPosition - mLastFrameContainerPos);

            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {   
                    //计算第一个显示的Item的高度
                    pos = -GetItemPos(mItemList[0].ItemIndex);
                }
                //第一个显示的Item的在Content中的localposY
                float pos1 = mItemList[0].CachedRectTransform.localPosition.y;
                float distance = pos - pos1;

                //根据第一个显示的Item的Pos更新后续Item的高度
                float curY = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem2 item = mItemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY - item.CachedRectTransform.rect.height - item.Padding;
                }
                if(distance != 0)
                {
                    Vector2 p = mContainerTrans.localPosition;
                    p.y = p.y - distance;
                    //mContainerTrans.localPosition = p;
                    SetContainerLocalPosY(p.y);
                }
                
            }
            else if(mArrangeType == ListItemArrangeType.BottomToTop)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {
                    pos = GetItemPos(mItemList[0].ItemIndex);
                }
                float pos1 = mItemList[0].CachedRectTransform.localPosition.y;
                float d = pos - pos1;
                float curY = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem2 item = mItemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY + item.CachedRectTransform.rect.height + item.Padding;
                }
                if(d != 0)
                {
                    Vector3 p = mContainerTrans.localPosition;
                    p.y = p.y - d;
                    //mContainerTrans.localPosition = p;
                    SetContainerLocalPosY(p.y);
                }
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {
                    pos = GetItemPos(mItemList[0].ItemIndex);
                }
                float pos1 = mItemList[0].CachedRectTransform.localPosition.x;
                float d = pos - pos1;
                float curX = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem2 item = mItemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX + item.CachedRectTransform.rect.width + item.Padding;
                }
                if (d != 0)
                {
                    Vector3 p = mContainerTrans.localPosition;
                    p.x = p.x - d;
                    mContainerTrans.localPosition = p;
                }

            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {
                    pos = -GetItemPos(mItemList[0].ItemIndex);
                }
                float pos1 = mItemList[0].CachedRectTransform.localPosition.x;
                float d = pos - pos1;
                float curX = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem2 item = mItemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX - item.CachedRectTransform.rect.width - item.Padding;
                }
                if (d != 0)
                {
                    Vector3 p = mContainerTrans.localPosition;
                    p.x = p.x - d;
                    mContainerTrans.localPosition = p;
                }

            }
            //如果在拖拽中，就将UGUI的ScrollRect标记为脏
            if (mIsDraging)
            {
                mScrollRect.OnBeginDrag(mPointerEventData);
                mScrollRect.Rebuild(CanvasUpdate.PostLayout);
                //mScrollRect.velocity = mAdjustedVec;
                SetVelocity(mAdjustedVec);
                mNeedAdjustVec = true;
            }
        }

        /// <summary>
        /// 根据Item的偏移啥的，计算Content的总Size，相当于Content的RectTransform的height 或 width。 并赋值给Content的Rect
        /// </summary>
        void UpdateContentSize()
        {
            float size = GetContentPanelSize();
            if (mIsVertList)
            {
                if(mContainerTrans.rect.height != size)
                { 
                    mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                }
            }
            else
            {
                if(mContainerTrans.rect.width != size)
                {
                    //Debug.LogErrorFormat("mContainerTrans.rect.height = {0}, {1}", mContainerTrans.rect.height, size);
                    mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                }
            }
        }

        /// <summary>
        /// 拖拽中断按钮的点击动画
        /// </summary>
        void CanCleItemClickAnimation()
        {
            GameObject go = EventSystem.current.currentSelectedGameObject;
            if (null != go && null != mItemList && mItemList.Count > 0)
            {
                for (int i = 0; i < mItemList.Count; ++i)
                {
                    if (null != mItemList[i] && go.transform.IsChildOf(mItemList[i].transform))
                    {
                        mItemList[i].CancleMouseClickAnimation();
                        break;
                    }
                }
            }
        }

        public void ClearAllItems() 
        {
            if (mItemList.Count > 0)
            {
                HashSet<string> prefabs = new HashSet<string>();
                for (int i = 0; i < mItemList.Count; ++i) 
                {
                    if (prefabs.Contains(mItemList[i].ItemPrefabName)) 
                    {
                        continue;
                    }
                    prefabs.Add(mItemList[i].ItemPrefabName);
                }
                SetListItemCount(0, true);
                foreach (var prefabName in prefabs) 
                {
                    ItemPool pool = null;
                    if (mItemPoolDict.TryGetValue(prefabName, out pool) == false)
                    {
                        return;
                    }
                    pool.DestroyAllItem();
                }
            }
        }


        void SetAnchoredPositionX(RectTransform rtf, float x)
        {
            Vector3 pos = rtf.anchoredPosition3D;
            pos.x = x;
            rtf.anchoredPosition3D = pos;
        }
        void SetAnchoredPositionY(RectTransform rtf, float y)
        {
            Vector3 pos = rtf.anchoredPosition3D;
            pos.y = y;
            rtf.anchoredPosition3D = pos;
        }

        /*
        InitListView method is to initiate the LoopListView2 component. There are 3 parameters:
        itemTotalCount: the total item count in the listview. If this parameter is set -1, then means there are infinite items, and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.
        onGetItemByIndex: when a item is getting in the scrollrect viewport, and this Action will be called with the item’ index as a parameter, to let you create the item and update its content.
        */
        public void InitListView(int itemTotalCount,
            Func<LoopListView2, int, LoopListViewItem2> onGetItemByIndex,
            LoopListViewInitParam initParam = null,
            Func<LoopListView2, int, string> onGetItemNameByIndex = null)
        {
            if (initParam != null)
            {
                mDistanceForRecycle0 = initParam.mDistanceForRecycle0;
                mDistanceForNew0 = initParam.mDistanceForNew0;
                mDistanceForRecycle1 = initParam.mDistanceForRecycle1;
                mDistanceForNew1 = initParam.mDistanceForNew1;
                mSmoothDumpRate = initParam.mSmoothDumpRate;
                mSnapFinishThreshold = initParam.mSnapFinishThreshold;
                mSnapVecThreshold = initParam.mSnapVecThreshold;
                mItemDefaultWithPaddingSize = initParam.mItemDefaultWithPaddingSize;
            }
            mScrollRect = gameObject.GetComponent<ScrollRect>();
            if (mScrollRect == null)
            {
                Debug.LogError("ListView Init Failed! ScrollRect component not found!");
                return;
            }
            if (mDistanceForRecycle0 <= mDistanceForNew0)
            {
                Debug.LogError("mDistanceForRecycle0 should be bigger than mDistanceForNew0");
            }
            if (mDistanceForRecycle1 <= mDistanceForNew1)
            {
                Debug.LogError("mDistanceForRecycle1 should be bigger than mDistanceForNew1");
            }
            mItemPosMgr = new ItemPosMgr(mItemDefaultWithPaddingSize);
            mScrollRectTransform = mScrollRect.GetComponent<RectTransform>();
            mContainerTrans = mScrollRect.content;
            mViewPortRectTransform = mScrollRect.viewport;
            if (mViewPortRectTransform == null)
            {
                mViewPortRectTransform = mScrollRectTransform;
            }
            if (mScrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport && mScrollRect.horizontalScrollbar != null)
            {
                Debug.LogError("ScrollRect.horizontalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            }
            if (mScrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport && mScrollRect.verticalScrollbar != null)
            {
                Debug.LogError("ScrollRect.verticalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            }
            mIsVertList = (mArrangeType == ListItemArrangeType.TopToBottom || mArrangeType == ListItemArrangeType.BottomToTop);
            mScrollRect.horizontal = !mIsVertList;
            mScrollRect.vertical = mIsVertList;
            SetScrollbarListener();
            AdjustPivot(mViewPortRectTransform);
            AdjustAnchor(mContainerTrans);
            AdjustContainerPivot(mContainerTrans);
            InitItemPool();
            mOnGetItemByIndex = onGetItemByIndex;
            mOnGetItemNameByIndex = onGetItemNameByIndex;
            ResetListView();
            //SetListItemCount(itemTotalCount, true);
        }



        /*
        This method will move the scrollrect content’s position to ( the positon of itemIndex-th item + offset ),
        and in current version the itemIndex is from 0 to MaxInt, offset is from 0 to scrollrect viewport size. 
        */
        public void MovePanelToItemIndex(int itemIndex, float offset)
        {
            mScrollRect.StopMovement();
            if (itemIndex < 0 || mItemTotalCount == 0)
                return;

            if (mItemTotalCount > 0 && itemIndex >= mItemTotalCount)
                itemIndex = mItemTotalCount - 1;

            if (offset < 0)
                offset = 0;

            Vector3 pos = Vector3.zero;
            float viewPortSize = ViewPortSize;
            if (offset > viewPortSize)
                offset = viewPortSize;


            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                float containerPos = mContainerTrans.localPosition.y;
                if (containerPos < 0)
                {
                    containerPos = 0;
                }
                pos.y = -containerPos - offset;
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                float containerPos = mContainerTrans.localPosition.y;
                if (containerPos > 0)
                {
                    containerPos = 0;
                }
                pos.y = -containerPos + offset;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                float containerPos = mContainerTrans.localPosition.x;
                if (containerPos > 0)
                {
                    containerPos = 0;
                }
                pos.x = -containerPos + offset;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                float containerPos = mContainerTrans.localPosition.x;
                if (containerPos < 0)
                {
                    containerPos = 0;
                }
                pos.x = -containerPos - offset;
            }

            RecycleAllItem();
            LoopListViewItem2 newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                ClearAllTmpRecycledItem();
                return;
            }
            if (mIsVertList)
                pos.x = newItem.StartPosOffset;
            else
                pos.y = newItem.StartPosOffset;

            newItem.CachedRectTransform.localPosition = pos;
            if (mSupportScrollBar)
            {
                if (mIsVertList)
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                else
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
            }

            mItemList.Add(newItem);
  
            UpdateListView(viewPortSize + 100, viewPortSize + 100, viewPortSize, viewPortSize);
            UpdateContentSize();
            AdjustPanelPos();
            ClearAllTmpRecycledItem();
        }

        //update all visible items.
        public void RefreshAllShownItem()
        {
            int count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            RefreshAllShownItemWithFirstIndex(mItemList[0].ItemIndex);
        }
        public void RefreshAllShownItemWithFirstIndex(int firstItemIndex)
        {
            int count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            LoopListViewItem2 firstItem = mItemList[0];
            Vector3 pos = firstItem.CachedRectTransform.localPosition;
            RecycleAllItem();
            for (int i = 0; i < count; ++i)
            {
                int curIndex = firstItemIndex + i;
                LoopListViewItem2 newItem = GetNewItemByIndex(curIndex);
                if (newItem == null)
                {
                    break;
                }
                if (mIsVertList)
                {
                    pos.x = newItem.StartPosOffset;
                }
                else
                {
                    pos.y = newItem.StartPosOffset;
                }
                newItem.CachedRectTransform.localPosition = pos;
                if (mSupportScrollBar)
                {
                    if (mIsVertList)
                    {
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }
                    else
                    {
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }
                }

                mItemList.Add(newItem);
            }
            UpdateContentSize();
            UpdateAllShownItemsPos();
            ClearAllTmpRecycledItem();
        }

        /*
        This method may use to set the item total count of the scrollview at runtime. 
        If this parameter is set -1, then means there are infinite items,
        and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. 
        If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.  
        If resetPos is set false, then the scrollrect’s content position will not changed after this method finished.
        */
        /// <summary>
        /// 用于在运行时设置滚动视图中的 Item 总数。
        /// </summary>
        /// <param name="itemCount"></param>
        /// <param name="resetPos"></param>
        /// <param name="needMoveToIndex"></param>
        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            if (itemCount == mItemTotalCount)
                return;
            mItemTotalCount = itemCount;
            if (mItemTotalCount < 0)
                mSupportScrollBar = false;
            if (mSupportScrollBar)
                mItemPosMgr.SetItemMaxCount(mItemTotalCount);
            else
                mItemPosMgr.SetItemMaxCount(0);

            if (mItemTotalCount == 0)
            {
                mCurReadyMaxItemIndex = 0;
                mCurReadyMinItemIndex = 0;
                mNeedCheckNextMaxItem = false;
                mNeedCheckNextMinItem = false;
                RecycleAllItem();
                ClearAllTmpRecycledItem();
                UpdateContentSize();
                if (IsVertList)
                {
                    SetAnchoredPositionY(mContainerTrans, 0f);
                }
                else
                {
                    SetAnchoredPositionX(mContainerTrans, 0f);
                }
                return;
            }

            mLeftSnapUpdateExtraCount = 1;
            mNeedCheckNextMaxItem = true;
            mNeedCheckNextMinItem = true;
            if (resetPos)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }
            if (mItemList.Count == 0)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }

            //如果给定的数据总数量 大于最后一个显示 Item 的数据索引时 才更新。 虽然不知道为毛要这么设计 
            int maxItemIndex = mItemTotalCount - 1;
            int lastItemIndex = mItemList[mItemList.Count - 1].ItemIndex;
            if (lastItemIndex <= maxItemIndex)
            {
                UpdateContentSize();
                UpdateAllShownItemsPos();
                return;
            }
            MovePanelToItemIndex(maxItemIndex, 0);
        }

        public void SetListItemCountNew(int itemCount)
        {
            //if (itemCount == mItemTotalCount)
            //    return;
            //mItemTotalCount = itemCount;
            //if (mItemTotalCount < 0)
            //    mSupportScrollBar = false;
            //if (mSupportScrollBar)
            //    mItemPosMgr.SetItemMaxCount(mItemTotalCount);
            //else
            //    mItemPosMgr.SetItemMaxCount(0);

            //if (mSupportScrollBar)
            //{
            //    mItemPosMgr.Update(false);
            //}

            //mLeftSnapUpdateExtraCount = 1;
            //mNeedCheckNextMaxItem = true;
            //mNeedCheckNextMinItem = true;


            //RecycleAllItem();
            //ClearAllTmpRecycledItem();

            //UpdateContentSize();
            //UpdateAllShownItemsPos();
        }


        //强制刷新一次 因为当新增数据的时候调用MovePanelToItemIndex 位置不对 ， add by sunliwen
        public void ForceUpdate()
        {
            Update();
        }

        void Update()
        {
            if (mNeedAdjustVec)
            {
                mNeedAdjustVec = false;
                if (mIsVertList)
                {
                    if (mScrollRect.velocity.y * mAdjustedVec.y > 0)
                    {
                        //mScrollRect.velocity = mAdjustedVec;
                        SetVelocity(mAdjustedVec);
                    }
                }
                else
                {
                    if (mScrollRect.velocity.x * mAdjustedVec.x > 0)
                    {
                        //mScrollRect.velocity = mAdjustedVec;
                        SetVelocity(mAdjustedVec);
                    }
                }
            }



            if (mSupportScrollBar)
            {
                mItemPosMgr.Update(false);
            }
            UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
            ClearAllTmpRecycledItem();
            mLastFrameContainerPos = mContainerTrans.localPosition;
        }


        #region 统一入口，方便定位Bug
        private void SetContainerLocalPosY(float y)
        {
            Vector2 srcPos = mContainerTrans.localPosition;
            var height = mContainerTrans.rect.height;

            //Debug.LogError($"#LoopList# SetContainerLocalPosY srcPos.y:{srcPos.y}, newY:{y}, height:{height}");
            if (y > height+10)
            {
                Debug.LogError($"#LoopList# SetContainerLocalPosY异常!!! srcPos.y:{srcPos.y}, newY:{y}, height:{height}");
            }
            
            srcPos.y = y;
            mContainerTrans.localPosition = srcPos;
        }

        public void SetVelocity(Vector2 velocity)
        {
            //Debug.LogError($"设置的速度为:{velocity.y}");
            var srcVelocity = mScrollRect.velocity;
            //Debug.LogError($"#LoopList# SetContainerLocalPosY srcVelocity.y:{srcVelocity.y}, newVelocity:{velocity.y}");
            if (Math.Abs(velocity.y - srcVelocity.y) > 5000)
            {
                Debug.LogError($"#LoopList# SetVelocity异常!!!  srcVelocity.y:{srcVelocity.y}, newVelocity:{velocity.y}");
            }

            velocity.y = Mathf.Clamp(velocity.y, -1000f, 1000f);
            mScrollRect.velocity = velocity;
        }

        #endregion
    }

}
