#pragma once

// helper priority queue of n elements for n-or-less-closest searches
// top (0) element is always largest/farthest away
// if out of space, largest is replaced if new element is smaller
// optionally implemented as sorted list or binary heap. list is preferred for small n.
#include <cstring>
template<typename T, bool ISLIST = true>
class NPriorityQueue {
public:
    NPriorityQueue(int _n, T* memory)
        : n(_n)
        , fill(0)
        , elems(memory)
    {
    }

    bool Enqueue(const T& elem)
    {
        if (fill == n) {
            return EnqueueFull(elem);
        } else {
            EnqueueNotFull(elem);
            return true;
        }
    }

    inline void EnqueueNotFull(const T& elem)
    {
        Assert(fill != n);
        // if there is space, put at end and move up
        elems[fill] = elem;
        fill++;
        TrickleUp(fill - 1);
    }

    inline bool EnqueueFull(const T& elem)
    {
        Assert(fill == n);
        // ignore if larger than largest
        if (elems[0] < elem)
            return false;
        // otherwise replace largest
        elems[0] = elem;
        TrickleDown(0);
        return true;
    }

    inline bool Full() const { return fill == n; }

    inline bool Empty() const { return fill == 0; }

    inline const T& Peek() const { return elems[0]; }

    inline int Fill() const { return fill; }

    void SortResult()
    {
        if (ISLIST) {
            return;
        } else {
            if (fill) // sort backwards so it's the same as list mode
                std::sort(elems, elems + fill, [](const T& a, const T& b) { return (b < a); });
        }
    }

private:
    int n;
    int fill;
    T* elems;

    inline bool CheckIsFullySorted() const
    {
        for (int i = 0; i < fill - 1; i++)
            if (elems[i] < elems[i + 1])
                return false;
        return true;
    }

    inline bool IsChildSmallerEqual(int idx, int side) const
    {
        int ichild = (idx << 1) + 1 + side;
        return ichild >= fill || !(elems[idx] < elems[ichild]);
    }

    inline int LargerChildIndex(int idx) const
    {
        int ichild = (idx << 1) + 1;
        Assert(ichild < fill);
        if (ichild + 1 == fill)
            return ichild;
        return !(elems[ichild] < elems[ichild + 1]) ? ichild : ichild + 1;
    }

    inline bool CheckIsHeap() const
    {
        for (int i = 0; i < fill; i++) {
            if (!IsChildSmallerEqual(i, 0))
                return false;
            if (!IsChildSmallerEqual(i, 1))
                return false;
        }
        return true;
    }

    inline void TrickleDown(int idx)
    {
        if (ISLIST) {
            // just use linear scan, not worth using a heap
            while (idx + 1 < fill) {
                if (elems[idx] < elems[idx + 1]) {
                    std::swap(elems[idx], elems[idx + 1]);
                    idx++;
                } else {
                    break;
                }
            }
            Assert(CheckIsFullySorted());
        } else {
            // move element down by swapping until heap property is satisfied
            while (idx < fill) {
                if (!IsChildSmallerEqual(idx, 0) || !IsChildSmallerEqual(idx, 1)) {
                    int idx2 = LargerChildIndex(idx);
                    std::swap(elems[idx], elems[idx2]);
                    idx = idx2;
                } else {
                    break;
                }
            }
            Assert(CheckIsHeap());
        }
    }

    inline void TrickleUp(int idx)
    {
        if (ISLIST) {
            // just use linear scan, not worth using heap
            while (idx > 0) {
                if (elems[idx - 1] < elems[idx]) {
                    std::swap(elems[idx - 1], elems[idx]);
                    idx--;
                } else {
                    break;
                }
            }
            Assert(CheckIsFullySorted());
        } else {
            // move element up until heap property is satisfied
            while (idx > 0) {
                int iparent = (idx - 1) >> 1;
                if (!(elems[iparent] < elems[idx]))
                    break;
                std::swap(elems[idx], elems[iparent]);
                idx = iparent;
            }
            Assert(CheckIsHeap());
        }
    }
};

