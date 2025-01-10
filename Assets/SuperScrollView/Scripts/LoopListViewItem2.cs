using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperScrollView
{
    public class LoopListViewItem2 : MonoBehaviour
    {


        bool mCancleAnimation = false;
        string mAnimatorPath;






        //这些是用户自定义的数据存储字段，可以用于存储与项相关的额外信息。这些字段通常用于存储特定的标识符或其他数据，以便在回调或事件处理中使用。
        public object UserObjectData
        {
            get { return mUserObjectData; }
            set { mUserObjectData = value; }
        }
        object mUserObjectData = null;

        public int UserIntData1
        {
            get { return mUserIntData1; }
            set { mUserIntData1 = value; }
        }
        int mUserIntData1 = 0;
        
        public int UserIntData2
        {
            get { return mUserIntData2; }
            set { mUserIntData2 = value; }
        }
        int mUserIntData2 = 0;
        
        public string UserStringData1
        {
            get { return mUserStringData1; }
            set { mUserStringData1 = value; }
        }
        string mUserStringData1 = null;
        
        public string UserStringData2
        {

            get { return mUserStringData2; }
            set { mUserStringData2 = value; }
        }
        string mUserStringData2 = null;



        /// <summary>
        /// 该项与视口中心的距离。
        /// </summary>
        public float DistanceWithViewPortSnapCenter
        {
            get { return mDistanceWithViewPortSnapCenter; }
            set { mDistanceWithViewPortSnapCenter = value; }
        }
        float mDistanceWithViewPortSnapCenter = 0;

        /// <summary>
        /// 项在列表中显示时的起始位置偏移量。
        /// </summary>
        public float StartPosOffset
        {
            get { return mStartPosOffset; }
            set { mStartPosOffset = value; }
        }
        float mStartPosOffset = 0;

        /// <summary>
        /// 创建项时的检查帧数。这个值可以用来控制项创建的帧数?????
        /// </summary>
        public int ItemCreatedCheckFrameCount
        {
            get { return mItemCreatedCheckFrameCount; }
            set { mItemCreatedCheckFrameCount = value; }
        }
        int mItemCreatedCheckFrameCount = 0;

        /// <summary>
        /// 项的内边距（即项之间的间距）
        /// </summary>
        public float Padding
        {
            get { return mPadding; }
            set { mPadding = value; }
        }
        float mPadding;

        /// <summary>
        /// 该项的 RectTransform，用于获取项的位置、大小等信息。只有在需要时才会缓存该引用。
        /// </summary>
        public RectTransform CachedRectTransform
        {
            get
            {
                if (mCachedRectTransform == null)
                {
                    mCachedRectTransform = gameObject.GetComponent<RectTransform>();
                }
                return mCachedRectTransform;
            }
        }
        RectTransform mCachedRectTransform;

        public string ItemPrefabName
        {
            get
            {
                return mItemPrefabName;
            }
            set
            {
                mItemPrefabName = value;
            }
        }
        string mItemPrefabName;

        public string ItemAnimatorPath
        {
            get
            {
                return mAnimatorPath;
            }
            set
            {
                mAnimatorPath = value;
            }
        }

        public bool ItemCancleAnimation
        {
            get
            {
                return mCancleAnimation;
            }
            set
            {
                mCancleAnimation = value;
            }
        }

        /// <summary>
        /// 表示该项在列表中的数据索引。如果列表的 itemTotalCount 设置为 -1（无限项），则 ItemIndex 可以从 -MaxInt 到 +MaxInt。如果 itemTotalCount >= 0，则 ItemIndex 必须在 0 到 itemTotalCount - 1 之间。
        /// </summary>
        public int ItemIndex
        {
            get
            {
                return mItemIndex;
            }
            set
            {
                mItemIndex = value;
            }
        }
        int mItemIndex = -1;

        /// <summary>
        /// 第几次从池子中GetItem()，从第一次开始累加计数的。
        /// 用户自定义的项 ID。可以设置为任何整数值，用于标识该项，通常用于搜索或其他特定用途。
        /// </summary>
        public int ItemId
        {
            get
            {
                return mItemId;
            }
            set
            {
                mItemId = value;
            }
        }
        int mItemId = -1;

        public bool IsInitHandlerCalled
        {
            get
            {
                return mIsInitHandlerCalled;
            }
            set
            {
                mIsInitHandlerCalled = value;
            }
        }
        bool mIsInitHandlerCalled = false;

        /// <summary>
        /// 对应的LoopListView2 
        /// </summary>
        public LoopListView2 ParentListView
        {
            get
            {
                return mParentListView;
            }
            set
            {
                mParentListView = value;
            }
        }
        LoopListView2 mParentListView = null;




        /*  TopY、BottomY、LeftX、RightX：在 RectTransform 中的边界位置（相对于其父容器的 RectTransform,这个父容器一般就是content）
            TopY 和 BottomY 代表项的上下位置（垂直布局时使用）。
            LeftX 和 RightX 代表项的左右位置（水平布局时使用）。
            比如竖直视窗,第一个Item的TopY就是0,第二个就是第一个Item的高度加两者之间的间距.
        */
        /// <summary>
        /// 在 RectTransform 中的边界位置（相对于其父容器的 RectTransform,这个父容器一般就是content），具体注释点进来看
        /// </summary>
        public float TopY
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.TopToBottom)
                {
                    return CachedRectTransform.localPosition.y;
                }
                else if(arrageType == ListItemArrangeType.BottomToTop)
                {
                    return CachedRectTransform.localPosition.y + CachedRectTransform.rect.height;
                }
                return 0;
            }
        }
        /// <summary>
        /// 在 RectTransform 中的边界位置（相对于其父容器的 RectTransform,这个父容器一般就是content），具体注释点进来看
        /// </summary>
        public float BottomY
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.TopToBottom)
                {
                    return CachedRectTransform.localPosition.y - CachedRectTransform.rect.height;
                }
                else if (arrageType == ListItemArrangeType.BottomToTop)
                {
                    return CachedRectTransform.localPosition.y;
                }
                return 0;
            }
        }
        /// <summary>
        /// 在 RectTransform 中的边界位置（相对于其父容器的 RectTransform,这个父容器一般就是content），具体注释点进来看
        /// </summary>
        public float LeftX
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                {
                    return CachedRectTransform.localPosition.x;
                }
                else if (arrageType == ListItemArrangeType.RightToLeft)
                {
                    return CachedRectTransform.localPosition.x - CachedRectTransform.rect.width;
                }
                return 0;
            }
        }
        /// <summary>
        /// 在 RectTransform 中的边界位置（相对于其父容器的 RectTransform,这个父容器一般就是content），具体注释点进来看
        /// </summary>
        public float RightX
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                {
                    return CachedRectTransform.localPosition.x + CachedRectTransform.rect.width;
                }
                else if (arrageType == ListItemArrangeType.RightToLeft)
                {
                    return CachedRectTransform.localPosition.x;
                }
                return 0;
            }
        }

        /// <summary>
        /// 表示项的大小。如果列表是垂直布局，则返回项的高度；如果是水平布局，则返回项的宽度。
        /// </summary>
        public float ItemSize
        {
            get
            {
                if (ParentListView.IsVertList)
                {
                    return  CachedRectTransform.rect.height;
                }
                else
                {
                    return CachedRectTransform.rect.width;
                }
            }
        }

        /// <summary>
        /// 返回项的大小（包括内边距）。用于计算项占用的实际空间。比如竖直视窗,就是项的高度加上项之间的间距.
        /// </summary>
        public float ItemSizeWithPadding
        {
            get
            {
                return ItemSize + mPadding;
            }
        }

        public void CancleMouseClickAnimation()
        {
            if(mCancleAnimation)
            {
                Transform trans = null;
                if(string.IsNullOrEmpty(mAnimatorPath))
                {
                    trans = transform;
                }
                else
                {
                    trans = transform.Find(mAnimatorPath);
                }
                if(null != trans)
                {
                    Animator animator = trans.GetComponent<Animator>();
                    if (null != animator)
                        animator.Play("Disabled");
                }
            }
        }
    }
}
