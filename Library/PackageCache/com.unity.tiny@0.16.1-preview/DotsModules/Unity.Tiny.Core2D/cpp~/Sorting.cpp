#include <stdint.h>

#include "zeroplayer.h"
#include "EntityWrappers.h"

#include <unordered_map>
#include <vector>
#include <algorithm>

#include "bind-Unity_Tiny_Core2D.h"

using namespace Unity::Entities;
using namespace Unity::Tiny::Core2D;

namespace std
{
    template<> struct hash<Entity>
    {
        std::size_t operator()(const Entity& e) const
        {
            return e.index;
        }
    };
};

namespace Unity { namespace Tiny { namespace Core2D {

bool
operator<(const SortedEntity& left, const SortedEntity& right)
{
    if (left.combinedKey == right.combinedKey)
        return left.e.index < right.e.index;
    return left.combinedKey < right.combinedKey;
}

}}};

struct Group
{
    int nElems;
    int startElemIdx;
    int flatIdx;
};

static std::unordered_map<Entity, Group> groups;
static std::vector<int> ingroup;

static void flattenRec(const SortedEntity& es, SortedEntity *sortedInput, SortedEntity *sortedlist, int &ofs)
{
    sortedlist[ofs++] = es;
    auto subgroup = groups.find(es.e); // find group the element is the head of
    if (subgroup != groups.end()) {
        for (int i = 0; i < (int)subgroup->second.nElems; i++)
            flattenRec(sortedInput[subgroup->second.startElemIdx + i], sortedInput, sortedlist, ofs);
    }
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL sortexternal ( const DisplayListEntry *de, SortedEntity *sortedlist, int n )
{
    int nextGroupFlatIdx = 0;
    groups.clear();
    ingroup.clear();
    ingroup.reserve(n);

    // first do an extra part for previously inlined push
    for ( int i=0; i<n; i++) {
        Entity group = de[i].inSortingGroup;
        int ig;
        auto g = groups.find(group); // find group the element is part of
        if (g == groups.end()) {
            ig = nextGroupFlatIdx;
            groups[group] = {1, 0, ig}; // nElems, startElemIdx, flatIdx
            nextGroupFlatIdx++;
        } else {
            ig = g->second.flatIdx;
            g->second.nElems++;
        }
        ingroup.push_back(ig);   // reserved
    }

    // sort
    if (groups.size() == 1) {
        // if there are no sorting groups, short cut to sorting the list directly
        std::sort(sortedlist, sortedlist+n);
        return;
    }
    // go through groups and put in flat list
    std::vector<Group> flatgroups(groups.size());
    for (auto& i : groups)
        flatgroups[i.second.flatIdx] = i.second;
    // assign memory locations via flat list and allocate locations in flat list
    int f = 0;
    for (int i = 0; i < (int)flatgroups.size(); i++) {
        flatgroups[i].startElemIdx = f;
        f += flatgroups[i].nElems;
        flatgroups[i].nElems = 0;
    }
    // write out all elements by group in the flat list ready for sorting
    std::vector<SortedEntity> sortedInput(n);
    for (int i = 0; i < (int)ingroup.size(); i++) {
        auto& fg = flatgroups[ingroup[i]];
        int desti = fg.startElemIdx + fg.nElems;
        fg.nElems++;
        sortedInput[desti] = sortedlist[i];
    }
    // sort all groups
    for (int i = 0; i < (int)flatgroups.size(); i++) {
        auto& fg = flatgroups[i];
        std::sort(sortedInput.begin() + fg.startElemIdx, sortedInput.begin() + fg.startElemIdx + fg.nElems);
    }
    // reassign hashed groups from flat groups
    // (could skip this step and do an indirection during flatten, but assume there are much less groups than elements)
    for (auto& i : groups)
        i.second = flatgroups[i.second.flatIdx];
    // flatten
    int ofs = 0;
    Entity noneE;
    noneE.index = 0;
    noneE.version = 0;
    auto& rootgroup = groups[noneE];
    for (int i = 0; i < rootgroup.nElems; i++)
        flattenRec(sortedInput[rootgroup.startElemIdx + i], sortedInput.data(), sortedlist, ofs);
}

// reference implemenetation for documentation and debugging
#if 0

static void flattenRec_ref (SortedEntity es, SortedEntity *dest, int &ndest, const std::unordered_map<Entity,std::vector<SortedEntity>> &groups) {
    dest[ndest++] = es;
    auto subgroup = groups.find(es.e);
    if (subgroup!=groups.end()) {
        for (int i=0; i<(int)subgroup->second.size(); i++)
            flattenRec_ref(subgroup->second[i], dest, ndest, groups);
    }
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL sortexternal_ref ( const DisplayListEntry *de, SortedEntity *sortedlist, int n )
{
    std::unordered_map<Entity,std::vector<SortedEntity>> groups;

    // push all to init
    for ( int i=0; i<n; i++ )
        groups[de[i].inSortingGroup].push_back(sortedlist[i]);

    // do sorting in groups
    for (auto &i : groups)
        std::sort(i.second.begin(), i.second.end());

    // gather groups, starting with top level
    Entity enone = {0,0};
    std::vector<SortedEntity> &rootgroup = groups[enone];
    int k = 0;
    for (int i=0; i<(int)rootgroup.size(); i++)
        flattenRec_ref (rootgroup[i], sortedlist, k, groups);
}

#endif