template<typename T, int K, typename IDT>
class KDTree {
public:
    struct Result
    {
        T distSqr;
        IDT id;

        bool operator<(const Result& b) const { return distSqr < b.distSqr; }
    };

    void Reset(int numExpected)
    {
        static_assert(K < AXISISLEAF, "K to large. Increase AXISBITS if needed.");
        points.clear();
        points.reserve(numExpected);
    }

    void AddElements(const T* values, const IDT* ids, int n)
    {
        Assert(MAXENTRIES > n + points.size());
        size_t i0 = points.size();
        points.resize(i0 + n);
        for (int i = 0; i < n; i++) {
            CopyPoint(points[i + i0].p, values + i * K);
            points[i + i0].id = ids[i];
        }
    }

    size_t GetSize() { return points.size(); }

    size_t GetNodeCount() { return nodes.size(); }

    void Build(int nLeafElements = 4)
    {
        if (points.empty())
            return;
        nodes.clear();
        nodes.reserve(2 * points.size());
        nodes.push_back({0, 0});
        SplitNodeRec(0, (int)points.size(), 0, nLeafElements);
        Assert(Check());
    }

    Result QueryClosest(const T* point, T maxDistSqr, T minDistSqr, IDT noneId)
    {
        Result r{maxDistSqr, noneId};
        if (nodes.empty())
            return r;
        ClosestRec(point, 0, maxDistSqr, r.id, minDistSqr);
        r.distSqr = maxDistSqr;
        return r;
    }

    Result QueryClosestRef(const T* point, T maxDistSqr, T minDistSqr, IDT noneId)
    {
        Result r{maxDistSqr, noneId};
        for (size_t i = 0; i < points.size(); i++) {
            T d = DistSqr(point, points[i].p);
            if (d < r.distSqr && d >= minDistSqr) {
                r.distSqr = d;
                r.id = points[i].id;
            }
        }
        return r;
    }

    int QueryNClosest(const T* point, T maxDistSqr, T minDistSqr, int n, Result* dest)
    {
        if (nodes.empty())
            return 0;
        if ( n<=32 ) { // use list for small n
            NPriorityQueue<Result, true> results(n, dest);
            NClosestRec(point, 0, maxDistSqr, results, minDistSqr);
            results.SortResult();
            return results.Fill();
        } else { // use heap for larger n 
            NPriorityQueue<Result, false> results(n, dest);
            NClosestRec(point, 0, maxDistSqr, results, minDistSqr);
            results.SortResult();
            return results.Fill();
        }
    }

    int QueryNClosestRef(const T* point, T maxDistSqr, T minDistSqr, int n, Result* dest)
    {
        NPriorityQueue<Result, false> results(n, dest);
        for (size_t i = 0; i < points.size(); i++) {
            Result r;
            r.id = points[i].id;
            r.distSqr = DistSqr(point, points[i].p);
            if (r.distSqr < maxDistSqr && r.distSqr >= minDistSqr)
                results.Enqueue(r);
        }
        results.SortResult();
        return results.Fill();
    }

private:
    static const int AXISBITS = 4;
    static const int MAXENTRIES = (1 << (30 - AXISBITS)) - 1;
    static const int AXISISLEAF = (1 << AXISBITS) - 1;

    inline void static CopyPoint(T* dest, const T* src) { memcpy(dest, src, sizeof(T) * K); }

    inline void GetBounds(int istart, int iend, T* minB, T* maxB) const
    {
        Assert(istart < iend);
        CopyPoint(minB, points[istart].p);
        CopyPoint(maxB, minB);
        for (int i = istart + 1; i < iend; i++) {
            for (int k = 0; k < K; k++) {
                minB[k] = std::min(minB[k], points[i].p[k]);
                maxB[k] = std::max(maxB[k], points[i].p[k]);
            }
        }
    }

