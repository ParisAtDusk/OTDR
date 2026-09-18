#ifndef _RESULT_H_
#define _RESULT_H_

#include <stdbool.h>

// clang-format off

typedef enum {
  R_Pending               = 1,
  R_Success               = 0,
  R_ErrorGeneric          = -1,
  R_ErrorTimeout          = -2,
  R_ErrorCmd              = -3,
  R_ErrorNotImplemented   = -4,
  R_ErrorSocketCreate     = -5,
  R_ErrorResolveAddress   = -6,
  R_ErrorConnect          = -7,
  R_ErrorNotOpen          = -8,
  R_ErrorSend             = -9,
  R_ErrorClosed           = -10,
  R_ErrorReceive          = -11,
  R_ErrorInvalidArgument  = -12,
  R_ErrorBind             = -13,
  R_ErrorAccept           = -14,
  R_ErrorListen           = -15,
  R_ErrorInit             = -16
} Result;

static inline bool ResIsError(Result r) { return (r != R_Success) ? true : false; }
static inline bool ResSuccess(Result r) { return (r == R_Success) ? true : false; }

#endif
