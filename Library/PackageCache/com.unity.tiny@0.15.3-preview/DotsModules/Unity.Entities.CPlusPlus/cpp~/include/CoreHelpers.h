#pragma once

#include <cstring>
#include <stdio.h>
#include <algorithm>

namespace Unity {
    namespace Tiny {
        namespace Core {

            //Copies the first "count" elements from src to dest. And adds a null terminated character to dest. Dest must be count + 1 size
            //This is because C# strings coming from dynamic buffers have a default internal capacity of 64 and no null terminated characters which results in incorrect text displayed in javascript
            inline void copyBufferAndAppendNull(uint16_t* dest, uint16_t* src, int count) {
                std::memcpy(static_cast<uint16_t*>(dest), static_cast<uint16_t*>(src), count * sizeof(uint16_t));
                dest[count] = (uint16_t)'\0';
            }
        }
    }
}