    inline void WriteLeaf(int istart, int iend, int inode)
    {
        // write leaf, done
        Node n;
        n.axis = AXISISLEAF | ((iend - istart) << AXISBITS); // axis and leaf element count
        n.firstLeafElement = istart;
        nodes[inode] = n;
    }

    inline int static MaxAxis(const T* d)
    {
        T maxval = d[0];
        int maxi = 0;
        for (int i = 1; i < K; i++) {
            if (d[i] > maxval) {
                maxval = d[i];
                maxi = i;
            }
        }
        return maxi;
    }

    // For some reason, one of our builds errors out on std::abs, seemingly
    // trying to use the one in the global namespace. Easier to just add this workarond.
    template<class V>
    inline static V AbsKD(V a) {
        return a > 0 ? a : -a;
    }

    inline static void AbsDiff(T* dest, const T* a, const T* b)
    {
        for (int i = 0; i < K; i++)
            dest[i] = AbsKD(a[i] - b[i]);
    }

    inline static bool PointInBounds(const T* p, const T* minB, const T* maxB)
    {
        for (int i = 0; i < K; i++)
            if (!(p[i] >= minB[i]) || !(p[i] <= maxB[i]))
                return false;
        return true;
    }

    bool CheckNodeRec(int inode, const T* minB, const T* maxB) const
    {
        const Node& n = nodes[inode];
        int axis = n.axis & AXISISLEAF;
        if (axis == AXISISLEAF) {
            int istart = n.firstLeafElement;
            int iend = istart + (n.numLeafElements >> AXISBITS);
            for (int i = istart; i < iend; i++) {
                if (!PointInBounds(points[i].p, minB, maxB))
                    return false;
            }
        } else {
            int ichild = n.childrenIdx >> AXISBITS;
            T leftMaxB[K], rightMinB[K];
            CopyPoint(leftMaxB, maxB);
            CopyPoint(rightMinB, minB);
            leftMaxB[axis] = n.p;
            rightMinB[axis] = n.p;
            if (!CheckNodeRec(ichild, minB, leftMaxB))
                return false;
            if (!CheckNodeRec(ichild + 1, rightMinB, maxB))
                return false;
        }
        return true;
    }

    bool Check() const
    {
        if (nodes.empty())
            return true;
        T minB[K], maxB[K];
        GetBounds(0, (int)points.size(), minB, maxB);
        return CheckNodeRec(0, minB, maxB);
    }

    void SplitNodeRec(int istart, int iend, int inode, int numInLeaf)
    {
        Assert(istart <= iend);
        if (istart + numInLeaf >= iend) {
            WriteLeaf(istart, iend, inode);
            return;
        }
        // find bounds and axis
        T minB[K], maxB[K];
        GetBounds(istart, iend, minB, maxB);
        T dAbs[K];
        AbsDiff(dAbs, maxB, minB);
        int axis = MaxAxis(dAbs);
        if (dAbs[axis] <= T(0)) {
            WriteLeaf(istart, iend, inode);
            return;
        }
        // sort (direct)
        std::sort(points.begin() + istart, points.begin() + iend,
                  [axis](const Element& a, const Element& b) { return (a.p[axis] < b.p[axis]); });

        // find median - but not an equal one
        int imed = (istart + iend) >> 1;
        Assert(imed != istart);
        while (imed < iend && points[imed].p[axis] == points[imed - 1].p[axis])
            imed++;
        if (imed == iend) { // could happen for very unbalanced, do not write zero nodes
            WriteLeaf(istart, iend, inode);
            return;
        }
        T splitval = (points[imed].p[axis] + points[imed - 1].p[axis]) / T(2);
        // create node
        int ichild = (int)nodes.size();
        nodes.push_back({0, 0});
        nodes.push_back({0, 0});
        Node n;
        n.axis = axis | (ichild << AXISBITS); // axis and leaf element count
        n.p = splitval;
        nodes[inode] = n;
        // recurse
        SplitNodeRec(istart, imed, ichild, numInLeaf);
        SplitNodeRec(imed, iend, ichild + 1, numInLeaf);
    }

