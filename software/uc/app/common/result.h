#ifndef _RESULT_H_
#define _RESULT_H_

#include <stdbool.h>

// clang-format off

typedef enum {
  R_Pending             = 1,
  R_Success             = 0,
  R_ErrorGeneric        = -1,
  R_ErrorTimeout        = -2,
  R_ErrorCmd            = -3,
  R_ErrorNotImplemented = -4,
} Result;

inline bool ResIsError(Result r) { return (r != R_Success) ? true : false; }
inline bool ResSuccess(Result r) { return (r == R_Success) ? true : false; }

#endif
