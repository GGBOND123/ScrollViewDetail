using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SuperScrollView
{

    public class ItemSizeGroup
    {
        /// <summary>
        /// Group中每个Item的Size的List
        /// </summary>
        public float[] mItemSizeArray = null;
        public float[] mItemStartPosArray = null;
        /// <summary>
        /// 当前Group的Item数量
        /// </summary>
        public int mItemCount = 0;
        int mDirtyBeginIndex = ItemPosMgr.mItemMaxCountPerGroup;
        /// <summary>
        /// 单个Gourp的Size大小之和
        /// </summary>
        public float mGroupSize = 0;
        public float mGroupStartPos = 0;
        public float mGroupEndPos = 0;
        public int mGroupIndex = 0;
        float mItemDefaultSize = 0;
        /// <summary>
        /// 当前Group最大Item的Index
        /// </summary>
        int mMaxNoZeroIndex = 0;
        public ItemSizeGroup(int index,float itemDefaultSize)
        {
            mGroupIndex = index;
            mItemDefaultSize = itemDefaultSize;
            Init();
        }

        public void Init()
        {
            mItemSizeArray = new float[ItemPosMgr.mItemMaxCountPerGroup];
            if (mItemDefaultSize != 0)
            {
                for (int i = 0; i < mItemSizeArray.Length; ++i)
                    mItemSizeArray[i] = mItemDefaultSize;
            }
            mItemStartPosArray = new float[ItemPosMgr.mItemMaxCountPerGroup];
            mItemStartPosArray[0] = 0;
            mItemCount = ItemPosMgr.mItemMaxCountPerGroup;
            mGroupSize = mItemDefaultSize * mItemSizeArray.Length;
            //如果itemDefaultSize为0，直接从最后一个索引 才开始标记为脏。
            if (mItemDefaultSize != 0)
            {
                mDirtyBeginIndex = 0;
            }
            else
            {
                mDirtyBeginIndex = ItemPosMgr.mItemMaxCountPerGroup;
            }
        }

        public float GetItemStartPos(int index)
        {
            return mGroupStartPos + mItemStartPosArray[index];
        }

        public bool IsDirty
        {
            get
            {
                return (mDirtyBeginIndex < mItemCount);
            }
        }
        public float SetItemSize(int index, float size)
        {
            if(index > mMaxNoZeroIndex && size > 0)
            {
                mMaxNoZeroIndex = index;
            }
            float old = mItemSizeArray[index];
            if (old == size)
            {
                return 0;
            }
            mItemSizeArray[index] = size;
            if (index < mDirtyBeginIndex)
            {
                mDirtyBeginIndex = index;
            }
            float ds = size - old;
            mGroupSize = mGroupSize + ds;
            return ds;
        }

        public void SetItemCount(int count)
        {
            if(count < mMaxNoZeroIndex)
                mMaxNoZeroIndex = count;
            if (mItemCount == count)
                return;
            
            mItemCount = count;
            RecalcGroupSize();
        }

        /// <summary>
        /// 重新累加mItemSizeArray中的每个Item的Size到   mGroupSize中
        /// </summary>
        public void RecalcGroupSize()
        {
            mGroupSize = 0;
            for (int i = 0; i < mItemCount; ++i)
            {
                mGroupSize += mItemSizeArray[i];
            }
        }

        public int GetItemIndexByPos(float pos)
        {
            if (mItemCount == 0)
            {
                return -1;
            }
            
            int low = 0;
            int high = mItemCount - 1;
            if (mItemDefaultSize == 0f)
            {
                if(mMaxNoZeroIndex < 0)
                {
                    mMaxNoZeroIndex = 0;
                }
                high = mMaxNoZeroIndex;
            }
            while (low <= high)
            {
                int mid = (low + high) / 2;
                float startPos = mItemStartPosArray[mid];
                float endPos = startPos + mItemSizeArray[mid];
                if (startPos <= pos && endPos >= pos)
                {
                    return mid;
                }
                else if (pos > endPos)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            return -1;
        }

        public void UpdateAllItemStartPos()
        {
            if (mDirtyBeginIndex >= mItemCount)
            {
                return;
            }
            int startIndex = (mDirtyBeginIndex < 1) ? 1 : mDirtyBeginIndex;
            for (int i = startIndex; i < mItemCount; ++i)
            {
                mItemStartPosArray[i] = mItemStartPosArray[i - 1] + mItemSizeArray[i - 1];
            }
            mDirtyBeginIndex = mItemCount;
        }

        public void ClearOldData()
        {
            for (int i = mItemCount; i < ItemPosMgr.mItemMaxCountPerGroup; ++i)
            {
                mItemSizeArray[i] = 0;
            }
        }
    }

    public class ItemPosMgr
    {
        public const int mItemMaxCountPerGroup = 100;
        /// <summary>
        /// 以Group为单位，List<ItemSizeGroup>。每个ItemSizeGroup包含所含每个Item的Size
        /// </summary>
        private List<ItemSizeGroup> mItemSizeGroupList = new List<ItemSizeGroup>();
        /// <summary>
        /// 第一个为脏的Item 的 Index
        /// </summary>
        private int mDirtyBeginIndex = int.MaxValue;

        /// <summary>
        /// 所有Group的Size总和。
        /// </summary>
        public float mTotalSize = 0;
        public float mItemDefaultSize = 20;
        /// <summary>
        /// 当前Group最大的Index
        /// </summary>
        private int mMaxNotEmptyGroupIndex = 0;

        public ItemPosMgr(float itemDefaultSize)
        {
            mItemDefaultSize = itemDefaultSize;
        }

        /// <summary>
        /// 设置重新计算的数据部分。
        /// </summary>
        /// <param name="maxCount"></param>
        public void SetItemMaxCount(int maxCount)
        {
            mDirtyBeginIndex = 0;
            mTotalSize = 0;
            int st = maxCount % mItemMaxCountPerGroup;
            int needMaxGroupCount = maxCount / mItemMaxCountPerGroup;
            
            //计算最后一个group的数量。
            int lastGroupItemCount = 0;
            if (st > 0)
            {
                lastGroupItemCount = st;
                needMaxGroupCount++;
            }
            else
                lastGroupItemCount = mItemMaxCountPerGroup;


            //根据需要的group的数量，计算mItemSizeGroupList的大小。每个group，100个单位
            int count = mItemSizeGroupList.Count;
            if (count > needMaxGroupCount)
            {
                //移除多余的ItemSizeGroup
                int d = count - needMaxGroupCount;
                mItemSizeGroupList.RemoveRange(needMaxGroupCount, d);
            }
            else if (count < needMaxGroupCount)
            {
                if(count > 0)
                    mItemSizeGroupList[count - 1].ClearOldData();
                int d = needMaxGroupCount - count;
                for (int i = 0; i < d; ++i)
                {
                    ItemSizeGroup tGroup = new ItemSizeGroup(count + i, mItemDefaultSize);
                    mItemSizeGroupList.Add(tGroup);
                }
            }
            else
            {
                if (count > 0)
                    mItemSizeGroupList[count - 1].ClearOldData();
            }


            count = mItemSizeGroupList.Count;
            if((count-1) < mMaxNotEmptyGroupIndex)
                mMaxNotEmptyGroupIndex = count - 1;
            if(mMaxNotEmptyGroupIndex < 0)
                mMaxNotEmptyGroupIndex = 0;
            if (count == 0)
                return;
            

            for (int i = 0; i < count - 1; ++i)
                mItemSizeGroupList[i].SetItemCount(mItemMaxCountPerGroup);
            mItemSizeGroupList[count - 1].SetItemCount(lastGroupItemCount);
            
            for (int i = 0; i < count; ++i)
                mTotalSize = mTotalSize + mItemSizeGroupList[i].mGroupSize;

        }

        public void SetItemSize(int itemIndex, float size)
        {
            int groupIndex = itemIndex / mItemMaxCountPerGroup;
            int indexInGroup = itemIndex % mItemMaxCountPerGroup;
            ItemSizeGroup tGroup = mItemSizeGroupList[groupIndex];
            float changedSize = tGroup.SetItemSize(indexInGroup, size);
            if (changedSize != 0f)
            {
                if (groupIndex < mDirtyBeginIndex)
                {
                    mDirtyBeginIndex = groupIndex;
                }
            }
            mTotalSize += changedSize;
            if(groupIndex > mMaxNotEmptyGroupIndex && size > 0)
            {
                mMaxNotEmptyGroupIndex = groupIndex;
            }
        }

        public float GetItemPos(int itemIndex)
        {
            Update(true);
            int groupIndex = itemIndex / mItemMaxCountPerGroup;
            int indexInGroup = itemIndex % mItemMaxCountPerGroup;
            return mItemSizeGroupList[groupIndex].GetItemStartPos(indexInGroup);
        }

        public bool GetItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
        {
            Update(true);
            index = 0;
            itemPos = 0f;
            int count = mItemSizeGroupList.Count;
            if (count == 0)
            {
                return true;
            }
            ItemSizeGroup hitGroup = null;

            int low = 0;
            int high = count - 1;

            if (mItemDefaultSize == 0f)
            {
                if(mMaxNotEmptyGroupIndex < 0)
                {
                    mMaxNotEmptyGroupIndex = 0;
                }
                high = mMaxNotEmptyGroupIndex;
            }
            while (low <= high)
            {
                int mid = (low + high) / 2;
                ItemSizeGroup tGroup = mItemSizeGroupList[mid];
                if (tGroup.mGroupStartPos <= pos && tGroup.mGroupEndPos >= pos)
                {
                    hitGroup = tGroup;
                    break;
                }
                else if (pos > tGroup.mGroupEndPos)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            int hitIndex = -1;
            if (hitGroup != null)
            {
                hitIndex = hitGroup.GetItemIndexByPos(pos - hitGroup.mGroupStartPos);
            }
            else
            {
                return false;
            }
            if (hitIndex < 0)
            {
                return false;
            }
            index = hitIndex + hitGroup.mGroupIndex * mItemMaxCountPerGroup;
            itemPos = hitGroup.GetItemStartPos(hitIndex);
            return true;
        }

        /// <summary>
        ///updateAll：false时，只更新mDirtyBeginIndex 第一个为脏的Item 的 Index
        /// </summary>
        /// <param name="updateAll"></param>
        public void Update(bool updateAll)
        {
            int count = mItemSizeGroupList.Count;
            if (count == 0)
                return;
            if (mDirtyBeginIndex >= count)
                return;

            int loopCount = 0;
            for (int i = mDirtyBeginIndex; i < count; ++i)
            {
                loopCount++;
                ItemSizeGroup tGroup = mItemSizeGroupList[i];
                mDirtyBeginIndex++;
                tGroup.UpdateAllItemStartPos();
                if (i == 0)
                {
                    tGroup.mGroupStartPos = 0;
                    tGroup.mGroupEndPos = tGroup.mGroupSize;
                }
                else
                {
                    tGroup.mGroupStartPos = mItemSizeGroupList[i - 1].mGroupEndPos;
                    tGroup.mGroupEndPos = tGroup.mGroupStartPos + tGroup.mGroupSize;
                }
                if (!updateAll && loopCount > 1)
                {
                    return;
                }

            }
        }

    }
}