    inline T static DistSqr(const T* a, const T* b)
    {
        T r = T(0);
        for (int i = 0; i < K; i++) {
            T d = a[i] - b[i];
            r += d * d;
        }
        return r;
    }

    void ClosestRec(const T* point, int inode, T& maxDistSqr, IDT& best, T minDistSqr) const
    {
        const Node& n = nodes[inode];
        int axis = n.axis & AXISISLEAF;
        if (axis == AXISISLEAF) {
            // leaf - check all elements, adjust closest
            int istart = n.firstLeafElement;
            int iend = istart + (n.numLeafElements >> AXISBITS);
            for (int i = istart; i < iend; i++) {
                T dsq = DistSqr(points[i].p, point);
                if (dsq < maxDistSqr && dsq >= minDistSqr) {
                    maxDistSqr = dsq;
                    best = points[i].id;
                }
            }
        } else {
            Assert(axis < K);
            // node, recurse closer first
            T d = point[axis] - n.p;
            int ichild = n.childrenIdx >> AXISBITS;
            if (d < T(0)) {
                // left first
                ClosestRec(point, ichild, maxDistSqr, best, minDistSqr);
                if (d * d <= maxDistSqr) // can skip right only if d > maxdist
                    ClosestRec(point, ichild + 1, maxDistSqr, best, minDistSqr);
            } else {
                // right first
                ClosestRec(point, ichild + 1, maxDistSqr, best, minDistSqr);
                if (d * d <= maxDistSqr) // can skip left only if d > maxdist
                    ClosestRec(point, ichild, maxDistSqr, best, minDistSqr);
            }
        }
    }

    template<bool ISLIST>
    void NClosestRec(const T* point, int inode, T maxDistSqr, NPriorityQueue<Result, ISLIST>& best, T minDistSqr) const
    {
        const Node& n = nodes[inode];
        int axis = n.axis & AXISISLEAF;
        if (axis == AXISISLEAF) {
            // leaf - check all elements, adjust closest
            int istart = n.firstLeafElement;
            int iend = istart + (n.numLeafElements >> AXISBITS);
            for (int i = istart; i < iend; i++) {
                Result r;
                r.distSqr = DistSqr(points[i].p, point);
                if (r.distSqr >= minDistSqr && r.distSqr < maxDistSqr) {
                    r.id = points[i].id;
                    best.Enqueue(r);
                }
            }
        } else {
            // node, recurse closer first
            Assert(axis < K);
            T d = point[axis] - n.p;
            int ichild = n.childrenIdx >> AXISBITS;
            if (d < T(0)) {
                // left first
                NClosestRec(point, ichild, maxDistSqr, best, minDistSqr);
                if (!best.Full() || d * d <= best.Peek().distSqr) // can skip right only if d > maxdist
                    NClosestRec(point, ichild + 1, maxDistSqr, best, minDistSqr);
            } else {
                // right first
                NClosestRec(point, ichild + 1, maxDistSqr, best, minDistSqr);
                if (!best.Full() || d * d <= best.Peek().distSqr) // can skip left only if d > maxdist
                    NClosestRec(point, ichild, maxDistSqr, best, minDistSqr);
            }
        }
    }

    struct Element
    {
        T p[K];
        IDT id;
    };

    struct Node
    {
        union {
            uint32_t childrenIdx;     // only if node, index to children, shifted by AXISBITS
            uint32_t axis;            // lowest AXISBITS bits: axis for split for nodes, AXISISLEAF if leaf
            uint32_t numLeafElements; // only if leaf: number of elements in leaf, shifted by AXISBITS
        };
        union {
            T p;                       // only if node: value to split along axis
            uint32_t firstLeafElement; // only if leaf: index into array of elements
        };
    };

    std::vector<Element> points;
    std::vector<Node> nodes;
};
