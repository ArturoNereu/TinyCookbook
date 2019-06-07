// KDTree doesn't have include files, so warm
// up the needed includes here, before including KDTree
#include <map>
#include <vector>
#include <algorithm>
#include <mutex>
#include <assert.h>
#define Assert assert
#include "KDTree.h"

#include <math.h>
#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#include "zeroplayer.h"

// #define WRITE_LOG

uint32_t XORShift()
{
    static uint32_t x = 1;
	x ^= x << 13;
	x ^= x >> 17;
	x ^= x << 5;
	return x;
}

float RandomRange(float x, float y) {
    return x + (y - x) * float(XORShift() % 1001) / 1000.f;
}

struct Vector3f {
    Vector3f() {}
    Vector3f(float _x, float _y, float _z) : x(_x), y(_y), z(_z) {}
    float& operator[](int i) { return *(&x + i); }

    float x, y, z;
};

static Vector3f
RandPoint()
{
    Vector3f p;
    p.x = RandomRange(-100.0f, 100.0f);
    p.y = RandomRange(-100.0f, 100.0f);
    p.z = RandomRange(-100.0f, 100.0f);
    return p;
}

typedef KDTree<float, 3, int> Tree3;
static const int NONE = -10000;
static const float FAR = 1000.0f * 1000.0f;

#define REQUIRE(x) { if (!(x)) return false; }

bool ResultEqual(const Tree3::Result& a, const Tree3::Result& b)
{
    return ((a.id == b.id) && fabs(a.distSqr - b.distSqr) < 0.01f);
}

ZEROPLAYER_EXPORT
bool ZEROPLAYER_CALL testKDTree()
{
    {
        Tree3 tree;
        tree.Reset(10000);

        for (int i = 0; i < 10000; i++) {
            Vector3f p = RandPoint();
            tree.AddElements(&p.x, &i, 1);
        }
        tree.Build();

        for (int i = 0; i < 100; i++) {
            Vector3f p = RandPoint();
            Tree3::Result pi = tree.QueryClosest(&p.x, FAR, 0, NONE);
            Tree3::Result piref = tree.QueryClosestRef(&p.x, FAR, 0, NONE);

            REQUIRE(ResultEqual(pi, piref));
        }

        Tree3::Result resultArr0[10];
        Tree3::Result resultArr1[10];

        for (int i = 0; i < 100; i++) {
            Vector3f p = RandPoint();
            int ni = tree.QueryNClosest(&p.x, FAR, 0, 10, resultArr0);
            int niref = tree.QueryNClosestRef(&p.x, FAR, 0, 10, resultArr1);

            REQUIRE(ni == 10);
            REQUIRE(niref == 10);

            for(int j=0; j<10; ++j) {
                REQUIRE(ResultEqual(resultArr0[j], resultArr1[j]));
            }
        }
    }

    {
        Tree3 tree;

        // uniform in axis only
        for (int axis = 0; axis < 3; axis++) {
            for (int i = -100; i < 100; i++) {
                Vector3f p(0, 0, 0);
                p[axis] = (float)i;
                tree.AddElements(&p.x, &i, 1);
            }
            tree.Build();
            for (int i = -10; i < 10; i++) {
                Vector3f p(0, 0, 0);
                p[axis] = (float)i * .71f;
                Tree3::Result pi = tree.QueryClosest(&p.x, FAR, 0, NONE);
                Tree3::Result piref = tree.QueryClosestRef(&p.x, FAR, 0, NONE);
                REQUIRE(ResultEqual(pi, piref));
            }
        }
        static const int k = 10000;
        static const int m = 1000;

        tree.Reset(k);
        for (int i = 0; i < k; i++) {
            Vector3f p = RandPoint();
            tree.AddElements(&p.x, &i, 1);
        }
        tree.Build();

        for (int i = 0; i < m; i++) {
            Vector3f p = RandPoint();
            Tree3::Result pi = tree.QueryClosest(&p.x, FAR, 0, NONE);
            Tree3::Result piref = tree.QueryClosestRef(&p.x, FAR, 0, NONE);
            REQUIRE(ResultEqual(pi, piref));
        }

        static const int l = 10;
        static const int nl = 80;

        Tree3::Result resultArr0[nl];
        Tree3::Result resultArr1[nl];

        for (int i = 0; i < l; i++) {
            Vector3f p = RandPoint();
            tree.QueryNClosest(&p.x, FAR, 0, nl, resultArr0);
            tree.QueryNClosestRef(&p.x, FAR, 0, nl, resultArr1);
            for(int j=0; j<nl; ++j) {
                REQUIRE(ResultEqual(resultArr0[j], resultArr1[j]));
             }
        }

        // perf
        std::chrono::high_resolution_clock cl;

        static const int k2 = 10000;
        static const int m2 = 1000;
        static const int m2scale = 10;
        static const int nq = 16;

        Tree3::Result resultArr[nq];

        tree.Reset(k2);

        for (int i = 0; i < k2; i++) {
            Vector3f p = RandPoint();
            tree.AddElements(&p.x, &i, 1);
        }
        auto t0 = cl.now();
        tree.Build();
        auto t1 = cl.now();
        for (int i = 0; i < m2; i++) {
            Vector3f p = RandPoint();
            Tree3::Result pi = tree.QueryClosest(&p.x, FAR, 0, NONE);
            REQUIRE(pi.id >= 0);
        }
        auto t2 = cl.now();
        for (int i = 0; i < m2 / m2scale; i++) {
            Vector3f p = RandPoint();
            Tree3::Result piref = tree.QueryClosestRef(&p.x, FAR, 0, NONE);
            REQUIRE(piref.id >= 0);
        }
        auto t3 = cl.now();
        for (int i = 0; i < m2; i++) {
            Vector3f p = RandPoint();
            int n = tree.QueryNClosest(&p.x, FAR, 0, nq, resultArr);
            REQUIRE(n == nq);
            for (int i = 0; i < nq; i++) {
                REQUIRE(resultArr[i].id != NONE);
                REQUIRE(resultArr[i].distSqr < FAR);
            }
        }
        auto t4 = cl.now();

        int dt0 = (int)std::chrono::duration_cast<std::chrono::milliseconds>(t1 - t0).count();
        int dt1 = (int)std::chrono::duration_cast<std::chrono::milliseconds>(t2 - t1).count();
        int dt2 = (int)std::chrono::duration_cast<std::chrono::milliseconds>(t3 - t2).count() * m2scale;
        int dt3 = (int)std::chrono::duration_cast<std::chrono::milliseconds>(t4 - t3).count();

        #ifdef WRITE_LOG
        printf("KDTree (PointQuery) perf test.\n");
        printf("%i points, %i querys. build: %ims query: %ims ref-query/%i: %ims n%i-query:%i, \n", 
                k2, m2, dt0,
                dt1, m2scale, dt2, nq, dt3);
        #endif
        REQUIRE(dt1 < dt2);       // query must be faster than ref REQUIRE(dt1 + dt0 < dt2);
        REQUIRE(dt1 + dt0 < dt2); // query + build must be faster than ref
    }
    return true;
}
