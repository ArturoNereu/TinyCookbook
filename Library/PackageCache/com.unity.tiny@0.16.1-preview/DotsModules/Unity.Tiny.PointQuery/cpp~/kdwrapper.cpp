#include <float.h>
#include <stdint.h>
#include <math.h>
#include <stdio.h>

#include "zeroplayer.h"
#include "EntityWrappers.h"

// KDTree doesn't have include files, so warm
// up the needed includes here, before including KDTree
#include <map>
#include <vector>
#include <algorithm>

// FIXME: better approach than just disabling asserts
#define Assert
#include "KDTree.h"

// #define WRITE_LOG

using namespace Unity::Entities;

typedef KDTree<float, 3, Entity> Tree3;
static const Entity NoEntity = {0, 0};

struct PointQueryStruct
{
    PointQueryStruct() {
        tree = new Tree3();
        dirty = true;
    }

    bool dirty;
    Tree3* tree;
};


int idPool = 0;
std::map<int, PointQueryStruct> treeStorage;

PointQueryStruct* GetPointQueryStruct(int queryID)
{
    if (queryID == 0) {
        queryID = ++idPool;
        #ifdef WRITE_LOG
        printf("Creating Tree id=%d\n", queryID);
        #endif
        treeStorage[queryID] = PointQueryStruct();
    }
    // Pointer should be stable as long as we mutate tree.
    PointQueryStruct* pQuery = &treeStorage.find(queryID)->second;
    return pQuery;
}


ZEROPLAYER_EXPORT
int ZEROPLAYER_CALL initPointQueryStruct(int queryID, int size)
{
    PointQueryStruct* pQuery = GetPointQueryStruct(queryID);
    pQuery->tree->Reset(size);
    pQuery->dirty = true;

    return (queryID > 0) ? queryID : idPool;
}


ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL freePointQueryStruct(int queryID)
{
    PointQueryStruct* pQuery = GetPointQueryStruct(queryID);
    delete pQuery->tree;
    treeStorage.erase(queryID);
    #ifdef WRITE_LOG
    printf("Free Tree id=%d\n", queryID);
    #endif
}


ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL addPointToQueryStruct(int queryID, Entity e, const float* vector3)
{
    PointQueryStruct* pQuery = GetPointQueryStruct(queryID);
    pQuery->tree->AddElements(vector3, &e, 1);
    pQuery->dirty = true;
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL addPointsToQueryStruct(int queryID, int n, const Entity* e, const float* vector3)
{
    PointQueryStruct* pQuery = GetPointQueryStruct(queryID);
    pQuery->tree->AddElements(vector3, e, n);
    pQuery->dirty = true;
}

// Returns the number of results found.
ZEROPLAYER_EXPORT
int ZEROPLAYER_CALL queryNearestPoints(int queryID, const float* vector3Array, float maxDist, float minDist, int nDst, void* _dst)
{
    Tree3::Result* dst = (Tree3::Result*)_dst;

    const float maxDist2 = (maxDist >= 0) ? maxDist * maxDist : FLT_MAX;
    const float minDist2 = (minDist >= 0) ? minDist * minDist : 0;

    PointQueryStruct* pQuery = GetPointQueryStruct(queryID);
    if (pQuery->dirty) {
        pQuery->tree->Build();
        pQuery->dirty = false;
    }

    // Convert the results back to distance (not squared).
    if (nDst == 0) {
        return 0;
    }
    if (nDst == 1) {
        *dst = pQuery->tree->QueryClosest(vector3Array, maxDist2, minDist2, NoEntity);
        if (dst->id == NoEntity) {
            dst->distSqr = 0;
            return 0;
        }
        dst->distSqr = sqrt(dst->distSqr);
        return 1;
    }
    int n = pQuery->tree->QueryNClosest(vector3Array, maxDist2, minDist2, nDst, dst);
    for(int i=0; i<n; ++i) {
        dst[i].distSqr = sqrt(dst[i].distSqr);
    }
    return n;
}

ZEROPLAYER_EXPORT
int ZEROPLAYER_CALL numTreesAllocated()
{
    #ifdef WRITE_LOG
    printf("Trees allocated=%d\n", (int)treeStorage.size());
    #endif
    return (int)treeStorage.size();
}


